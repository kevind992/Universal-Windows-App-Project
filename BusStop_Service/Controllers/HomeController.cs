using BusStop_Service.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BusStop_Service.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public async Task<ActionResult> Index(string stopID, string stopName)
        {

            string url = "http://data.dublinked.ie/cgi-bin/rtpi/realtimebusinformation?stopid=" + stopID + "&format=json";

            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(url);

            var data = JsonConvert.DeserializeObject<Rootobject>(response);


            ViewBag.BusNo = data.results[0].route;
            ViewBag.Time = data.results[0].departureduetime;
            ViewBag.Destination = "from " + stopName;


            return View();
        }
    }
}