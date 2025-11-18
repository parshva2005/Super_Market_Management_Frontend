using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Super_Market_Management.Models;

namespace Super_Market_Management.Controllers
{
    public class OrderItemController : Controller
    {
        private readonly HttpClient _client;

        public OrderItemController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7210/api/");
        }

        #region Get Order Items
        public async Task<IActionResult> Index(int orderId)
        {
            var response = await _client.GetAsync($"OrderItem/order/{orderId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to load order items";
                return View(new List<OrderItem>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var orderItems = JsonConvert.DeserializeObject<List<OrderItem>>(json);
            return View(orderItems);
        }
        #endregion
    }
}