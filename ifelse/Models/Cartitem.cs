namespace ifelse.Models
{
    public class CartItem
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Qty { get; set; }

        public decimal Subtotal => Price * Qty;
    }
}