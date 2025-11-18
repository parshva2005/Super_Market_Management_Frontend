using Microsoft.AspNetCore.Mvc;
using SMM_API.Models;
using Super_Market_Management.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Super_Market_Management.Controllers
{
    public class LoginController : Controller
    {
        #region API Call
        public readonly HttpClient _client;
        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7210/api/");
        }
        #endregion

        #region Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("Login/login", model);

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

                    HttpContext.Session.SetString("JWTToken", authResponse.Token);
                    HttpContext.Session.SetInt32("UserId", authResponse.User.UserId);
                    HttpContext.Session.SetString("UserName", authResponse.User.UserName.ToString());
                    HttpContext.Session.SetString("UserRole", authResponse.User.Role);

                    _client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", authResponse.Token);

                    if(authResponse.User.Role == "Admin")
                        return RedirectToAction("Index", "Home");
                    else return RedirectToAction("GetAllProductCustomer", "Product");
                }

                ViewBag.Error = "Invalid email or password";
            }
            catch
            {
                ViewBag.Error = "Error connecting to the server";
            }

            return View(model);
        }
        #endregion

        #region Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            try
            {
                model.CreationDate = DateTime.UtcNow;
                model.IsActive = true;
                model.RoleId = 3002;

                var response = await _client.PostAsJsonAsync("Login/register", model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Registration successful! Please login.";
                    return RedirectToAction("Login");
                }

                ViewBag.Error = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                ViewBag.Error = "Error connecting to the server";
            }

            return View(model);
        }
        #endregion

        #region LogOut
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            _client.DefaultRequestHeaders.Authorization = null;
            return RedirectToAction("GetAllProductCustomer","Product");
        }
        #endregion
    }
}

