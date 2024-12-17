using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApiConsume.Models;

namespace WebApiConsume.Controllers
{
    public class CountryController : Controller
    {

        Uri BaseUrl = new Uri("http://localhost:20444/api");
        private readonly HttpClient _client;

        public CountryController()
        {
            _client = new HttpClient();
            _client.BaseAddress = BaseUrl;
        }

        public IActionResult CountryGet()
        {
            List<CountryModel> products = new List<CountryModel>();
            HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/Country/GetAllCountries").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonobject = JsonConvert.DeserializeObject(data);
                var extData = JsonConvert.SerializeObject(jsonobject);
                products = JsonConvert.DeserializeObject<List<CountryModel>>(extData);
            }
            return View("CountryList", products);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
