using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public enum BillStatus
    {
        Pending,
        PartiallyPaid,
        Paid
    }

    public class Bill
    {
        public Guid BillId { get; set; } = Guid.NewGuid();

        public Guid VisitId { get; set; }

        [ForeignKey(nameof(VisitId))]
        public Visit? Visit { get; set; }

        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient? Patient { get; set; }

        public string? CashierId { get; set; }

        [ForeignKey(nameof(CashierId))]
        public ApplicationUser? Cashier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance => TotalAmount - PaidAmount;

        public BillStatus Status { get; set; } = BillStatus.Pending;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        public ICollection<BillItem> Items { get; set; } = new List<BillItem>();
    }
}
