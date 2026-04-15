using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ifelse.Models
{
    public class MenuModel
    {
        [Key]
        public int IdMenu { get; set; }
        [Required]
        public string NamaMenu { get; set; } = null!;
        [Required]
        public decimal Harga { get; set; } = decimal.Zero;
        [Required]
        public string Kategori { get; set; } = null!;
        public string Deskripsi { get; set; } = string.Empty;
        [Required]
        public bool isAvaible { get; set; } = true;

    }
}
