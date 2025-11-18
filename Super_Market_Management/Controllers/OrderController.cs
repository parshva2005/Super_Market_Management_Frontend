using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Super_Market_Management.Models;
using System.Text;

namespace Super_Market_Management.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _client;

        public OrderController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7210/api/");
        }

        #region Get All Order 
        public async Task<IActionResult> GetAllOrder()
        {
            var response = await _client.GetAsync("Order");
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<Order>>(json);
            return View(list);
        }
        #endregion

        #region Add Edit Order
        public async Task<IActionResult> AddEditOrder(int? id)
        {
            var userResponse = await _client.GetAsync("User/Dropdown");
            var userJson = await userResponse.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<List<UserDropDown>>(userJson);

            var customerResponse = await _client.GetAsync("Customer/Dropdown");
            var customerJson = await customerResponse.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<List<CustomerDropDown>>(customerJson);

            Order order;

            if (id == null || id == 0)
            {
                order = new Order
                {
                    OrderDate = DateTime.Today
                };
            }
            else
            {
                var orderResponse = await _client.GetAsync($"Order/{id}");
                if (!orderResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                var orderJson = await orderResponse.Content.ReadAsStringAsync();
                order = JsonConvert.DeserializeObject<Order>(orderJson);
            }

            order.Userlist = user;
            order.Customerlist = customer;
            return View(order);
        }
        #endregion

        #region SAVE Order
        [HttpPost]
        public async Task<IActionResult> Save(Order order)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddEditOrder");
            }
            try
            {
                order.Status = "Completed";
                var json = JsonConvert.SerializeObject(order, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                });

                
                var content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (order.OrderId == 0)
                {
                    response = await _client.PostAsync("Order", content);
                }
                else
                {
                    
                    response = await _client.PutAsync($"Order/{order.OrderId}", content);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"API call failed: {response.StatusCode} - {error}";
                    return View("AddEditOrder", error);
                }

                return RedirectToAction("GetAllOrder");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to save Order: {ex.Message}";
                return View("GetAllOrder", order);
            }
        }
        #endregion

        #region Get All Orders for Current User
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Login");
            }

            var response = await _client.GetAsync($"Order/user/{userId.Value}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to load orders";
                return View(new List<Order>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<Order>>(json);
            return View(orders);
        }
        #endregion

        #region Order Details
        public async Task<IActionResult> Details(int id)
        {
            var response = await _client.GetAsync($"Order/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Order not found";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var order = JsonConvert.DeserializeObject<Order>(json);
            return View(order);
        }
        #endregion

        #region Checkout Page
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Login");
            }

            // Get cart items for the current user
            var response = await _client.GetAsync($"Cart/user/{userId.Value}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to load cart items";
                return RedirectToAction("ViewCart", "Cart");
            }

            var json = await response.Content.ReadAsStringAsync();
            var cartItems = JsonConvert.DeserializeObject<List<Cart>>(json);

            // If cart is empty, redirect back to cart with message
            if (cartItems == null || !cartItems.Any())
            {
                TempData["Warning"] = "Your cart is empty. Please add items before checkout.";
                return RedirectToAction("ViewCart", "Cart");
            }

            return View(cartItems);
        }
        #endregion

        #region Process Buy Now (Form Submission)
        [HttpPost]
        public async Task<IActionResult> ProcessBuyNow(BuyNowModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    TempData["Error"] = "User not logged in";
                    return RedirectToAction("Login", "Login");
                }

                var buyNowRequest = new
                {
                    UserId = userId.Value,
                    CustomerId = model.CustomerId,
                    TotalAmount = model.TotalAmount,
                    TaxAmount = model.TaxAmount,
                    DiscountAmount = model.DiscountAmount,
                    Items = model.Items.Select(i => new
                    {
                        i.ProductId,
                        i.Quantity,
                        i.Price
                    }).ToList()
                };

                var json = JsonConvert.SerializeObject(buyNowRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync("Order/buynow", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = "Order processing failed: " + error;
                    return RedirectToAction("ViewCart", "Cart");
                }

                // Properly deserialize the response
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<BuyNowResponse>(resultJson);

                if (result == null)
                {
                    TempData["Error"] = "Invalid response from server";
                    return RedirectToAction("ViewCart", "Cart");
                }

                TempData["OrderSuccess"] = true;
                return RedirectToAction("Confirmation", new
                {
                    orderId = result.OrderId,
                    orderNumber = result.OrderNumber
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Order processing failed: " + ex.Message;
                return RedirectToAction("ViewCart", "Cart");
            }
        }
        #endregion

        #region Order Confirmation
        public IActionResult Confirmation(int orderId, string orderNumber)
        {
            ViewBag.OrderId = orderId;
            ViewBag.OrderNumber = orderNumber;
            return View();
        }
        #endregion
    }

    public class BuyNowModel
    {
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<BuyNowItemModel> Items { get; set; }
    }

    public class BuyNowItemModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
    public class BuyNowResponse
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string Message { get; set; }
    }
}