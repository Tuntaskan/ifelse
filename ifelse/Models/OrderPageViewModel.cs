using System.Collections.Generic;

namespace ifelse.Models
{
    public class OrderPageViewModel
    {
        public int? LastOrderId { get; set; }
        public List<Order> Orders { get; set; } = new();

        public List<MenuModel> Menus { get; set; } = new List<MenuModel>();

        public List<CartItem> Cart { get; set; } = new List<CartItem>();
        public List<TableMeja> Tables { get; set; } = new();
    }
}