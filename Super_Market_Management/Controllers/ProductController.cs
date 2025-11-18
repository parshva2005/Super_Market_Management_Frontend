using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Super_Market_Management.Models;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace Super_Market_Management.Controllers
{
    public class ProductController : Controller
    {
        #region API Call
        public readonly HttpClient _client;
        public ProductController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7210/api/");
        }
        #endregion

        #region Get All Product Admin (Updated with filter option)
        public async Task<IActionResult> GetAllProduct(
            string searchTerm = "",
            int? categoryId = null,
            string sortBy = "name_asc",
            string priceRange = "",
            bool showRemoved = false)
        {
            try
            {
                // Load categories for dropdown
                var categoryResponse = await _client.GetAsync("Category/Dropdown");
                if (categoryResponse.IsSuccessStatusCode)
                {
                    var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
                    ViewBag.Categories = JsonConvert.DeserializeObject<List<CategoryDropDown>>(categoryJson);
                }

                // Use admin endpoint that includes deleted products
                var endpoint = "Product/Admin";
                var response = await _client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to load products.";
                    return View(new List<Product>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var allProducts = JsonConvert.DeserializeObject<List<Product>>(json);

                // Apply filters locally
                var filteredProducts = ApplyFiltersLocally(allProducts, searchTerm, categoryId, sortBy, priceRange);

                // Filter based on showRemoved flag
                if (!showRemoved)
                {
                    filteredProducts = filteredProducts.Where(p => !p.IsRemoved.GetValueOrDefault()).ToList();
                }
                else
                {
                    filteredProducts = filteredProducts.Where(p => p.IsRemoved.GetValueOrDefault()).ToList();
                }

                // Pass filter values to view
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.SortBy = sortBy;
                ViewBag.PriceRange = priceRange;
                ViewBag.ShowRemoved = showRemoved;

                return View(filteredProducts);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(new List<Product>());
            }
        }
        #endregion

        #region Restore Product
        [HttpPost]
        public async Task<IActionResult> RestoreProduct(int id)
        {
            var response = await _client.PatchAsync($"Product/ToggleStatus/{id}", null);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to restore product.";
            }
            else
            {
                TempData["Success"] = "Product restored successfully.";
            }
            return RedirectToAction("GetAllProduct", new { showRemoved = true });
        }
        #endregion

        #region Get All Product Customer (Updated with filtering)
        public async Task<IActionResult> GetAllProductCustomer(
            string searchTerm = "",
            int? categoryId = null,
            string sortBy = "name_asc",
            string priceRange = "")
        {
            try
            {
                // Load categories
                var categoryResponse = await _client.GetAsync("Category/Dropdown");
                var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<List<CategoryDropDown>>(categoryJson);
                ViewBag.Categories = categories;

                // Get all products first
                var response = await _client.GetAsync("Product/Customer");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to load products.";
                    return View(new List<Product>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var allProducts = JsonConvert.DeserializeObject<List<Product>>(json);

                // Apply filters locally
                var filteredProducts = ApplyFiltersLocally(allProducts, searchTerm, categoryId, sortBy, priceRange);

                // Pass filter values to view
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.SortBy = sortBy;
                ViewBag.PriceRange = priceRange;

                return View(filteredProducts);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(new List<Product>());
            }
        }

        private List<Product> ApplyFiltersLocally(List<Product> products, string searchTerm, int? categoryId, string sortBy, string priceRange)
        {
            IEnumerable<Product> query = products;

            // Search term filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(searchTerm) ||
                    (p.ProductDescription != null && p.ProductDescription.ToLower().Contains(searchTerm)) ||
                    (p.Sku != null && p.Sku.ToLower().Contains(searchTerm)));
            }

            // Category filter
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Price range filter
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "0-50":
                        query = query.Where(p => p.ProductPrice <= 50);
                        break;
                    case "50-100":
                        query = query.Where(p => p.ProductPrice > 50 && p.ProductPrice <= 100);
                        break;
                    case "100-500":
                        query = query.Where(p => p.ProductPrice > 100 && p.ProductPrice <= 500);
                        break;
                    case "500-1000":
                        query = query.Where(p => p.ProductPrice > 500 && p.ProductPrice <= 1000);
                        break;
                    case "1000+":
                        query = query.Where(p => p.ProductPrice > 1000);
                        break;
                }
            }

            // Sorting
            query = sortBy.ToLower() switch
            {
                "name_desc" => query.OrderByDescending(p => p.ProductName),
                "price_asc" => query.OrderBy(p => p.ProductPrice),
                "price_desc" => query.OrderByDescending(p => p.ProductPrice),
                "newest" => query.OrderByDescending(p => p.CreationDate),
                "oldest" => query.OrderBy(p => p.CreationDate),
                _ => query.OrderBy(p => p.ProductName), // Default: name_asc
            };

            return query.ToList();
        }
        #endregion

        #region Add Edit Product
        public async Task<IActionResult> AddEditProduct(int? id)
        {
            try
            {
                var categoryResponse = await _client.GetAsync("Category/Dropdown");
                var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
                var category = JsonConvert.DeserializeObject<List<CategoryDropDown>>(categoryJson);


                Product product;

                if (id == null || id == 0)
                {
                    product = new Product();
                }
                else
                {
                    var productResponse = await _client.GetAsync($"Product/{id}");
                    if (!productResponse.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }
                    var productJson = await productResponse.Content.ReadAsStringAsync();
                    product = JsonConvert.DeserializeObject<Product>(productJson);
                }

                product.Categorylist = category;
                return View(product);
            }
            catch
            {
                TempData["Error"] = "Failed to load product data.";
                return RedirectToAction("GetAllProduct");
            }

        }
        #endregion

        #region SAVE PRODUCT
        [HttpPost]
        public async Task<IActionResult> Save(Product product)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    product.UserId = userId.Value;
                }
                else
                {
                    // Handle case where user is not logged in
                    TempData["Error"] = "User not logged in";
                    return RedirectToAction("Login", "Account");
                }
                if (!ModelState.IsValid)
                {
                    var categoryResponse = await _client.GetAsync("Category/dropdown");
                    var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
                    product.Categorylist = JsonConvert.DeserializeObject<List<CategoryDropDown>>(categoryJson);
                    return View("AddEditProduct", product);
                }

                // Set dates
                if (product.ProductId == 0)
                {
                    product.CreationDate = DateTime.Now;
                }
                else
                {
                    product.ModifyDate = DateTime.Now;
                }

                var productJson = JsonConvert.SerializeObject(product, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                });

                var content = new StringContent(productJson, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (product.ProductId == 0)
                {
                    response = await _client.PostAsync("Product", content);
                    var createdProductResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    response = await _client.PutAsync($"Product/{product.ProductId}", content);
                    var createdProductResponse = await response.Content.ReadAsStringAsync();
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"API call failed: {response.StatusCode} - {error}";

                    // Reload categories
                    var categoryResponse = await _client.GetAsync("Category/dropdown");
                    var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
                    product.Categorylist = JsonConvert.DeserializeObject<List<CategoryDropDown>>(categoryJson);

                    return View("AddEditProduct", product);
                }

                return RedirectToAction("GetAllProduct");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("AddEditProduct", new { id = product.ProductId });
            }
        }
        #endregion

        #region Remove Product
        [HttpPost]
        public async Task<IActionResult> ToggleProductStatus(int id)
        {
            var response = await _client.PatchAsync($"Product/ToggleStatus/{id}", null);
            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            return RedirectToAction("GetAllProduct");
        }

        #endregion

        #region Get Product By Id
        public async Task<IActionResult> GetProductById(int id)
        {
            var response = await _client.GetAsync($"Product/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var json = await response.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<Product>(json);
            return View(product);
        }
        #endregion

        #region Delete Product
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"Product/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to delete Product: {ex.Message}";
            }
            TempData["DeleteSuccess"] = true;
            return RedirectToAction("GetAllProduct");
        }
        #endregion
    }
}
