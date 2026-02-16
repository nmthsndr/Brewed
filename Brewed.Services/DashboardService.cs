using Microsoft.EntityFrameworkCore;
using Brewed.DataContext.Context;
using Brewed.DataContext.Dtos;

namespace Brewed.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 10);
        Task<List<CustomerStatsDto>> GetTopCustomersAsync(int count = 10);
    }

    public class DashboardService : IDashboardService
    {
        private readonly BrewedDbContext _context;

        public DashboardService(BrewedDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            // Total Revenue
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "Delivered")
                .SumAsync(o => o.TotalAmount);

            // Monthly Revenue
            var monthlyRevenue = await _context.Orders
                .Where(o => o.Status == "Delivered" && o.OrderDate >= monthStart)
                .SumAsync(o => o.TotalAmount);

            // Total Orders
            var totalOrders = await _context.Orders.CountAsync();

            // Monthly Orders
            var monthlyOrders = await _context.Orders
                .Where(o => o.OrderDate >= monthStart)
                .CountAsync();

            // Total Customers
            var totalCustomers = await _context.Users
                .Where(u => u.Role == "RegisteredUser" && !u.IsDeleted)
                .CountAsync();

            // Total Products
            var totalProducts = await _context.Products.CountAsync();

            // Low Stock Products (< 10)
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity < 10)
                .CountAsync();

            // Average Order Value
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // Top 5 Products
            var topProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Product != null)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.ImageUrl })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    ImageUrl = g.Key.ImageUrl,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(5)
                .ToListAsync();

            // Recent 10 Orders
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.User != null)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User.Name,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            // Monthly Revenue Chart (Last 6 months)
            var sixMonthsAgo = now.AddMonths(-6);
            var monthlyRevenueData = await _context.Orders
                .Where(o => o.Status == "Delivered" && o.OrderDate >= sixMonthsAgo)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .ToListAsync();
            var monthlyRevenueChart = monthlyRevenueData
               .Select(m => new MonthlyRevenueDto
               {
                   Month = $"{m.Year}-{m.Month:D2}",
                   Revenue = m.Revenue,
                   OrderCount = m.OrderCount
               })
               .OrderBy(m => m.Month)
               .ToList();


            // Category Sales
            var categorySales = await _context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(oi => oi.Product != null && oi.Product.Category != null)
                .GroupBy(oi => oi.Product.Category.Name)
                .Select(g => new CategorySalesDto
                {
                    CategoryName = g.Key,
                    ProductCount = g.Select(oi => oi.ProductId).Distinct().Count(),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice),
                    TotalItemsSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(c => c.TotalRevenue)
                .ToListAsync();

            return new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                MonthlyRevenue = monthlyRevenue,
                TotalOrders = totalOrders,
                MonthlyOrders = monthlyOrders,
                TotalCustomers = totalCustomers,
                TotalProducts = totalProducts,
                LowStockProducts = lowStockProducts,
                AverageOrderValue = averageOrderValue,
                TopProducts = topProducts,
                RecentOrders = recentOrders,
                MonthlyRevenueChart = monthlyRevenueChart,
                CategorySales = categorySales
            };
        }

        public async Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 10)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity <= threshold)
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    ImageUrl = p.ImageUrl,
                    CurrentStock = p.StockQuantity,
                    SoldLast30Days = p.OrderItems
                        .Where(oi => oi.Order.OrderDate >= thirtyDaysAgo)
                        .Sum(oi => oi.Quantity)
                })
                .OrderBy(p => p.CurrentStock)
                .ToListAsync();

            return lowStockProducts;
        }

        public async Task<List<CustomerStatsDto>> GetTopCustomersAsync(int count = 10)
        {
            var topCustomers = await _context.Users
                .Where(u => u.Role == "RegisteredUser" && !u.IsDeleted)
                .Select(u => new CustomerStatsDto
                {
                    UserId = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    TotalOrders = u.Orders.Count,
                    TotalSpent = u.Orders
                        .Where(o => o.Status == "Delivered")
                        .Sum(o => o.TotalAmount),
                    LastOrderDate = u.Orders
                        .OrderByDescending(o => o.OrderDate)
                        .Select(o => o.OrderDate)
                        .FirstOrDefault()
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(count)
                .ToListAsync();

            return topCustomers;
        }
    }
}