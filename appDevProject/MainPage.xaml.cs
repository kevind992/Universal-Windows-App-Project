using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        #region Variables
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

        #endregion

        public MainPage()
        {
            this.InitializeComponent();
        }

        #region HTTP Get Methods - Used for getting data from the api
        // Method for getting all the bus times
        async void getSearchResults(string stop, int sPos)
        {
            // Adapted from : https://www.youtube.com/watch?v=UMQ2JVOE_xE

            try
            {
                // Url which is used for making the get request
                string url = "http://data.dublinked.ie/cgi-bin/rtpi/realtimebusinformation?stopid=" + stop + "&format=json";
                //Creating a Http client
                HttpClient client = new HttpClient();
                // Making the request and putting the response into responce
                string response = await client.GetStringAsync(url);
                // Parsing the data using NewtonJson package
                var data = JsonConvert.DeserializeObject<RootBusStopTimeObject>(response);

                if (sPos == 1) // if sPos is 1 then add to the top listbox on the bus times pivot page
                {
                    tbxStopName1.Text = stopName1;
                    lvListBuses1.ItemsSource = data.results;                
                }
                else if (sPos == 3) // if sPos is 3 then add to the map listbox
                {
                    lvListMapTimes.ItemsSource = data.results;
                }
                else // else add to the bottem listbox on the bus times pivot page
                {
                    tbxStopName2.Text = stopName2;
                    lvListBuses2.ItemsSource = data.results;
                }
            }
            catch
            {
                // Display message to the user
                MessageDialog message = new MessageDialog("You have no Internet Data..");
                await message.ShowAsync();
            }
           
        }
        // Method used to gathering all the bus stops in the Co. Galway area
        private async void getBusStops()
        {

            // Adapted from : https://www.youtube.com/watch?v=UMQ2JVOE_xE

            // Url which is used for making the get request
            string url = "http://data.dublinked.ie/cgi-bin/rtpi/busstopinformation?&operator=BE&format=json%22";
            //Creating a Http client
            HttpClient client = new HttpClient();
            // Making the request and putting the response into responce2
            string response2 = await client.GetStringAsync(url);
            // Parsing the data using NewtonJson package
            var busData = JsonConvert.DeserializeObject<RootBusStopobject>(response2);

            // An algorithm to filter throught all the bus stops and store the stops which are in the galway area.
            // This is done by setting 4 points using Latitude and Longtitude.
            for (int i = 1; i < busData.numberofresults; i++)
            {
                // If bus stop is within set latitudes
                if (busData.results[i].latitude < galLatHigh && busData.results[i].latitude > galLatLow)
                {
                    // If bus stop is within set logitudes
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
        #endregion

        #region refesh - method to refresh bus times
        private void refresh()
        {
            System.Diagnostics.Debug.WriteLine("Refresh Selected..");

            // Emptying both listboxs
            lvListBuses1.ItemsSource = null;
            lvListBuses1.Items.Clear();
            lvListBuses2.ItemsSource = null;
            lvListBuses2.Items.Clear();
            //Repopulating listboxes
            getSearchResults(stopID1, 1);
            getSearchResults(stopID2, 2);
        }
        #endregion

        #region searchId Method - used for searching for the id's of the selected bus stop names
        private string searchID(string s)
        {
            // creating an empty string
            string id = "";
            //Looping throught all the stops
            for (int i = 0; i < galwayStops.Count; i++)
            {
                // if the 2 stops match
                if (galwayStops[i].fullname.Equals(s))
                {
                    // store the id
                    id = galwayStops[i].stopid;
                }
            }
            // Returning the id
            return id;
        }
        #endregion

        #region Tile Notification Method
        private void tileNotification(double lat, double lon)
        {
            // Adapted from : https://channel9.msdn.com/Series/Windows-10-development-for-absolute-beginners/UWP-061-UWP-Weather-Updating-the-Tile-with-Periodic-Notifications

            // Url of Azure Service which sends latitude and longitude
            var uri = String.Format("http://busstopservice20180218022023.azurewebsites.net/?lat={0}&lon={1}", lat, lon);
            // Setting tile
            var tileContent = new Uri(uri);
            // Setting update of tile for every half hour       
            var requestedInterval = PeriodicUpdateRecurrence.HalfHour;
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            // Starting tile notification
            updater.StartPeriodicUpdate(tileContent, requestedInterval);

        }
        #endregion

        #region Location and Map Methods
        // Method for positioning the map over galway city
        private void setMap()
        {
            // Map will line up the the selected latitude and longitude
            BasicGeoposition cityPosition = new BasicGeoposition()
            {
                Latitude = 53.281551,
                Longitude = -9.035187
            };

            Geopoint cityCentre = new Geopoint(cityPosition);

            MapControl1.Center = cityCentre;
            MapControl1.LandmarksVisible = true;
            MapControl1.ZoomLevel = 12;
        }
        // Method for adding bus stop icons to the map
        private async Task setIconsAsync()
        {
            // Adapted from : https://docs.microsoft.com/en-us/windows/uwp/maps-and-location/display-poi

            // Creating a Random access stream reference for the bus stop icon
            RandomAccessStreamReference mapStreamReference =
                RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/BusStopLogo.png"));

            //for every bus stop
            for (int i = 0; i < galwayStops.Count; i++)
            {
                // Create a basic geoposition
                BasicGeoposition snPosition = new BasicGeoposition();
                // Set the latitude and longitude
                snPosition.Latitude = galwayStops[i].latitude;
                snPosition.Longitude = galwayStops[i].longitude;
                //Create new Map icon
                MapIcon mapIcon = new MapIcon();
                // give it a tag of bus stop id
                mapIcon.Tag = galwayStops[i].displaystopid;
                // Set the location of the map icon
                mapIcon.Location = new Geopoint(snPosition);
                // Set the name of the map icon
                mapIcon.Title = galwayStops[i].shortname;
                // Set the image
                mapIcon.Image = mapStreamReference;
                // Add the icon to the map
                MapControl1.MapElements.Add(mapIcon);
            }
            System.Diagnostics.Debug.WriteLine("Bus Stop points added..");

            await getUserLocationAsync();
        }
        private void populateSettings()
        {

            tblStopChange1.Text = stopName1;
            tblStopChange2.Text = stopName2;

        }
        private async Task getUserLocationAsync()
        {
            //Adapted from : https://docs.microsoft.com/en-us/windows/uwp/maps-and-location/get-location

            // Request access status
            var access = await Geolocator.RequestAccessAsync();

            // Create a randon access Stream reference of the location icon
            RandomAccessStreamReference mapStreamReference =
                RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/UserLocation.png"));

            switch (access)
            {
                case GeolocationAccessStatus.Allowed: // If access status is allowed

                    // Creata a new geolocator with a desired accuracy of what ever is possible
                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 0 };
                    geolocator.StatusChanged += Geolocator_StatusChanged;
                    // Getting position of user
                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    // Creating a Basic Geoposition
                    BasicGeoposition snPosition = new BasicGeoposition();

                    // setting snPosition latitude and longitude with the pos coodinates of the user
                    snPosition.Latitude = (float)pos.Coordinate.Point.Position.Latitude;
                    snPosition.Longitude = (float)pos.Coordinate.Point.Position.Longitude;

                    //Creating a new map icon
                    MapIcon mapIcon = new MapIcon();
                    // Setting the location of the map icon
                    mapIcon.Location = new Geopoint(snPosition);
                    // Setting title
                    mapIcon.Title = "Your Position";
                    // Setting map icon image
                    mapIcon.Image = mapStreamReference;
                    // Adding icon to the map
                    MapControl1.MapElements.Add(mapIcon);
                    // Zooming map around user
                    MapControl1.Center = new Geopoint(snPosition);
                    MapControl1.ZoomLevel = 17;
                   
                    // Updating Tile
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
        #endregion

        #region Pivot Option Changed Event Method
        private void pvtOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Calling the refresh button
            refresh();
        }
        #endregion

        #region CLick and Tog Event Methods

        private async void togLocation_ToggledAsync(object sender, RoutedEventArgs e)
        {
            // Checking access
            var access = await Geolocator.RequestAccessAsync();

            // if access is true
            if (togLocation.IsOn == true)
            {
                // Allowed to access users location
                access = GeolocationAccessStatus.Allowed;
            }
            else
            {
                // Disallowed to access user location
                access = GeolocationAccessStatus.Denied;
            }
        }
        private void MapControl1_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {  
            // Event for when user clicks a bus stop on the map
            MapIcon myClickedIcon = args.MapElements.FirstOrDefault(x => x is MapIcon) as MapIcon;
            // Making a grid visable which displays bus arrival times
            grdMapStopTimes.Visibility = Visibility.Visible;      
            // Populating grid
            getSearchResults((string)myClickedIcon.Tag, 3);
        }
        private void btnCloseBox_Click(object sender, RoutedEventArgs e)
        {
            // Collapsing grid
            grdMapStopTimes.Visibility = Visibility.Collapsed;
            // Clearing results from grid
            lvListMapTimes.ItemsSource = null;
            lvListMapTimes.Items.Clear();

        }
        private void Item1_Click(object sender, RoutedEventArgs e)
        {
            // Getting an instance of the selected menu flyout item
            MenuFlyoutItem click = (MenuFlyoutItem)sender;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            //Storing the new selection 
            stopName1 = click.Text;
            stopID1 = searchID(stopName1);
            localSettings.Values["stopName1"] = stopName1;
            localSettings.Values["stopID1"] = stopID1;
            System.Diagnostics.Debug.WriteLine("End ID1: " + stopID1);
            tblStopChange1.Text = click.Text;

        }
        private void Item2_Click(object sender, RoutedEventArgs e)
        {
            // Getting an instance of the selected menu flyout item
            MenuFlyoutItem click = (MenuFlyoutItem)sender;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            
            //Storing the new selection 
            stopName2 = click.Text;
            stopID2 = searchID(stopName2);
            localSettings.Values["stopName2"] = stopName2;
            localSettings.Values["stopID2"] = stopID2;
            System.Diagnostics.Debug.WriteLine("End ID2: " + stopID2);
            tblStopChange2.Text = click.Text;
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Calling the refresh button
            refresh();
        }
        #endregion

        #region OnNavigatedTo
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            // Assigning variables with the localSettings of stopID1, stopID2, stopName1 and stopName2
            stopID1 = (string)localSettings.Values["stopID1"];
            stopID2 = (string)localSettings.Values["stopID2"];
            stopName1 = (string)localSettings.Values["stopName1"];
            stopName2 = (string)localSettings.Values["stopName2"];

            // If either stopID1 or stopID2 are empty return to FirstTimeUser page 
            if (string.IsNullOrEmpty(stopID1) || string.IsNullOrEmpty(stopID2))
            {
                System.Diagnostics.Debug.WriteLine("Is empty..");
                localSettings.Values["IsFirstTime"] = true;
                // Navigating back to FirstTimeUser page
                this.Frame.Navigate(typeof(FirstTimeUser));
            }
            else
            {
                // Populate listboxes with bus arrival times
                getSearchResults(stopID1, 1);
                getSearchResults(stopID2, 2);
                // Get bus stops for settings pivot
                getBusStops();
                // Set map
                setMap();
                // Populate map with icons
                populateSettings();
            }
        }
        #endregion

    }
}
