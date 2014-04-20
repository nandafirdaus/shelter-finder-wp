using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Device.Location;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Maps.Controls;
using System.Threading.Tasks;
using TransJakartaLocator.Model;
using TransJakartaLocator.Utils;
using Newtonsoft.Json;

namespace TransJakartaLocator.Pages
{
    public partial class FromPoint : PhoneApplicationPage
    {
        private MapLayer shelterLocationLayer;
        private MapLayer myLocationLayer;
        private Shelter Nearest;

        public FromPoint()
        {
            InitializeComponent();

            MainMap.Center = new GeoCoordinate(-6.2297465, 106.829518);
            MainMap.ZoomLevel = 12;
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Center = new GeoCoordinate(-6.2297465, 106.829518);
            MainMap.ZoomLevel = 12;
            loadingProgressBar.IsVisible = true;
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += webClient_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(String.Format("https://maps.googleapis.com/maps/api/geocode/json?address={0}&sensor=false", searchText.Text)));
        }

        void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Result))
            {
                // Parse result
                var root = JsonConvert.DeserializeObject<SearchResult>(e.Result);

                loadingProgressBar.IsVisible = false;

                if (root.results.Count() == 0)
                {
                    MessageBox.Show("Place not found.", "Result", MessageBoxButton.OK);
                    return;
                }

                MainMap.Layers.Remove(shelterLocationLayer);
                MainMap.Layers.Remove(myLocationLayer);

                myLocationLayer = new MapLayer();

                foreach (var item in root.results)
                {
                    if (!item.formatted_address.Contains("Jakarta") &&
                        !item.formatted_address.Contains("Bekasi") &&
                        !item.formatted_address.Contains("Tangerang") &&
                        !item.formatted_address.Contains("Depok"))
                    {
                        continue;
                    }

                    Pushpin pushpin = new Pushpin();

                    // Generate pushpin content
                    StackPanel panel = new StackPanel();
                    TextBlock text = new TextBlock();
                    text.Text = item.formatted_address.Replace(",", ",\n");
                    panel.Children.Add(text);
                    panel.Visibility = Visibility.Collapsed;
                    pushpin.Content = panel;

                    pushpin.GeoCoordinate = new GeoCoordinate(item.geometry.location.lat, item.geometry.location.lng);
                    pushpin.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(UserLocationTap);

                    MapOverlay myLocationOverlay = new MapOverlay();
                    myLocationOverlay.Content = pushpin;
                    myLocationOverlay.PositionOrigin = new Point(0, 1);
                    myLocationOverlay.GeoCoordinate = pushpin.GeoCoordinate;

                    myLocationLayer.Add(myLocationOverlay);

                }

                // Add the MapLayer to the Map.
                MainMap.Layers.Add(myLocationLayer);
            }
        }

        private void UserLocationTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Pushpin location = sender as Pushpin;
            if (location.Content != null)
            {
                StackPanel content = (StackPanel)location.Content;
                if (content.Visibility == Visibility.Collapsed)
                {
                    foreach (var item in myLocationLayer)
                    {
                        Pushpin temp = item.Content as Pushpin;
                        (temp.Content as StackPanel).Visibility = Visibility.Collapsed;
                    }

                    content.Visibility = Visibility.Visible;
                }
                else if (content.Visibility == Visibility.Visible)
                {
                    Shelter nearest = GetNearestShelter(location.GeoCoordinate);

                    if (nearest != null)
                    {
                        content.Visibility = Visibility.Collapsed;

                        this.Nearest = nearest;

                        MainMap.Center = new System.Device.Location.GeoCoordinate(nearest.DoubleLat, nearest.DoubleLon);
                        MainMap.ZoomLevel = 13;

                        Pushpin pushpin = new Pushpin();

                        // Generate pushpin content
                        StackPanel panel = new StackPanel();
                        TextBlock text = new TextBlock();
                        text.Text = nearest.Name;
                        panel.Children.Add(text);
                        pushpin.Content = panel;

                        pushpin.GeoCoordinate = new GeoCoordinate(nearest.DoubleLat, nearest.DoubleLon);
                        pushpin.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(PushpinTap);

                        MapOverlay shelterLocationOverlay = new MapOverlay();
                        shelterLocationOverlay.Content = pushpin;
                        shelterLocationOverlay.PositionOrigin = new Point(0, 1);
                        shelterLocationOverlay.GeoCoordinate = pushpin.GeoCoordinate;

                        shelterLocationLayer = new MapLayer();
                        shelterLocationLayer.Add(shelterLocationOverlay);

                        // Add the MapLayer to the Map.
                        MainMap.Layers.Add(shelterLocationLayer);
                    }
                }
            }
            // to stop the event from going to the parent map control
            e.Handled = true;
        }

        private async void PushpinTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Pushpin pushpin = sender as Pushpin;
            if (pushpin.Content != null)
            {
                StackPanel content = (StackPanel)pushpin.Content;
                if (content.Visibility == Visibility.Collapsed)
                {
                    content.Visibility = Visibility.Visible;
                }
                else if (content.Visibility == Visibility.Visible)
                {
                    MessageBoxResult result = MessageBox.Show("Get navigation to the shelter?",
                        "Confirm", MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        await ShowRoute();
                    }
                }
            }
            // to stop the event from going to the parent map control
            e.Handled = true;
        }

        private async Task ShowRoute()
        {
            string latitude = Nearest.DoubleLat.ToString().Replace(",", ".");
            string longitude = Nearest.DoubleLon.ToString().Replace(",", ".");
            string name = Nearest.Name;

            // Assemble the Uri to launch.
            Uri uri = new Uri("ms-drive-to:?destination.latitude=" + latitude +
                "&destination.longitude=" + longitude + "&destination.name=" + name);

            // Launch the Uri.
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                // Uri launched.
            }
            else
            {
                // Uri failed to launch.
            }
        }

        private Shelter GetNearestShelter(GeoCoordinate geocoordinate)
        {
            string data = FileReader.ReadFile();

            if (data != null)
            {
                string[] datas = data.Split('\n');

                Shelter min = new Shelter();
                Shelter shelter = new Shelter();
                double distance = 1000;

                foreach (string item in datas)
                {
                    string[] temp = item.Split(',');

                    shelter = new Shelter();
                    shelter.Name = temp[0];
                    shelter.Longitude = int.Parse(temp[1]);
                    shelter.Latitude = int.Parse(temp[2]);

                    double latDifference = geocoordinate.Latitude - shelter.DoubleLat;
                    double lonDifference = geocoordinate.Longitude - shelter.DoubleLon;

                    double shelterDistance = Math.Sqrt(Math.Pow(latDifference, 2) + Math.Pow(lonDifference, 2));

                    if (shelterDistance < distance)
                    {
                        min = shelter;
                        distance = shelterDistance;
                    }
                }

                return min;
            }

            return new Shelter();
        }

        private void MainMap_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            GeoCoordinate location = this.MainMap.ConvertViewportPointToGeoCoordinate(e.GetPosition(this.MainMap));

            MainMap.Layers.Remove(shelterLocationLayer);
            MainMap.Layers.Remove(myLocationLayer);

            Pushpin pushpin = new Pushpin();

            // Generate pushpin content
            StackPanel panel = new StackPanel();
            TextBlock text = new TextBlock();
            text.Text = "Find shelter \nnear this point";
            panel.Children.Add(text);
            pushpin.Content = panel;

            pushpin.GeoCoordinate = new GeoCoordinate(location.Latitude, location.Longitude);
            pushpin.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(UserLocationTap);

            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = pushpin;
            myLocationOverlay.PositionOrigin = new Point(0, 1);
            myLocationOverlay.GeoCoordinate = pushpin.GeoCoordinate;

            myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);

            // Add the MapLayer to the Map.
            MainMap.Layers.Add(myLocationLayer);
        }
    }
}