namespace Brewed.DataContext.Dtos
{
    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int MonthlyOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<TopProductDto> TopProducts { get; set; }
        public List<RecentOrderDto> RecentOrders { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenueChart { get; set; }
        public List<CategorySalesDto> CategorySales { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class CategorySalesDto
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalItemsSold { get; set; }
    }

    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public int CurrentStock { get; set; }
        public int SoldLast30Days { get; set; }
    }

    public class CustomerStatsDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
    }
}