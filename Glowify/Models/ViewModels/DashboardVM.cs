namespace Glowify.Models.ViewModels
{
    public class DashboardVM
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public double TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public IEnumerable<TopProductVM> TopProducts { get; set; }
    }

    public class TopProductVM
    {
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
    }
}