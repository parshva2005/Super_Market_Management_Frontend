using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Super_Market_Management.Models;
using System.Text;

namespace Super_Market_Management.Controllers
{
    public class CategoryController : Controller
    {
        #region API Call
        public readonly HttpClient _client;
        public CategoryController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7210/api/");
        }
        #endregion

        #region Get All Category 
        public async Task<IActionResult> GetAllCategory()
        {
            var response = await _client.GetAsync("Category");
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<Category>>(json);
            return View(list);
        }
        #endregion

        

        #region Add Edit Category
        public async Task<IActionResult> AddEditCategory(int? id)
        {
            var userResponse = await _client.GetAsync("User/Dropdown");
            var userJson = await userResponse.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<List<UserDropDown>>(userJson);

            Category category;

            if (id == null || id == 0)
            {
                category = new Category();
            }
            else
            {
                var categoryResponse = await _client.GetAsync($"Category/{id}");
                if (!categoryResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
                category = JsonConvert.DeserializeObject<Category>(categoryJson);
            }
            category.Userlist = user;
            return View(category);
        }
        #endregion

        #region SAVE Category
        [HttpPost]
        public async Task<IActionResult> Save(Category category)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddEditCategory");
            }
            try
            {
                var json = JsonConvert.SerializeObject(category, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (category.CategoryId == 0)
                {
                    response = await _client.PostAsync("Category", content);
                }
                else
                {
                    response = await _client.PutAsync($"Category/{category.CategoryId}", content);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"API call failed: {response.StatusCode} - {error}";
                    return View("AddEditCategory", error);
                }

                return RedirectToAction("GetAllCategory");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to save category: {ex.Message}";
                return View("GetAllCategory", category);
            }
        }
        #endregion

        #region Delete Category
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"Category/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to delete Category: {ex.Message}";
            }
            TempData["DeleteSuccess"] = true;
            return RedirectToAction("GetAllCategory");
        }
        #endregion
    }
}
