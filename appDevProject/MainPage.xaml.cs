using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using UWP_Main_App;
using UWP_Main_App.Class_Files;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace appDevProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private double galLatLow = 53.01347187;
        private double galLatHigh = 53.54880427;
        private double galLongLeft = -9.58028032;
        private double galLongRight = -8.4061165;

        private string stopName1;
        private string stopName2;
        private string stopID1;
        private string stopID2;
        private bool stopTurn = true;
        private bool loaded = false;

        private List<Result> galwayStops = new List<Result>();

        public object MapIcon1 { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
           
           
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            stopID1 = (string)localSettings.Values["stopID1"];
            stopID2 = (string)localSettings.Values["stopID2"];
            stopName1 = (string)localSettings.Values["stopName1"];
            stopName2 = (string)localSettings.Values["stopName2"];

            getSearchResults(stopID1);
            getSearchResults(stopID2);

            getBusStops();

            setMap();
        }

        async void getSearchResults(string stop)
        {
            string url = "http://data.dublinked.ie/cgi-bin/rtpi/realtimebusinformation?stopid=" + stop + "&format=json";

            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(url);

            var data = JsonConvert.DeserializeObject<Rootobject2>(response);

            if (stopTurn == false)
            {
                tbxStopName1.Text = stopName1;
                lvListBuses1.ItemsSource = data.results;
                stopTurn = true;
            }
            else
            {
                tbxStopName2.Text = stopName2;
                lvListBuses2.ItemsSource = data.results;
                stopTurn = false;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Refresh Selected..");

            lvListBuses1.ItemsSource = null;
            lvListBuses1.Items.Clear();
            lvListBuses2.ItemsSource = null;
            lvListBuses2.Items.Clear();

            stopTurn = true;

            getSearchResults(stopID1);
            getSearchResults(stopID2);
        }

        private void setMap()
        {
            BasicGeoposition cityPosition = new BasicGeoposition() {
                Latitude = 53.281551,
                Longitude = -9.035187
            };

            Geopoint cityCentre = new Geopoint(cityPosition);

            MapControl1.Center = cityCentre;
            MapControl1.LandmarksVisible = true;
            MapControl1.ZoomLevel = 12;
        }

        private async void getBusStops()
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
            loaded = true;
            System.Diagnostics.Debug.WriteLine("Finished Loading bus stops for locations..");

            await setIconsAsync();
        }

        private async Task setIconsAsync()
        {
            for (int i = 0; i < galwayStops.Count; i++)
            {
                BasicGeoposition snPosition = new BasicGeoposition();
                snPosition.Latitude = galwayStops[i].latitude;
                snPosition.Longitude = galwayStops[i].longitude;

                MapIcon mapIcon = new MapIcon();

                mapIcon.Location = new Geopoint(snPosition);
                mapIcon.Title = galwayStops[i].shortname;
                MapControl1.MapElements.Add(mapIcon);
            }
            System.Diagnostics.Debug.WriteLine("Bus Stop points added..");

            await getUserLocationAsync();

        }
        
        private async Task getUserLocationAsync()
        {
            var access = await Geolocator.RequestAccessAsync();

            switch (access)
            {
                case GeolocationAccessStatus.Allowed:

                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 0 };
                    geolocator.StatusChanged += Geolocator_StatusChanged;
                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    BasicGeoposition snPosition = new BasicGeoposition();
                    snPosition.Latitude = (float)pos.Coordinate.Point.Position.Latitude;
                    snPosition.Longitude = (float)pos.Coordinate.Point.Position.Longitude;

                    MapIcon mapIcon = new MapIcon();

                    mapIcon.Location = new Geopoint(snPosition);
                    mapIcon.Title = "Your Position";
                    MapControl1.MapElements.Add(mapIcon);
                    break;

                case GeolocationAccessStatus.Denied:

                    break;
            }
        }

        async private void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (e.Status)
                {
                    case PositionStatus.Ready:
                        //If its ready to get location data,you can add some code.  
                        break;

                    case PositionStatus.Initializing:
                        //Location is being initialized.waiting for it to complete.  
                        break;

                    case PositionStatus.NoData:
                        //Some places can not access location.Metros,Mountains,Elevators or fields with jammers.This case works when you're in one of them.  
                        break;

                    case PositionStatus.Disabled:
                        //You either rejected location access at start or closed Location.  
                        break;

                    case PositionStatus.NotInitialized:
                        //The app has not yet accessed location data.  
                        break;

                    case PositionStatus.NotAvailable:
                        //Location may not be possible due to OS settings.  
                        break;

                    default:
                        //If non of above works,this will.Writing a message helps.  
                        break;
                }
            });
        }
    }
}
