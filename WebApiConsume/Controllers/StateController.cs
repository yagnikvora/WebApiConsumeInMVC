using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using WebApiConsume.Helper;
using WebApiConsume.Models;

namespace WebApiConsume.Controllers
{
    public class StateController : Controller
    {
        #region configurations
        Uri BaseUrl = new Uri("http://localhost:20444/api");
        private readonly HttpClient _client;

        public StateController()
        {
            _client = new HttpClient();
            _client.BaseAddress = BaseUrl;
        }
        #endregion

        #region StateList
        public IActionResult StateList()
        {
            List<StateModel> states = new List<StateModel>();
            HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/State/GetAllStates").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                dynamic jsonobject = JsonConvert.DeserializeObject(data);
                var extData = JsonConvert.SerializeObject(jsonobject);
                states = JsonConvert.DeserializeObject<List<StateModel>>(extData);
            }
            return View("StateList", states);
        }
        #endregion

        #region AddEditState

        public IActionResult AddEditState(string? id)
        {
            int? decryptedStateID = null;

            if (!string.IsNullOrEmpty(id))
            {
                string decryptedStateIDString = UrlEncryptor.Decrypt(id); // Decrypt the encrypted StateID
                decryptedStateID = int.Parse(decryptedStateIDString); // Convert decrypted string to integer
            }

            LoadCountryList();

            if (decryptedStateID != null)
            {
                List<StateModel> states = new List<StateModel>();
                HttpResponseMessage response = _client.GetAsync($"{_client.BaseAddress}/State/GetAllStateByID/{decryptedStateID}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    dynamic jsonobject = JsonConvert.DeserializeObject(data);
                    var extData = JsonConvert.SerializeObject(jsonobject);
                    states = JsonConvert.DeserializeObject<List<StateModel>>(extData);
                    StateModel stateModel = new StateModel
                    {
                        StateID = states[0].StateID,
                        StateName = states[0].StateName,
                        StateCode = states[0].StateCode,
                        CountryID = states[0].CountryID,
                        CityCount = states[0].CityCount
                    };
                    return View(stateModel);
                }
                else
                {
                    return RedirectToAction("StateList");
                }

            }
            else
            {
                return View();
            }
        }
        #endregion

        #region Save State
        [HttpPost]
        public async Task<IActionResult> SaveState(StateModel cm)
        {
            try
            {
                var json = JsonConvert.SerializeObject(cm);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;
                if (cm.StateID == 0)
                {
                    response = await _client.PostAsync($"{_client.BaseAddress}/State/InsertState", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Record Inserted Successfully";
                        return RedirectToAction("AddEditState");
                    }
                }
                else
                {
                    response = await _client.PutAsync($"{_client.BaseAddress}/State/UpdateState/{cm.StateID}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Record Updated Successfully";
                        return RedirectToAction("StateList");
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("StateList");

        }
        #endregion

        #region Delete State
        public IActionResult StateDelete(string StateID)
        {
            int decryptedStateID = Convert.ToInt32(UrlEncryptor.Decrypt(StateID.ToString()));
            try
            {
                var response = _client.DeleteAsync($"{_client.BaseAddress}/State/DeleteStateByID/{decryptedStateID}");
                TempData["Notification"] = "Record Deleted Successfully";
                return RedirectToAction("StateList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                TempData["Notification"] = "You can not delete this record due to Foreign Key Constraints ";
                return RedirectToAction("StateList");
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
    }
}
