using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using UWP_Main_App;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
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

            if(string.IsNullOrEmpty(stopID1) || string.IsNullOrEmpty(stopID2))
            {
                System.Diagnostics.Debug.WriteLine("Is empty..");
                localSettings.Values["IsFirstTime"] = true;
                this.Frame.Navigate(typeof(FirstTimeUser));
            }
            else
            {
                getSearchResults(stopID1,1);
                getSearchResults(stopID2,2);

                getBusStops();

                setMap();

                populateSettings();
            }
        }

        async void getSearchResults(string stop, int sPos)
        {
            try
            {
                string url = "http://data.dublinked.ie/cgi-bin/rtpi/realtimebusinformation?stopid=" + stop + "&format=json";

                HttpClient client = new HttpClient();

                string response = await client.GetStringAsync(url);

                var data = JsonConvert.DeserializeObject<Rootobject2>(response);

                if (sPos == 1)
                {
                    tbxStopName1.Text = stopName1;
                    lvListBuses1.ItemsSource = data.results;                
                }
                else
                {
                    tbxStopName2.Text = stopName2;
                    lvListBuses2.ItemsSource = data.results;
                }
            }
            catch
            {
                MessageDialog message = new MessageDialog("You have no Internet Data..");
                await message.ShowAsync();
            }
           
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refresh();
        }

        private void refresh()
        {
            System.Diagnostics.Debug.WriteLine("Refresh Selected..");

            lvListBuses1.ItemsSource = null;
            lvListBuses1.Items.Clear();
            lvListBuses2.ItemsSource = null;
            lvListBuses2.Items.Clear();

            getSearchResults(stopID1, 1);
            getSearchResults(stopID2, 2);
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

                        //Populating Setting Flyout 1
                        MenuFlyoutItem item1 = new MenuFlyoutItem();
                        item1.Text = busData.results[i].fullname.ToString();
                        item1.Click += Item1_Click; ;
                        flyStopsChange1.Items.Add(item1);
                        //Populating Setting Flyout 2
                        MenuFlyoutItem item2 = new MenuFlyoutItem();
                        item2.Text = busData.results[i].fullname.ToString();
                        item2.Click += Item2_Click; 
                        flyStopsChange2.Items.Add(item2);

                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine("Finished Loading bus stops for locations..");

            await setIconsAsync();
        }

        private void Item1_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Start ID1: "+ stopID1);
            MenuFlyoutItem click = (MenuFlyoutItem)sender;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            stopName1 = click.Text;
            stopID1 = searchID(stopName1);
            localSettings.Values["stopName1"] = stopName1;
            localSettings.Values["stopID1"] = stopID1;
            System.Diagnostics.Debug.WriteLine("End ID1: " + stopID1);
            tblStopChange1.Text = click.Text;
            
        }

        private void Item2_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Start ID2: " + stopID2);
            MenuFlyoutItem click = (MenuFlyoutItem)sender;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            stopName2 = click.Text;
            stopID2 = searchID(stopName2);
            localSettings.Values["stopName2"] = stopName2;
            localSettings.Values["stopID2"] = stopID2;
            System.Diagnostics.Debug.WriteLine("End ID2: " + stopID2);
            tblStopChange2.Text = click.Text;
        }

        private async Task setIconsAsync()
        {

            RandomAccessStreamReference mapBillboardStreamReference =
                RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/BusStopLogo.png"));

            for (int i = 0; i < galwayStops.Count; i++)
            {
                BasicGeoposition snPosition = new BasicGeoposition();
                snPosition.Latitude = galwayStops[i].latitude;
                snPosition.Longitude = galwayStops[i].longitude;

                MapIcon mapIcon = new MapIcon();

                mapIcon.Location = new Geopoint(snPosition);
                mapIcon.Title = galwayStops[i].shortname;
                mapIcon.Image = mapBillboardStreamReference;
                mapIcon.Tag = "Next Bus is in 2min";
                MapControl1.MapElements.Add(mapIcon);
            }
            System.Diagnostics.Debug.WriteLine("Bus Stop points added..");

            await getUserLocationAsync();
        }
        
        private async Task getUserLocationAsync()
        {
            var access = await Geolocator.RequestAccessAsync();


            RandomAccessStreamReference mapBillboardStreamReference =
                RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/UserLocation.png"));

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
                    mapIcon.Image = mapBillboardStreamReference;
                    MapControl1.MapElements.Add(mapIcon);
                    MapControl1.Center = new Geopoint(snPosition);
                    MapControl1.ZoomLevel = 17;

                    tileNotification((double)pos.Coordinate.Point.Position.Latitude, (double)pos.Coordinate.Point.Position.Longitude);

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

        private void tileNotification(double lat, double lon)
        {

            var uri = String.Format("http://busstopservice20180218022023.azurewebsites.net/?lat={0}&lon={1}", lat, lon);

            var tileContent = new Uri(uri);

            var requestedInterval = PeriodicUpdateRecurrence.HalfHour;

            var updater = TileUpdateManager.CreateTileUpdaterForApplication();

            updater.StartPeriodicUpdate(tileContent, requestedInterval);

        }

        private void populateSettings()
        {

            tblStopChange1.Text = stopName1;
            tblStopChange2.Text = stopName2;

        }

        private string searchID(string s)
        {
            string id = "";

            for (int i = 0; i < galwayStops.Count; i++)
            {
                if (galwayStops[i].fullname.Equals(s))
                {
                    id = galwayStops[i].stopid;
                }
            }
            return id;
        }
        private async void togLocation_ToggledAsync(object sender, RoutedEventArgs e)
        {
            var access = await Geolocator.RequestAccessAsync();

            if (togLocation.IsOn == true)
            {
                access = GeolocationAccessStatus.Allowed;
            }
            else
            {
                access = GeolocationAccessStatus.Denied;

            }
        }

        private void pvtOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refresh();
        }
    }
}
