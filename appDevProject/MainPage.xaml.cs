using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
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

        public string stopID;

        public MainPage()
        {
            this.InitializeComponent();
            
        }

        async void getSearchResults()
        {
            string url = "http://data.dublinked.ie/cgi-bin/rtpi/realtimebusinformation?stopid="+ stopID + "&format=json";

            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(url);

            var data = JsonConvert.DeserializeObject<Rootobject>(response);

            tblResults.Text = data.results[0].route + " | Next bus is in " + data.results[0].departureduetime + " min"; 

        }
        async void getBusStops()
        {
            string url = "http://data.dublinked.ie/cgi-bin/rtpi/busstopinformation?&format=json%22;";

            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(url);

            var data = JsonConvert.DeserializeObject<RootBusStopobject>(response);


                        
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            stopID = tbxSubmit.Text;

            System.Diagnostics.Debug.WriteLine(stopID);

            getData();

            tbxSubmit.Visibility = Visibility.Collapsed;
            btnSubmit.Visibility = Visibility.Collapsed;
            tblResults.Visibility = Visibility.Visible;
        }
       
        
    }
}
