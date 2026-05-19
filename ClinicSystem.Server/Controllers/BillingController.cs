using ClinicSystem.Server.Data;
using ClinicSystem.Server.DTOs.Billing;
using ClinicSystem.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicSystem.Server.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BillingController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("visits/{visitId}/bill")]
        public async Task<IActionResult> GetBill(Guid visitId)
        {
            var bill = await _db.Bills
                .Include(b => b.Patient)
                .Include(b => b.Cashier)
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.VisitId == visitId);

            if (bill == null) return NotFound();
            return Ok(MapToDto(bill));
        }

        [HttpGet("bills")]
        [Authorize(Roles = "Admin,Cashier")]
        public async Task<IActionResult> GetBills([FromQuery] string? status)
        {
            var query = _db.Bills
                .Include(b => b.Patient)
                .Include(b => b.Cashier)
                .Include(b => b.Items)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BillStatus>(status, true, out var s))
                query = query.Where(b => b.Status == s);

            var bills = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return Ok(bills.Select(MapToDto));
        }

        [HttpPost("visits/{visitId}/bill")]
        [Authorize(Roles = "Admin,Cashier")]
        public async Task<IActionResult> CreateBill(Guid visitId, [FromBody] CreateBillRequest request)
        {
            var visit = await _db.Visits.FindAsync(visitId);
            if (visit == null) return NotFound(new { message = "Visit not found." });

            var existing = await _db.Bills.FirstOrDefaultAsync(b => b.VisitId == visitId);
            if (existing != null) return BadRequest(new { message = "Bill already exists for this visit." });

            var cashierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            var items = request.Items.Select(i => new BillItem
            {
                Description = i.Description,
                Category = Enum.TryParse<BillItemCategory>(i.Category, true, out var cat) ? cat : BillItemCategory.Other,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList();

            var bill = new Bill
            {
                VisitId = visitId,
                PatientId = visit.PatientId,
                CashierId = cashierId,
                Notes = request.Notes,
                TotalAmount = items.Sum(i => i.UnitPrice * i.Quantity),
                Items = items
            };

            _db.Bills.Add(bill);
            visit.Status = VisitStatus.ForBilling;
            await _db.SaveChangesAsync();

            var created = await _db.Bills
                .Include(b => b.Patient).Include(b => b.Cashier).Include(b => b.Items)
                .FirstAsync(b => b.BillId == bill.BillId);

            return CreatedAtAction(nameof(GetBill), new { visitId }, MapToDto(created));
        }

        [HttpPost("bills/{billId}/items")]
        [Authorize(Roles = "Admin,Cashier")]
        public async Task<IActionResult> AddItem(Guid billId, [FromBody] AddBillItemRequest request)
        {
            var bill = await _db.Bills.Include(b => b.Items).FirstOrDefaultAsync(b => b.BillId == billId);
            if (bill == null) return NotFound();

            var item = new BillItem
            {
                BillId = billId,
                Description = request.Description,
                Category = Enum.TryParse<BillItemCategory>(request.Category, true, out var cat) ? cat : BillItemCategory.Other,
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity
            };

            bill.Items.Add(item);
            bill.TotalAmount = bill.Items.Sum(i => i.UnitPrice * i.Quantity);
            await _db.SaveChangesAsync();
            return Ok(new BillItemDto
            {
                ItemId = item.ItemId,
                Description = item.Description,
                Category = item.Category.ToString(),
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.UnitPrice * item.Quantity
            });
        }

        [HttpDelete("bills/{billId}/items/{itemId}")]
        [Authorize(Roles = "Admin,Cashier")]
        public async Task<IActionResult> RemoveItem(Guid billId, Guid itemId)
        {
            var bill = await _db.Bills.Include(b => b.Items).FirstOrDefaultAsync(b => b.BillId == billId);
            if (bill == null) return NotFound();

            var item = bill.Items.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null) return NotFound();

            bill.Items.Remove(item);
            bill.TotalAmount = bill.Items.Sum(i => i.UnitPrice * i.Quantity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("bills/{billId}/payment")]
        [Authorize(Roles = "Admin,Cashier")]
        public async Task<IActionResult> RecordPayment(Guid billId, [FromBody] RecordPaymentRequest request)
        {
            var bill = await _db.Bills.FindAsync(billId);
            if (bill == null) return NotFound();

            bill.PaidAmount += request.Amount;
            if (bill.PaidAmount >= bill.TotalAmount)
            {
                bill.PaidAmount = bill.TotalAmount;
                bill.Status = BillStatus.Paid;
                bill.PaidAt = DateTime.UtcNow;

                var visit = await _db.Visits.FindAsync(bill.VisitId);
                if (visit != null) visit.Status = VisitStatus.Completed;
            }
            else
            {
                bill.Status = BillStatus.PartiallyPaid;
            }

            await _db.SaveChangesAsync();
            return Ok(new { bill.TotalAmount, bill.PaidAmount, Balance = bill.TotalAmount - bill.PaidAmount, Status = bill.Status.ToString() });
        }

        private static BillDto MapToDto(Bill b) => new()
        {
            BillId = b.BillId,
            VisitId = b.VisitId,
            PatientId = b.PatientId,
            PatientName = b.Patient != null ? b.Patient.FirstName + " " + b.Patient.LastName : string.Empty,
            CashierId = b.CashierId,
            CashierName = b.Cashier?.FullName,
            TotalAmount = b.TotalAmount,
            PaidAmount = b.PaidAmount,
            Balance = b.TotalAmount - b.PaidAmount,
            Status = b.Status.ToString(),
            Notes = b.Notes,
            CreatedAt = b.CreatedAt,
            PaidAt = b.PaidAt,
            Items = b.Items?.Select(i => new BillItemDto
            {
                ItemId = i.ItemId,
                Description = i.Description,
                Category = i.Category.ToString(),
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                TotalPrice = i.UnitPrice * i.Quantity
            }).ToList() ?? new()
        };
    }
}
