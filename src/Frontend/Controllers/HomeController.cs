using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private static string BackendUrl = "http://localhost:5000/api/values/";
        private static string TextDetailsUrl = "http://127.0.0.1:5001/Home/TextDetails/";
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string data, string location)
        {
            TempData["Text"] = data;
            string id = null; 
            string dataWithLocation = $"{data}:{location}";
            id = await GetId(dataWithLocation); //GetId(data);
            Console.WriteLine("Location: "+location);
            return new RedirectResult(TextDetailsUrl + id);
        }

        private static async Task<string> GetId(string data)
        {
            string url = BackendUrl;
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PostAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult TextDetails(string id, string data)
		{
			string details = SendGetRequest(BackendUrl+id).Result;
			ViewData["Message"] = "vowels\\consonants = "+details;
			return View();
		}
        private async Task<string> SendGetRequest(string requestUri)
		{
            HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync(requestUri);
			string value = await response.Content.ReadAsStringAsync();
            Console.WriteLine("value : "+value);
			if (response.IsSuccessStatusCode && value != null)
			{
				return value;
			}
			return response.StatusCode.ToString();
		}
    }
}
