using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ifelse.Models
{
    public class BrokenItemReport
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int InventoryId { get; set; }

        [ForeignKey("InventoryId")]
        public Inventory? Inventory { get; set; }

        [Required]
        public int QtyBroken { get; set; }

        public string? BrokenBy { get; set; }

        public string? ReporterRole { get; set; }

        public string? Notes { get; set; }

        public DateTime? ReportDate { get; set; } = DateTime.Now;
    }
}
