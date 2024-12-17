using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http;
using System.Text;
using WebApiConsume.Helper;
using WebApiConsume.Models;

namespace WebApiConsume.Controllers
{
    public class CityController : Controller
    {
        #region configurations
        Uri BaseUrl = new Uri("http://localhost:20444/api");
        private readonly HttpClient _client;

        public CityController()
        {
            _client = new HttpClient();
            _client.BaseAddress = BaseUrl;
        }
        #endregion
        
        #region CityList
        public IActionResult CityList()
        {
            List<CityModel> cities = new List<CityModel>();
            HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/City/GetAllCitis").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonobject = JsonConvert.DeserializeObject(data);
                var extData = JsonConvert.SerializeObject(jsonobject);
                cities = JsonConvert.DeserializeObject<List<CityModel>>(extData);
            }
            return View("CityList", cities);
        }
        #endregion

        #region AddEditCity

        public IActionResult AddEditCity(string? id)
        {
            int? decryptedCityID = null;

            if (!string.IsNullOrEmpty(id))
            {
                string decryptedCityIDString = UrlEncryptor.Decrypt(id); // Decrypt the encrypted CityID
                decryptedCityID = int.Parse(decryptedCityIDString); // Convert decrypted string to integer
            }

            LoadCountryList();

            if (decryptedCityID != null)
            {
                List<CityModel> cities = new List<CityModel>();
                HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/City/GetAllCityByID/{decryptedCityID}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    dynamic jsonobject = JsonConvert.DeserializeObject(data);
                    var extData = JsonConvert.SerializeObject(jsonobject);
                    cities = JsonConvert.DeserializeObject<List<CityModel>>(extData);
                    CityModel cityModel = new CityModel
                    {
                        CityID = cities[0].CityID,
                        CityName = cities[0].CityName,
                        CountryID = cities[0].CountryID,
                        StateID = cities[0].StateID,
                        DistrictID = cities[0].DistrictID, 
                        TalukaID = cities[0].TalukaID,
                    };
                    
                    ViewBag.StateList = GetStateByCountryID(cityModel.CountryID);
                    ViewBag.DistrictList = GetDistrictByStateID(cityModel.StateID);
                    ViewBag.TalukaList = GetTalukaByDistrictID(cityModel.DistrictID);
                    return View(cityModel);
                }
                else
                {
                    return RedirectToAction("CityList");
                }

            }
            else
            {
                return View();
            }
        }
        #endregion

        #region Save City
        [HttpPost]
        public async Task<IActionResult> SaveCity(CityModel cm)
        {
            try
            {
                var json = JsonConvert.SerializeObject(cm);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;
                if (cm.CityID == 0)
                {
                    response = await _client.PostAsync($"{_client.BaseAddress}/City/InsertCity", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Record Inserted Successfully";
                        return RedirectToAction("AddEditCity");
                    }
                }
                else
                {
                    response = await _client.PutAsync($"{_client.BaseAddress}/City/UpdateCity/{cm.CityID}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Record Updated Successfully";
                        return RedirectToAction("CityList");
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("CityList");

        }
        #endregion

        #region Delete City
        public IActionResult CityDelete(string CityID)
        {
            int decryptedCityID = Convert.ToInt32(UrlEncryptor.Decrypt(CityID.ToString()));
            try
            {
                var response = _client.DeleteAsync($"{_client.BaseAddress}/City/DeleteCityByID/{decryptedCityID}");
                TempData["Notification"] = "Record Deleted Successfully";
                return RedirectToAction("CityList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                TempData["Notification"] = "You can not delete this record due to Foreign Key Constraints ";
                return RedirectToAction("CityList");
            }
        }
        #endregion

        #region LoadCountryList
        // Load the dropdown list of countries
        private void LoadCountryList()
        {
            List<CountryModel> countries = new List<CountryModel>();
            HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/Country/GetAllCountries").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonobject = JsonConvert.DeserializeObject(data);
                var extData = JsonConvert.SerializeObject(jsonobject);
                countries = JsonConvert.DeserializeObject<List<CountryModel>>(extData);
            }
            ViewBag.CountryList = countries;
        }
        #endregion

        #region GetStatesByCountry
        // AJAX handler for loading states dynamically
        [HttpPost]
        public JsonResult GetStatesByCountry(int CountryID)
        {
            List<StateDropDownModel> loc_State = GetStateByCountryID(CountryID); // Fetch states
            return Json(loc_State); // Return JSON response
        }
        #endregion

        #region GetStateByCountryID
        // Helper method to fetch states by country ID
        public List<StateDropDownModel> GetStateByCountryID(int CountryID)
        {
            List<StateDropDownModel> states = new List<StateDropDownModel>();
            HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/State/StatesDropDownByCountryID/{CountryID}").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonobject = JsonConvert.DeserializeObject(data);
                var extData = JsonConvert.SerializeObject(jsonobject);
                states = JsonConvert.DeserializeObject<List<StateDropDownModel>>(extData);
            }
            return states;
        }
        #endregion

        #region GetDistrictsByState
        // AJAX handler for loading states dynamically
        [HttpPost]
        public JsonResult GetDistrictsByState(int StateID)
        {
            List<DistrictDropDownModel> loc_District = GetDistrictByStateID(StateID); // Fetch states
            return Json(loc_District); // Return JSON response
        }
        #endregion

        #region GetDistrictByStateID
        // Helper method to fetch states by country ID
        public List<DistrictDropDownModel> GetDistrictByStateID(int StateID)
        {
            List<DistrictDropDownModel> districts = new List<DistrictDropDownModel>();
            HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/District/DistrictsDropDownByStateID/{StateID}").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonobject = JsonConvert.DeserializeObject(data);
                var extData = JsonConvert.SerializeObject(jsonobject);
                districts = JsonConvert.DeserializeObject<List<DistrictDropDownModel>>(extData);
            }
            return districts;
        }
        #endregion

        #region GetTalukasByDistrict
        // AJAX handler for loading states dynamically
        [HttpPost]
        public JsonResult GetTalukasByDistrict(int DistrictID)
        {
            List<TalukaDropDownModel> loc_Taluka = GetTalukaByDistrictID(DistrictID); // Fetch states
            return Json(loc_Taluka); // Return JSON response
        }
        #endregion

        #region GetTalukaByDistrictID
        // Helper method to fetch states by country ID
        public List<TalukaDropDownModel> GetTalukaByDistrictID(int DistrictID)
        {
            List<TalukaDropDownModel> talukas = new List<TalukaDropDownModel>();
            HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/Taluka/TalukasDropDownByDistrictID/{DistrictID}").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonobject = JsonConvert.DeserializeObject(data);
                var extData = JsonConvert.SerializeObject(jsonobject);
                talukas = JsonConvert.DeserializeObject<List<TalukaDropDownModel>>(extData);
            }
            return talukas;
        }
        #endregion
    }
}
