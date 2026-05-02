using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ifelse.Models
{
    public enum MenuCategory
    {
        MainCourse,
        Snack,
        Dessert,
        Drink
    }
    public class MenuModel
    {
        [Key]
        public int MenuId { get; set; }
        [Required]
        public string MenuName { get; set; } = null!;
        [Required]
        public MenuCategory CategoryId { get; set; }
        [Required]
        public decimal Price { get; set; } = decimal.Zero;
        [Required]
        public int Stock { get; set; }
        public string Photo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Status { get; set; } = true;

    }
}
