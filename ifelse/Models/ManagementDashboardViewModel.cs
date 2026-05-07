using System;
using System.Collections.Generic;
using System.Linq;

namespace ifelse.Models
{
    public class ManagementDashboardViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public List<MenuModel> Menus { get; set; } = new();
        public List<Inventory> Inventory { get; set; } = new();
        public List<BrokenItemReport> BrokenReports { get; set; } = new();
        public Inventory NewInventory { get; set; } = new();
        public BrokenItemReport NewBrokenReport { get; set; } = new();
        public DateTime? SalesDate { get; set; }

        public IEnumerable<Order> SalesOrders => Orders
            .Where(x => string.Equals(x.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            .Where(x => !SalesDate.HasValue || x.OrderDate.Date == SalesDate.Value.Date);

        public decimal TotalSales => SalesOrders.Sum(x => x.TotalPrice);

        public int TotalOrders => Orders.Count;

        public int PaidOrders => SalesOrders.Count();

        public int WaitingOrders => Orders.Count(x =>
            string.Equals(x.OrderStatus, "Waiting", StringComparison.OrdinalIgnoreCase));
    }

    public class KitchenDashboardViewModel
    {
        public List<Order> Orders { get; set; } = new();
    }
}
