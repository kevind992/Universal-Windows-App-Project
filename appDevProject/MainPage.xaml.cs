﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using UWP_Main_App.Class_Files;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        private string stopName1;
        private string stopName2;
        private string stopID1;
        private string stopID2;
        private int stopTurn=0;

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
        }


        async void getSearchResults(string stop)
        {
            string url = "http://data.dublinked.ie/cgi-bin/rtpi/realtimebusinformation?stopid=" + stop + "&format=json";

            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(url);

            var data = JsonConvert.DeserializeObject<Rootobject2>(response);

            if (stopTurn == 0)
            {
                tbxStopName1.Text = stopName1;
                lvListBuses1.ItemsSource = data.results;
                stopTurn = 1;
            }
            else
            {
                tbxStopName2.Text = stopName2;
                lvListBuses2.ItemsSource = data.results;
                stopTurn = 0;
            }
        }

        private void Seattle_Click(object sender, RoutedEventArgs e)
        {
            Geopoint seattlePoint = new Geopoint
                (new BasicGeoposition { Latitude = 47.6062, Longitude = -122.3321 });

            PlaceInfo spaceNeedlePlace = PlaceInfo.Create(seattlePoint);

            FrameworkElement targetElement = (FrameworkElement)sender;

            GeneralTransform generalTransform =
                targetElement.TransformToVisual((FrameworkElement)targetElement.Parent);

            Rect rectangle = generalTransform.TransformBounds(new Rect(new Point
                (targetElement.Margin.Left, targetElement.Margin.Top), targetElement.RenderSize));

            spaceNeedlePlace.Show(rectangle, Windows.UI.Popups.Placement.Below);
        }
        private void showResults()
        {
            lvListBuses1.Visibility = Visibility.Visible;
            lvListBuses2.Visibility = Visibility.Visible;
        }
        private void pvtOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer localSettings =
                ApplicationData.Current.LocalSettings;

            localSettings.Values["currentOption"] = pvtOptions.SelectedIndex;
        }

    }
}
