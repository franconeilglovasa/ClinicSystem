namespace ClinicSystem.Server.DTOs.Billing
{
    public class BillItemDto
    {
        public Guid ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class BillDto
    {
        public Guid BillId { get; set; }
        public Guid VisitId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? CashierId { get; set; }
        public string? CashierName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public List<BillItemDto> Items { get; set; } = new();
    }

    public class CreateBillRequest
    {
        public string? Notes { get; set; }
        public List<CreateBillItemRequest> Items { get; set; } = new();
    }

    public class CreateBillItemRequest
    {
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class RecordPaymentRequest
    {
        public decimal Amount { get; set; }
    }

    public class AddBillItemRequest
    {
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
