using appDevProject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Main_App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FirstTimeUser : Page
    {
        #region Variables
        private string stopName1;
        private string stopName2;

        private string stopID1;
        private string stopID2;

        private bool stop1Sel = false;
        private bool stop2Sel = false;

        private List<Result> galwayStops = new List<Result>();
        ObservableCollection<string> stops = new ObservableCollection<string>();
        private ObservableCollection<String> suggestions;
        
        //Latitude and Longitude for area around Galway City
        private double galLatLow = 53.01347187;
        private double galLatHigh = 53.54880427;
        private double galLongLeft = -9.58028032;
        private double galLongRight = -8.4061165;
        #endregion
        public FirstTimeUser()
        {
            this.InitializeComponent();

            // Running the getBusStops method which populates the the fly menus
            getBusStops();
        }

        #region getBusStops - A method which makes a get request and retreves all the bus-stop data in the Co. Galway area. 
        async void getBusStops()
        {
            // Adpted from : https://www.youtube.com/watch?v=UMQ2JVOE_xE

            try
            {
                // Adapted from : https://www.youtube.com/watch?v=UMQ2JVOE_xE

                // Url where the get request will be sent too
                string url = "http://data.dublinked.ie/cgi-bin/rtpi/busstopinformation?&operator=BE&format=json%22";

                // Using HttpClient
                HttpClient client = new HttpClient();
                // Making get request and storing responce in response2
                string response = await client.GetStringAsync(url);
                // Using Newtonsoft Json to parse the responce. 
                // This uses c# class BusStopData
                var busData = JsonConvert.DeserializeObject<RootBusStopobject>(response);

                System.Diagnostics.Debug.WriteLine("Loading Data..");

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
                            // Creating an instance of MenuFlyoutItem
                            MenuFlyoutItem item1 = new MenuFlyoutItem();
                            // Assigning it a name
                            item1.Text = busData.results[i].fullname.ToString();
                            // Assigning it with a click event
                            item1.Click += Item_Click1;
                            // Adding item into flyStops1
                            flyStops1.Items.Add(item1);

                            // Creating another instance of MenuFlyoutItem
                            MenuFlyoutItem item2 = new MenuFlyoutItem();
                            // Assigning it a name
                            item2.Text = busData.results[i].fullname.ToString();
                            // Assigning it with a click event
                            item2.Click += Item_Click2;
                            // Adding item into flyStops2
                            flyStops2.Items.Add(item2);

                            galwayStops.Add(busData.results[i]);
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine("Data Loaded..");
            }
            catch
            {
                // Displaying message to user if no data is avaible
                MessageDialog message = new MessageDialog("You have no Internet Data..");
                await message.ShowAsync();
            }
           
        }
        #endregion
       
        #region Click Methods
        // Click event for selected stop1
        private void Item_Click1(object sender, RoutedEventArgs e)
        {
            // Getting an instance of selected Menu Flyout item
            MenuFlyoutItem click = (MenuFlyoutItem)sender;
            // Assigning tblStop1 with the text of the selected flyout item
            tblStop1.Text = click.Text;
            stopName1 = click.Text;
            // stop1 has been selected for set stopSel to true
            stop1Sel = true;
        }
        // Click event for selected stop2
        private void Item_Click2(object sender, RoutedEventArgs e)
        {
            // Getting an instance of selected Menu Flyout item
            MenuFlyoutItem click = (MenuFlyoutItem)sender;
            // Assigning tblStop2 with the text of the selected flyout item
            stopName2 = click.Text;
            tblStop2.Text = click.Text;
            // stop2 has been selected for set stopSel to true
            stop2Sel = true;
        }
        // Button click event for submit button
        private async void btnSubmit_ClickAsync(object sender, RoutedEventArgs e)
        {
            // Setting localSettings 
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            
            // if both stop1Sel and stop2Sel are false which means that two stops were not selected. 
            if (stop1Sel == false || stop2Sel == false)
            {
                // Display a message to the user
                MessageDialog message = new MessageDialog("Two Bus-stop need to be selected before continuing..");
                await message.ShowAsync();
            }
            else // Both stops were selected, user can progress
            {
                // Getting the stopId's for the two selected stops 
                stopID1 = searchID(stopName1);
                stopID2 = searchID(stopName2);

                // Storing stopID1, stopID2, StopName1 and stopName2 to localSettings
                localSettings.Values["stopID1"] = stopID1;
                localSettings.Values["stopID2"] = stopID2;
                localSettings.Values["stopName1"] = stopName1;
                localSettings.Values["stopName2"] = stopName2;

                // Navigate to MainPage once the localSettings have been stored
                Frame.Navigate(typeof(MainPage));
            }
        }
        #endregion

        #region searchId - A method for searching the stopId for the selected stop name
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

        #region OnNavigated Methods
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Getting local settings
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            //If localSettings - IsFirstTime is null, i.e first time app has been run
            if (localSettings.Values["IsFirstTime"] == null)
            {
                // set to true
                localSettings.Values["IsFirstTime"] = true;
            }
            //if contains a bool, i.e. not null
            if ((bool)localSettings.Values["IsFirstTime"])
            {
                // Set to false
                localSettings.Values["IsFirstTime"] = false;
                // Navigate to MainPage
                this.Frame.Navigate(typeof(MainPage));
            }

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Displaying welcome message when user arrives for the first time
            showWelcomeMessageAsync();        
        }
        #endregion

        #region ShowWelcomeMessageAsync - When app is first opened a welcome message is shown to user 
        private async System.Threading.Tasks.Task showWelcomeMessageAsync()
        {
            // Welcome Message shown to user when app first run
            MessageDialog message = new MessageDialog("Welcome to Commute! Please Select two Bus-Stops from the two select boxes on the on the right and Click the Submit Button.");
            await message.ShowAsync();
        }
        #endregion
    }
}
