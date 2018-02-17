using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace appDevProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int stopTurn = 0;

        private string stopName1;
        private string stopName2;

        private string stopID1;
        private string stopID2;

        private List<Result> galwayStops = new List<Result>();

        //Latitude and Longitude for area around Galway City
        private double galLatLow = 53.01347187;
        private double galLatHigh = 53.54880427;
        private double galLongLeft = -9.58028032;
        private double galLongRight = -8.4061165;

        public MainPage()
        {
            this.InitializeComponent();
            getBusStops();
        }

        async void getSearchResults(string stop)
        {
            string url = "http://data.dublinked.ie/cgi-bin/rtpi/realtimebusinformation?stopid="+ stop + "&format=json";

            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(url);

            var data = JsonConvert.DeserializeObject<Rootobject2>(response);

            if(stopTurn == 0)
            {
                lvListBuses1.ItemsSource = data.results;
                stopTurn = 1;
            }
            else
            {
                lvListBuses2.ItemsSource = data.results;
                stopTurn = 0;
            }

            

        }

        ObservableCollection<string> stops = new ObservableCollection<string>();

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
                stopID1 = searchID(stopName1);
                stopID2 = searchID(stopName2);

                System.Diagnostics.Debug.WriteLine(stopID1);
                System.Diagnostics.Debug.WriteLine(stopID2);

                getSearchResults(stopID1);
                getSearchResults(stopID2);

                hideOptions();
                showResults();

            }
            catch 
            {}
        }
        private string searchID(string s)
        {
            string id = "";

            for(int i = 0; i < galwayStops.Count;i++)
            {
                if(galwayStops[i].fullname.Equals(s))
                {
                    id = galwayStops[i].stopid;
                }
            }
            return id;
        }
        private void hideOptions()
        {
            btnStops1.Visibility = Visibility.Collapsed;
            btnStops2.Visibility = Visibility.Collapsed;
            btnSubmit.Visibility = Visibility.Collapsed;
            tblStop1.Visibility = Visibility.Collapsed;
            tblStop2.Visibility = Visibility.Collapsed;
        }
        private void showResults()
        {
            lvListBuses1.Visibility = Visibility.Visible;
            lvListBuses2.Visibility = Visibility.Visible;
        }
    }
}
