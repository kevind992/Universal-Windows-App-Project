using appDevProject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using UWP_Main_App.Class_Files;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Notifications;
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

        private string stopName1;
        private string stopName2;

        private string stopID1;
        private string stopID2;

        private List<Result> galwayStops = new List<Result>();
        ObservableCollection<string> stops = new ObservableCollection<string>();
        private ObservableCollection<String> suggestions;

        //Latitude and Longitude for area around Galway City
        private double galLatLow = 53.01347187;
        private double galLatHigh = 53.54880427;
        private double galLongLeft = -9.58028032;
        private double galLongRight = -8.4061165;

        public FirstTimeUser()
        {
            this.InitializeComponent();
            suggestions = new ObservableCollection<string>();
            getBusStops();
        }

        async void getBusStops()
        {
            string url = "http://data.dublinked.ie/cgi-bin/rtpi/busstopinformation?&operator=BE&format=json%22";

            HttpClient client = new HttpClient();

            string response2 = await client.GetStringAsync(url);

            var busData = JsonConvert.DeserializeObject<Rootobject>(response2);


            System.Diagnostics.Debug.WriteLine("Inside If..");
            System.Diagnostics.Debug.WriteLine("Loading Data..");

            for (int i = 1; i < busData.numberofresults; i++)
            {
                if (busData.results[i].latitude < galLatHigh && busData.results[i].latitude > galLatLow)
                {
                    if (busData.results[i].longitude > galLongLeft && busData.results[i].longitude < galLongRight)
                    {
                        MenuFlyoutItem item = new MenuFlyoutItem();
                        item.Text = busData.results[i].fullname.ToString();
                        //suggestions.Add(busData.results[i].fullname.ToString());
                        item.Click += Item_Click1;
                        flyStops1.Items.Add(item);
                        
                        galwayStops.Add(busData.results[i]);
                    }
                }
                if (busData.results[i].latitude < galLatHigh && busData.results[i].latitude > galLatLow)
                {
                    if (busData.results[i].longitude > galLongLeft && busData.results[i].longitude < galLongRight)
                    {
                        MenuFlyoutItem item = new MenuFlyoutItem();
                        item.Text = busData.results[i].fullname.ToString();
                        item.Click += Item_Click2;
                        flyStops2.Items.Add(item);
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("Data Loaded..");
        }

        private void Item_Click1(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem click = (MenuFlyoutItem)sender;
            tblStop1.Text = click.Text;
            stopName1 = click.Text;
            System.Diagnostics.Debug.WriteLine(stopName1);
        }

        private void Item_Click2(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem click = (MenuFlyoutItem)sender;
            stopName2 = click.Text;
            tblStop2.Text = click.Text;
            System.Diagnostics.Debug.WriteLine(stopName2);
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parameters = new StopParameters();

                stopID1 = searchID(stopName1);
                stopID2 = searchID(stopName2);

                parameters.StopName1 = stopName1;
                parameters.StopName2 = stopName2;
                parameters.StopID1 = stopID1;
                parameters.StopID2 = stopID2;


                System.Diagnostics.Debug.WriteLine(stopID1);
                System.Diagnostics.Debug.WriteLine(stopID2);

                //getSearchResults(stopID1);
                // getSearchResults(stopID2);

                Frame.Navigate(typeof(MainPage), parameters);

                tileNotification(stopID1, stopName1);

                //hideOptions();
               
            }
            catch
            { }
        }
        private void tileNotification(string id, string name)
        {

            var uri = String.Format("http://busstopservice20180218022023.azurewebsites.net/?stopID={0}&stopName={1}", id, name);

            var tileContent = new Uri(uri);

            var requestedInterval = PeriodicUpdateRecurrence.HalfHour;

            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.StartPeriodicUpdate(tileContent, requestedInterval);

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
      
        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)

                autoListbx1.Text = args.ChosenSuggestion.ToString();

            else

                autoListbx1.Text = sender.Text;
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            autoListbx1.Text = "Choosen";
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {

                suggestions.Clear();
                suggestions.Add("1");
                suggestions.Add("2");
                suggestions.Add("3");
                suggestions.Add("4");
                suggestions.Add("5");
                suggestions.Add("6");
                suggestions.Add("7");
                sender.ItemsSource = suggestions;
            }
        }
    }
}
