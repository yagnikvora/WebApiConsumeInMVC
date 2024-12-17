using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApiConsume.Models;

namespace WebApiConsume.Controllers
{
    public class ProductController : Controller
    {
        Uri BaseUrl = new Uri("https://localhost:44391/api");
        private readonly HttpClient _client;

        public ProductController()
        {
            _client = new HttpClient();
            _client.BaseAddress = BaseUrl;
        }

        public IActionResult ProductGet()
        {
            List<ProductModel> products = new List<ProductModel>();
            HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/Product/GetAllProducts").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonobject = JsonConvert.DeserializeObject(data);
                var extData = JsonConvert.SerializeObject(jsonobject);
                products = JsonConvert.DeserializeObject<List<ProductModel>>(extData);
            }
            return View("ProductList",products);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
