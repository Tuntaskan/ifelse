using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ifelse.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }

        [Required]
        public string ItemName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        [Required]
        public int Stock { get; set; }

        public string Unit { get; set; } = string.Empty;

        public string? Condition { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;


        // Relasi
        public ICollection<BrokenItemReport> BrokenReports { get; set; } = new List<BrokenItemReport>();
    }
}
