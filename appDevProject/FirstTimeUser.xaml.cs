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
            
            suggestions = new ObservableCollection<string>();
            getBusStops();
        }

        #region getBusStops - A method which makes a get request and retreves all the bus-stop data in the Co. Galway area. 
        async void getBusStops()
        {

            try
            {
                // Adapted from : https://www.youtube.com/watch?v=UMQ2JVOE_xE

                // Url where the get request will be sent too
                string url = "http://data.dublinked.ie/cgi-bin/rtpi/busstopinformation?&operator=BE&format=json%22";

                //
                HttpClient client = new HttpClient();
                // Making get request and storing responce in response2
                string response = await client.GetStringAsync(url);

                var busData = JsonConvert.DeserializeObject<Rootobject>(response);

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
                            MenuFlyoutItem item = new MenuFlyoutItem();
                            // Assigning it a name
                            item.Text = busData.results[i].fullname.ToString();
                            // Assigning it with a click event
                            item.Click += Item_Click1;

                            // Adding item into flyStops1 and flyStops2
                            flyStops1.Items.Add(item);  
                            flyStops2.Items.Add(item);
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
        private void Item_Click1(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem click = (MenuFlyoutItem)sender;
            tblStop1.Text = click.Text;
            stopName1 = click.Text;
            System.Diagnostics.Debug.WriteLine(stopName1);
            stop1Sel = true;
            System.Diagnostics.Debug.WriteLine("Stop 1 is: " + stop1Sel);
        }
        private void Item_Click2(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem click = (MenuFlyoutItem)sender;
            stopName2 = click.Text;
            tblStop2.Text = click.Text;
            System.Diagnostics.Debug.WriteLine(stopName2);
            stop2Sel = true;
            System.Diagnostics.Debug.WriteLine("Stop 2 is: " + stop2Sel);
        }
        private async void btnSubmit_ClickAsync(object sender, RoutedEventArgs e)
        {

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (stop1Sel == false || stop2Sel == false)
            {
                MessageDialog message = new MessageDialog("Two Bus-stop need to be selected before continuing..");
                await message.ShowAsync();
                System.Diagnostics.Debug.WriteLine("Not passed if statement..");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Passed if statement..");
                stopID1 = searchID(stopName1);
                stopID2 = searchID(stopName2);

                System.Diagnostics.Debug.WriteLine(stopID1);
                System.Diagnostics.Debug.WriteLine(stopID2);

                localSettings.Values["stopID1"] = stopID1;
                localSettings.Values["stopID2"] = stopID2;
                localSettings.Values["stopName1"] = stopName1;
                localSettings.Values["stopName2"] = stopName2;

                Frame.Navigate(typeof(MainPage));
            }
        }
        #endregion

        #region searchId - A method for searching the stopId for the selected stop name
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
        #endregion

        #region OnNavigated Methods
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values["IsFirstTime"] == null)
            {
                localSettings.Values["IsFirstTime"] = true;
            }

            if ((bool)localSettings.Values["IsFirstTime"])
            {
                localSettings.Values["IsFirstTime"] = false;
                this.Frame.Navigate(typeof(MainPage));
            }

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            showWelcomeMessageAsync();        
        }
        #endregion

        #region ShowWelcomeMessageAsync - When app is first opened a welcome message is shown to user 
        private async System.Threading.Tasks.Task showWelcomeMessageAsync()
        {
            MessageDialog message = new MessageDialog("Welcome to Commute! Please Select two Bus-Stops from the two select boxes on the on the right and Click the Submit Button.");
            await message.ShowAsync();
        }
        #endregion
    }
}
