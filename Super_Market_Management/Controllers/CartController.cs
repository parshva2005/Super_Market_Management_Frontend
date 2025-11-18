using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Super_Market_Management.Models;
using System.Text;

namespace Super_Market_Management.Controllers
{
    public class CartController : Controller
    {
        private readonly HttpClient _client;

        public CartController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7210/api/");
        }

        #region ADD TO CART (MVC)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "Please login to add items to cart";
                    return RedirectToAction("Login", "Login");
                }

                var request = new
                {
                    UserId = userId.Value,
                    ProductId = productId,
                    Quantity = quantity
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync("cart", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Product added to cart successfully!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = "Error adding to cart. Please try again.";
                }

                return RedirectToAction("GetAllProductCustomer", "Product");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error adding to cart. Please try again.";
                return RedirectToAction("GetAllProductCustomer", "Product");
            }
        }
        #endregion

        #region GET CART COUNT (For AJAX)
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Content("0");
                }

                var response = await _client.GetAsync($"cart/count/{userId.Value}");

                if (response.IsSuccessStatusCode)
                {
                    var count = await response.Content.ReadAsStringAsync();
                    return Content(count);
                }

                return Content("0");
            }
            catch
            {
                return Content("0");
            }
        }
        #endregion

        #region VIEW CART
        public async Task<IActionResult> ViewCart()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "Please login to view your cart";
                    return RedirectToAction("Login", "Login");
                }

                var response = await _client.GetAsync($"cart/user/{userId.Value}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var cartItems = JsonConvert.DeserializeObject<List<Cart>>(json);

                    // Calculate totals
                    decimal cartTotal = cartItems.Sum(item => item.ProductQuantity * (item.Product?.ProductPrice ?? 0));
                    decimal taxAmount = cartTotal * 0.18m;
                    decimal finalTotal = cartTotal + taxAmount;
                    int totalItems = cartItems.Sum(item => item.ProductQuantity);
                    int totalProducts = cartItems.Count;

                    ViewBag.CartTotal = cartTotal;
                    ViewBag.TaxAmount = taxAmount;
                    ViewBag.FinalTotal = finalTotal;
                    ViewBag.TotalItems = totalItems;
                    ViewBag.TotalProducts = totalProducts;

                    return View(cartItems);
                }
                else
                {
                    TempData["ErrorMessage"] = "Error loading cart";
                    return View(new List<Cart>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading cart";
                return View(new List<Cart>());
            }
        }
        #endregion

        #region UPDATE QUANTITY (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateCartQuantity(int cartId, int quantity)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Please login" });
                }

                var request = new { UserId = userId.Value, Quantity = quantity };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PatchAsync($"cart/{cartId}/quantity", content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Error updating quantity" });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Network error" });
            }
        }
        #endregion

        #region REMOVE FROM CART (AJAX)
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Please login" });
                }

                var response = await _client.DeleteAsync($"cart/{cartId}?userId={userId.Value}");

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Error removing item" });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Network error" });
            }
        }
        #endregion

        #region BUY NOW (Process Order)
        [HttpPost]
        public async Task<IActionResult> BuyNow()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Please login to complete your purchase" });
                }

                // Get cart items
                var cartResponse = await _client.GetAsync($"cart/user/{userId.Value}");
                if (!cartResponse.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = "Failed to load cart items" });
                }

                var json = await cartResponse.Content.ReadAsStringAsync();
                var cartItems = JsonConvert.DeserializeObject<List<Cart>>(json);

                if (cartItems == null || !cartItems.Any())
                {
                    return Json(new { success = false, message = "Your cart is empty" });
                }

                // Calculate totals
                decimal cartTotal = cartItems.Sum(item => item.ProductQuantity * (item.Product?.ProductPrice ?? 0));
                decimal taxAmount = cartTotal * 0.18m;
                decimal finalTotal = cartTotal + taxAmount;

                // Prepare buy now request
                var buyNowRequest = new
                {
                    UserId = userId.Value,
                    CustomerId = 0, // You might want to get this from user profile
                    TotalAmount = finalTotal,
                    TaxAmount = taxAmount,
                    DiscountAmount = 0,
                    Items = cartItems.Select(item => new
                    {
                        ProductId = item.ProductId,
                        Quantity = item.ProductQuantity,
                        Price = item.Product?.ProductPrice ?? 0
                    }).ToList()
                };

                var buyNowJson = JsonConvert.SerializeObject(buyNowRequest);
                var content = new StringContent(buyNowJson, Encoding.UTF8, "application/json");

                // Call the API to process the order
                var response = await _client.PostAsync("Order/buynow", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Failed to process order: " + error });
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(resultJson);

                return Json(new
                {
                    success = true,
                    orderId = result.OrderId,
                    orderNumber = result.OrderNumber,
                    message = "Order placed successfully!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error processing order: " + ex.Message });
            }
        }
        #endregion
    }
}