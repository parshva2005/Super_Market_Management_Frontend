using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Super_Market_Management.Models;
using System.Data;
using System.Text;

namespace Super_Market_Management.Controllers
{
    public class UserController : Controller
    {
        #region API Call
        public readonly HttpClient _client;
        public UserController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7210/api/");
        }
        #endregion

        #region Get All User 
        public async Task<IActionResult> GetAllUser()
        {
            var response = await _client.GetAsync("User");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to load users";
                TempData["role"] = "User";
                return View(new List<User>());
            }

            var users = JsonConvert.DeserializeObject<List<User>>(
                await response.Content.ReadAsStringAsync());

            TempData["role"] = "User";
            return View( users);
            
        }
        #endregion

        #region Add Edit User
        public async Task<IActionResult> AddEditUser(int? id)
        {
            var role = await GetRoles();

            User user;

            if (id == null || id == 0)
            {
                user = new User();
            }
            else
            {
                var userResponse = await _client.GetAsync($"User/{id}");
                if (!userResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                var userJson = await userResponse.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<User>(userJson);
            }
            user.Rolelist = role;
            return View(user);
        }
        #endregion

        #region SAVE User
        [HttpPost]
        public async Task<IActionResult> Save(User user)
        {
            if (!ModelState.IsValid)
            {
                user.Rolelist = await GetRoles();
                return View("AddEditUser", user);
            }

            try
            {
                user.RoleId = 3002;
                var json = JsonConvert.SerializeObject(user, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (user.UserId == 0)
                {
                    response = await _client.PostAsync("User", content);
                    var createdUserResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    response = await _client.PutAsync($"User/{user.UserId}", content);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"API call failed: {response.StatusCode} - {error}";
                    return View("AddEditUser", user);

                }
                TempData["SaveSuccess"] = true;

                return RedirectToAction("GetAllUser");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to save user: {ex.Message}";
                return RedirectToAction("GetAllUser");
            }

        }
        #endregion

        #region Delete User
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"User/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to delete User: {ex.Message}";
            }
            TempData["DeleteSuccess"] = true;
            return RedirectToAction("GetAllUser");
        }
        #endregion

        #region Get User by id
        public async Task<IActionResult> GetUserById(int id)
        {
            var response = await _client.GetAsync($"User/{id}");
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<User>>(json);
            return View(list);
        }
        #endregion

        #region Get Role
        private async Task<List<RoleDropDown>> GetRoles()
        {
            var roleResponse = await _client.GetAsync("Role/Dropdown");
            var roleJson = await roleResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<RoleDropDown>>(roleJson) ?? new List<RoleDropDown>();
        }

        #endregion

        #region Profile View
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var response = await _client.GetAsync($"User/profile/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to load profile";
                return RedirectToAction("GetAllProductCustomer", "Product");
            }

            var userJson = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<User>(userJson);

            return View(user);
        }
        #endregion

        #region Profile Edit
        [HttpPost]
        public async Task<IActionResult> Profile(User user, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                // Reload the user data if validation fails
                var userId = HttpContext.Session.GetInt32("UserId");
                var response = await _client.GetAsync($"User/profile/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var userJson = await response.Content.ReadAsStringAsync();
                    var userData = JsonConvert.DeserializeObject<User>(userJson);
                    return View(userData);
                }
                return View(user);
            }

            try
            {
                using var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(user.UserId.ToString()), "UserId");
                formData.Add(new StringContent(user.UserName), "UserName");
                formData.Add(new StringContent(user.UserAddress), "UserAddress");
                formData.Add(new StringContent(user.UserMobileNumber), "UserMobileNumber");
                formData.Add(new StringContent(user.UserEmailAddress), "UserEmailAddress");

                if (file != null && file.Length > 0)
                {
                    var fileContent = new StreamContent(file.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    formData.Add(fileContent, "File", file.FileName);
                }

                var response = await _client.PutAsync($"User/profile/{user.UserId}", formData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Profile updated successfully";

                    // Update session with new name if changed
                    HttpContext.Session.SetString("UserName", user.UserName);

                    // Refresh the user data
                    var userResponse = await _client.GetAsync($"User/profile/{user.UserId}");
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var userJson = await userResponse.Content.ReadAsStringAsync();
                        var updatedUser = JsonConvert.DeserializeObject<User>(userJson);
                        return View(updatedUser);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Failed to update profile: {response.StatusCode} - {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            return View(user);
        }
        #endregion
    }
}
