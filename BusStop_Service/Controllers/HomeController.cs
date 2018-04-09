using appDevProject;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace BusStop_Service.Controllers
{
    public class HomeController : Controller
    {
        // Setting Lat and Lon for area around Co. Galway
        private double galLatLow = 53.01347187;
        private double galLatHigh = 53.54880427;
        private double galLongLeft = -9.58028032;
        private double galLongRight = -8.4061165;

        #region Index
        // GET: Home
        public async Task<ActionResult> Index(double lat, double lon)
        {
            // Addapted from : https://channel9.msdn.com/Series/Windows-10-development-for-absolute-beginners/UWP-061-UWP-Weather-Updating-the-Tile-with-Periodic-Notifications

            string stopName = await GetShortestBusStop(lat, lon);

            ViewBag.Message1 = "Nearest";
            ViewBag.Message2 = "Nearest Stop";
            ViewBag.Name = stopName;

            return View();
        }
        #endregion
        #region GetShorestBusStop Method
        public async Task<string> GetShortestBusStop(double lat, double lon)
        {
            //Initialising Variables
            double shortest = 0.0;
            int shortestIndex = 0;
            bool check = true;

            try
            {
                //Restful Api url for retreaving bus stop information
                string url = "http://data.dublinked.ie/cgi-bin/rtpi/busstopinformation?&operator=BE&format=json%22";

                // Making the get request and storing the response within response
                HttpClient client = new HttpClient();
                string response = await client.GetStringAsync(url);
                var busData = JsonConvert.DeserializeObject<Rootobject>(response);

                //An algorithm to filter throught all the bus stops and when a stop from the galway area is found it is then checked to see if it is close
                // to the users latitude and longtitude
                for (int i = 1; i < busData.numberofresults; i++)
                {
                    if (busData.results[i].latitude < galLatHigh && busData.results[i].latitude > galLatLow)
                    {
                        if (busData.results[i].longitude > galLongLeft && busData.results[i].longitude < galLongRight)
                        {
                            //Code addapted from : https://social.msdn.microsoft.com/Forums/vstudio/en-US/58ff6473-81de-42bf-be40-b550de26bdb1/uwpmapc-how-to-calculate-distance-of-2-points-by-longitude-and-latitude?forum=wpdevelop

                            //Calcuating the distance from the users Lat and Lon to the galway stop currently going throught the for loop
                            double rlat1 = Math.PI * lat / 180;
                            double rlat2 = Math.PI * busData.results[i].latitude / 180;
                            double theta = lon - busData.results[i].longitude;
                            double rtheta = Math.PI * theta / 180;
                            double dist =
                                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                                Math.Cos(rlat2) * Math.Cos(rtheta);
                            dist = Math.Acos(dist);
                            dist = dist * 180 / Math.PI;
                            dist = dist * 60 * 1.1515;

                            dist = dist * 1.609344;

                            //If it is the first time going through the algorithm then store the values
                            if (check == true)
                            {
                                shortest = dist;
                                shortestIndex = i;
                                check = false;
                            }
                            else //If it is the second time going through the algorithm check wheather the distance is less then the stored distance, if it is less then store.
                            {
                                if (dist < shortest)
                                {
                                    shortest = dist;
                                    shortestIndex = i;
                                }
                            }
                        }
                    }
                }

                check = true;

                //Returning the closed bus-stop
                return busData.results[shortestIndex].fullname;
            }
            catch
            {
                return "Error - getting Data";
            }
        }
        #endregion
    }
}