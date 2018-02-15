using Newtonsoft.Json;
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

        private string stopID;

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

        async void getSearchResults()
        {
            //string url = "http://data.dublinked.ie/cgi-bin/rtpi/realtimebusinformation?stopid="+ stopID + "&format=json";

            //HttpClient client = new HttpClient();

            //string response = await client.GetStringAsync(url);

            //var data = JsonConvert.DeserializeObject<Rootobject>(response);

            //tblResults.Text = data.results[0].route + " | Next bus is in " + data.results[0].departureduetime + " min"; 

        }

        ObservableCollection<string> stops = new ObservableCollection<string>();

        async void getBusStops()
        {
            string url = "http://data.dublinked.ie/cgi-bin/rtpi/busstopinformation?&operator=BE&format=json%22";

            HttpClient client = new HttpClient();

            string response2 = await client.GetStringAsync(url);

            var busData = JsonConvert.DeserializeObject<Rootobject>(response2);
            int count = 0;
            //System.Diagnostics.Debug.WriteLine(busData.results[186].shortname.ToString());

            for (int i = 1; i < busData.numberofresults; i++)
            {
                if(busData.results[i].latitude < galLatHigh && busData.results[i].latitude > galLatLow)
                {
                    if(busData.results[i].longitude > galLongLeft && busData.results[i].longitude < galLongRight)
                    {
                        count++;
                        System.Diagnostics.Debug.WriteLine("Loading Data..");
                        MenuFlyoutItem item = new MenuFlyoutItem();
                        item.Text = busData.results[i].fullname.ToString();
                        item.Click += Item_Click;
                        flyStops1.Items.Add(item);
                        flyStops2.Items.Add(item);
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("Data Loaded..");
            System.Diagnostics.Debug.WriteLine(count);
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem click = (MenuFlyoutItem)sender;

            stopID = click.Text;

            System.Diagnostics.Debug.WriteLine(stopID);
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(stopID);

                getSearchResults();

                btnStops.Visibility = Visibility.Collapsed;
                btnSubmit.Visibility = Visibility.Collapsed;
                tblResults.Visibility = Visibility.Visible;
            }
            catch 
            {}
        }
    }
}
