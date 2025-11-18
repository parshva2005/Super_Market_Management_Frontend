namespace Super_Market_Management.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public decimal TodaySales { get; set; }
        public int NewOrders { get; set; }
        public int LowStockItems { get; set; }
        public List<RecentOrder> RecentOrders { get; set; } = new List<RecentOrder>();
        public List<TopSellingProduct> TopSellingProducts { get; set; } = new List<TopSellingProduct>();
    }

    public class RecentOrder
    {
        public string OrderId { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TopSellingProduct
    {
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int SalesCount { get; set; }
    }
}
