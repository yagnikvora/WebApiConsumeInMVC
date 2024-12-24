using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using WebApiConsume.Helper;
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
        #region Get All Country
        public IActionResult CountryList()
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
        #endregion

        #region AddEditCountry

        public IActionResult AddEditCountry(string? id)
        {
            int? decryptedCountryID = null;

            if (!string.IsNullOrEmpty(id))
            {
                string decryptedCountryIDString = UrlEncryptor.Decrypt(id); // Decrypt the encrypted CountryID
                decryptedCountryID = int.Parse(decryptedCountryIDString); // Convert decrypted string to integer
            }


            if (decryptedCountryID != null)
            {
                List<CountryModel> countries = new List<CountryModel>();
                HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/Country/GetAllCountryByID/{decryptedCountryID}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    dynamic jsonobject = JsonConvert.DeserializeObject(data);
                    var extData = JsonConvert.SerializeObject(jsonobject);
                    countries = JsonConvert.DeserializeObject<List<CountryModel>>(extData);
                    CountryModel countryModel = new CountryModel
                    {
                        CountryID = countries[0].CountryID,
                        CountryName = countries[0].CountryName,
                        CountryCode = countries[0].CountryCode
                    };

                    return View(countryModel);
                }
                else
                {
                    return RedirectToAction("CountryList");
                }

            }
            else
            {
                return View();
            }
        }
        #endregion

        #region Save Country
        [HttpPost]
        public async Task<IActionResult> SaveCountry(CountryModel cm)
        {
            try
            {
                var json = JsonConvert.SerializeObject(cm);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;
                if (cm.CountryID == 0)
                {
                    response = await _client.PostAsync($"{_client.BaseAddress}/Country/InsertCountry", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Record Inserted Successfully";
                        return RedirectToAction("AddEditCountry");
                    }
                }
                else
                {
                    response = await _client.PutAsync($"{_client.BaseAddress}/Country/UpdateCountry/{cm.CountryID}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Record Updated Successfully";
                        return RedirectToAction("CountryList");
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("CountryList");

        }
        #endregion

        #region Delete Country
        public IActionResult CountryDelete(string CountryID)
        {
            int decryptedCountryID = Convert.ToInt32(UrlEncryptor.Decrypt(CountryID.ToString()));
            try
            {
                var response = _client.DeleteAsync($"{_client.BaseAddress}/Country/DeleteCountryByID/{decryptedCountryID}");
                TempData["Notification"] = "Record Deleted Successfully";
                return RedirectToAction("CountryList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                TempData["Notification"] = "You can not delete this record due to Foreign Key Constraints ";
                return RedirectToAction("CountryList");
            }
        }
        #endregion
    }
}
