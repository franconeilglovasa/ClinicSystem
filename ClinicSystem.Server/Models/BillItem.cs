using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public enum BillItemCategory
    {
        Consultation,
        Laboratory,
        Procedure,
        Medication,
        Other
    }

    public class BillItem
    {
        [Key] 
        public Guid ItemId { get; set; } = Guid.NewGuid();

        public Guid BillId { get; set; }

        [ForeignKey(nameof(BillId))]
        public Bill? Bill { get; set; }

        [Required, MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        public BillItemCategory Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
