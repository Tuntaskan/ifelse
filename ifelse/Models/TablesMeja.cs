using System.ComponentModel.DataAnnotations;

namespace ifelse.Models
{
    public class TableMeja
    {
        [Key]
        public int TableId { get; set; }

        public int TableNumber { get; set; }

        public string? QRCode { get; set; }

        public string Status { get; set; } = "Available";
    }
}