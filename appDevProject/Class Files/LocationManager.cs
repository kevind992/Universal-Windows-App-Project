﻿using appDevProject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Maps;

namespace UWP_Main_App
{
    public class LocationManager
    {

        private List<Result> galwayStops = new List<Result>();

        private double galLatLow = 53.01347187;
        private double galLatHigh = 53.54880427;
        private double galLongLeft = -9.58028032;
        private double galLongRight = -8.4061165;

        private int count;

        public async static Task<Geoposition> GetPosition()
        {

            var accessStatus = await Geolocator.RequestAccessAsync();

            if (accessStatus != GeolocationAccessStatus.Allowed) throw new Exception();

            var geolocator = new Geolocator { DesiredAccuracyInMeters = 0 };

            var position = await geolocator.GetGeopositionAsync();

            return position;
        }

        public List<MapElement> AddSpaceNeedleIcon()
        {
            getBusStops();

            var MyLandmarks = new List<MapElement>();

            for (int i = 0; i > galwayStops.Count; i++)
            {
                BasicGeoposition snPosition = new BasicGeoposition { Latitude = galwayStops[i].latitude, Longitude = galwayStops[i].longitude };
                Geopoint snPoint = new Geopoint(snPosition);

                var spaceNeedleIcon = new MapIcon
                {
                    Location = snPoint,
                    NormalizedAnchorPoint = new Point(0.5, 1.0),
                    ZIndex = 0,
                    Title = "Space Needle " + count
                    
                };
                count++;

                MyLandmarks.Add(spaceNeedleIcon);
            }

            var LandmarksLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = MyLandmarks
            };


            return MyLandmarks;

        }

        async void getBusStops()
        {
            string url = "http://data.dublinked.ie/cgi-bin/rtpi/busstopinformation?&operator=BE&format=json%22";

            HttpClient client = new HttpClient();

            string response2 = await client.GetStringAsync(url);

            var busData = JsonConvert.DeserializeObject<Rootobject>(response2);

            for (int i = 1; i < busData.numberofresults; i++)
            {
                if (busData.results[i].latitude < galLatHigh && busData.results[i].latitude > galLatLow)
                {
                    if (busData.results[i].longitude > galLongLeft && busData.results[i].longitude < galLongRight)
                    {
                        galwayStops.Add(busData.results[i]);
                    }
                }
            }

        }
    }
}
