using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Super_Market_Management.Models;
using System.Text;

namespace Super_Market_Management.Controllers
{
    public class CustomerController : Controller
    {
        #region API Call
        public readonly HttpClient _client;
        public CustomerController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7210/api/");
        }
        #endregion

        #region Get All Customer 
        public async Task<IActionResult> GetAllCustomer()
        {
            var response = await _client.GetAsync("Customer");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to load customers";
                TempData["role"] = "Customer";
                return View(new List<Customer>());
            }

            var customers = JsonConvert.DeserializeObject<List<Customer>>(
                await response.Content.ReadAsStringAsync());

            TempData["role"] = "Customer";
            return View(customers);

        }
        #endregion

        #region Add Edit Customer
        public async Task<IActionResult> AddEditCustomer(int? id, Customer customer)
        {

            if (id == null || id == 0)
            {
                customer = new Customer();
            }
            else
            {
                var customerResponse = await _client.GetAsync($"Customer/{id}");
                if (!customerResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                var customerJson = await customerResponse.Content.ReadAsStringAsync();
                customer = JsonConvert.DeserializeObject<Customer>(customerJson);
            }
            return View(customer);
        }
        #endregion

        #region SAVE Customer
        [HttpPost]
        public async Task<IActionResult> Save(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddEditCustomer");
            }
            try
            {
                var json = JsonConvert.SerializeObject(customer, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (customer.CustomerId == 0)
                {
                    response = await _client.PostAsync("Customer", content);
                }
                else
                {
                    response = await _client.PutAsync($"Customer/{customer.CustomerId}", content);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"API call failed: {response.StatusCode} - {error}";
                    return View("AddEditCustomer", error);
                }

                return RedirectToAction("GetAllCustomer");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to save user: {ex.Message}";
                return View("GetAllCustomer", customer);
            }
        }
        #endregion

        #region Delete Customer
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"Customer/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unable to delete Customer: {ex.Message}";
            }
            TempData["DeleteSuccess"] = true;
            return RedirectToAction("GetAllCustomer");
        }
        #endregion

        #region Get Customer by id
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var response = await _client.GetAsync($"Customer/{id}");
            var json = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<Customer>(json);
            return View(new List<Customer> { customer });
        }
        #endregion
    }
}
