using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Super_Market_Management.Models;
using System.Text;

namespace Super_Market_Management.Controllers
{
    public class RoleController : Controller
    {

        #region API Call
        public readonly HttpClient _client;
        public RoleController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7210/api/");
        }
        #endregion

        #region Get All Role 
        public async Task<IActionResult> GetAllRole()
        {
            var response = await _client.GetAsync("Role");
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<Role>>(json);
            return View(list);
        }
        #endregion

        #region Add Edit Role
        public async Task<IActionResult> AddEditRole(int? id)
        {
            Role role;

            if (id == null || id == 0)
            {
                role = new Role();
            }
            else
            {
                var roleResponse = await _client.GetAsync($"Role/{id}");
                if (!roleResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                var roleJson = await roleResponse.Content.ReadAsStringAsync();
                role = JsonConvert.DeserializeObject<Role>(roleJson);
            }
            return View("AddEditRole", role);
        }
        #endregion
         
        #region Save Role
        [HttpPost]
        public async Task<IActionResult> SaveRole(Role role)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddEditRole");
            }
            try
            {
                var json = JsonConvert.SerializeObject(role, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (role.RoleId == 0)
                {
                    response = await _client.PostAsync("Role", content);
                }
                else
                {
                    response = await _client.PutAsync($"Role/{role.RoleId}", content);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"API call failed: {response.StatusCode} - {error}";
                    return View("AddEditRole", error);
                }

                return RedirectToAction("GetAllRole");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to save Role: {ex.Message}";
                return View("GetAllRole", role);
            }
        }
        #endregion

        #region Delete Role
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"Role/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to delete Role: {ex.Message}";
            }
            TempData["DeleteSuccess"] = true;
            return RedirectToAction("GetAllRole");
        }
        #endregion
    }
}
