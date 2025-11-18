using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Super_Market_Management.Models;

namespace Super_Market_Management.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7210/api/");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Call the API to get dashboard data
                var response = await _httpClient.GetAsync("Dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var dashboardData = JsonConvert.DeserializeObject<DashboardViewModel>(json);

                    return View(dashboardData);
                }
                else
                {
                    // If API call fails, create a default view model
                    var defaultData = new DashboardViewModel
                    {
                        TotalProducts = 1254,
                        TodaySales = 8542,
                        NewOrders = 42,
                        LowStockItems = 17,
                        RecentOrders = new List<RecentOrder>(),
                        TopSellingProducts = new List<TopSellingProduct>()
                    };

                    TempData["Error"] = "Unable to load real-time dashboard data. Showing sample data.";
                    return View(defaultData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");

                // Return view with sample data if API call fails
                var sampleData = new DashboardViewModel
                {
                    TotalProducts = 1254,
                    TodaySales = 8542,
                    NewOrders = 42,
                    LowStockItems = 17,
                    RecentOrders = GetSampleRecentOrders(),
                    TopSellingProducts = GetSampleTopSellingProducts()
                };

                TempData["Error"] = "Unable to load dashboard data. Showing sample data.";
                return View(sampleData);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Helper Methods for Sample Data
        private List<RecentOrder> GetSampleRecentOrders()
        {
            return new List<RecentOrder>
            {
                new RecentOrder { OrderId = "#12345", Customer = "John Doe", Date = DateTime.Parse("2023-05-15"), Amount = 125.00m, Status = "Completed" },
                new RecentOrder { OrderId = "#12346", Customer = "Jane Smith", Date = DateTime.Parse("2023-05-15"), Amount = 89.50m, Status = "Processing" },
                new RecentOrder { OrderId = "#12347", Customer = "Bob Johnson", Date = DateTime.Parse("2023-05-14"), Amount = 215.75m, Status = "Completed" },
                new RecentOrder { OrderId = "#12348", Customer = "Alice Brown", Date = DateTime.Parse("2023-05-14"), Amount = 45.99m, Status = "Shipped" },
                new RecentOrder { OrderId = "#12349", Customer = "Charlie Wilson", Date = DateTime.Parse("2023-05-13"), Amount = 178.30m, Status = "Completed" }
            };
        }

        private List<TopSellingProduct> GetSampleTopSellingProducts()
        {
            return new List<TopSellingProduct>
            {
                new TopSellingProduct { ProductName = "Organic Milk", Category = "Dairy", SalesCount = 142 },
                new TopSellingProduct { ProductName = "Whole Wheat Bread", Category = "Bakery", SalesCount = 98 },
                new TopSellingProduct { ProductName = "Fresh Eggs", Category = "Dairy", SalesCount = 87 },
                new TopSellingProduct { ProductName = "Bananas", Category = "Fruits", SalesCount = 76 },
                new TopSellingProduct { ProductName = "Chicken Breast", Category = "Meat", SalesCount = 65 }
            };
        }
        #endregion
    }
    
}
