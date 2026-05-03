using System.ComponentModel.DataAnnotations;
namespace ifelse.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }

        public int? TableId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string PaymentStatus { get; set; } = "Unpaid";

        public string OrderStatus { get; set; } = "Waiting";

        public ICollection<OrderDetail> OrderDetails { get; set; }
            = new List<OrderDetail>();
    }

    public class OrderDetail
    {
        [Key]
        public int DetailId { get; set; }

        public int OrderId { get; set; }

        public int MenuId { get; set; }

        public int Qty { get; set; }
        public decimal Price { get; set; }

        public decimal Subtotal { get; set; }

        public Order? Order { get; set; }
    }
}