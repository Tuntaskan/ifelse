namespace ifelse.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } // Waiting, Cooking, Done
    }

    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int MenuId { get; set; }

        public int Qty { get; set; }

        public decimal Subtotal { get; set; }
    }
}
