using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using Windows.Devices.Geolocation;
using TransJakartaLocator.Model;
using TransJakartaLocator.Utils;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;
using System.Device.Location;
using System.Windows.Media;

namespace TransJakartaLocator.Pages
{
    public partial class GetNearest : PhoneApplicationPage
    {
        Shelter Nearest;

        public GetNearest()
        {
            InitializeComponent();
            MainMap.Center = new GeoCoordinate(-6.2297465, 106.829518);
            MainMap.ZoomLevel = 12;
            BuildMap();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                return;
            }
            else
            {
                MessageBoxResult result =
                    MessageBox.Show("Aplikasi ini membutuhkan akses ke lokasi ponsel. Apakah boleh?",
                    "Location", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                    return;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        private async void BuildMap()
        {
            loadingProgressBar.IsVisible = true;
            
            GeoCoordinate geocoordinate = await GetLocation();

            if (geocoordinate == null)
            {
                return;
            }

            Shelter nearest = GetNearestShelter(geocoordinate);

            loadingProgressBar.IsVisible = false;

            if (nearest != null)
            {
                this.Nearest = nearest;

                MainMap.Center = new System.Device.Location.GeoCoordinate(nearest.DoubleLat, nearest.DoubleLon);
                MainMap.ZoomLevel = 13;

                UserLocationMarker marker = new UserLocationMarker();
                marker.GeoCoordinate = new GeoCoordinate(geocoordinate.Latitude, geocoordinate.Longitude);

                Pushpin pushpin = new Pushpin();

                // Generate pushpin content
                StackPanel panel = new StackPanel();
                TextBlock text = new TextBlock();
                text.Text = nearest.Name;
                panel.Children.Add(text);
                pushpin.Content = panel;
                pushpin.Background = new SolidColorBrush(Color.FromArgb(255, 50, 50, 255));

                pushpin.GeoCoordinate = new GeoCoordinate(nearest.DoubleLat, nearest.DoubleLon);
                pushpin.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(PushpinTap);

                MapOverlay myLocationOverlay = new MapOverlay();
                myLocationOverlay.Content = marker;
                myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
                myLocationOverlay.GeoCoordinate = marker.GeoCoordinate;

                MapOverlay shelterLocationOverlay = new MapOverlay();
                shelterLocationOverlay.Content = pushpin;
                shelterLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
                shelterLocationOverlay.GeoCoordinate = pushpin.GeoCoordinate;

                // Create a MapLayer to contain the MapOverlay.
                MapLayer myLocationLayer = new MapLayer();
                myLocationLayer.Add(myLocationOverlay);

                MapLayer shelterLocationLayer = new MapLayer();
                shelterLocationLayer.Add(shelterLocationOverlay);

                // Add the MapLayer to the Map.
                MainMap.Layers.Add(myLocationLayer);
                MainMap.Layers.Add(shelterLocationLayer);

                //UserLocationMarker marker = (UserLocationMarker)this.FindName("UserLocationMarker");
                //marker.GeoCoordinate = new GeoCoordinate(geocoordinate.Latitude, geocoordinate.Longitude);

                //Pushpin pushpin = (Pushpin)this.FindName("ShelterPushpin");
                //pushpin.Content = nearest.Name;
                //pushpin.GeoCoordinate = new GeoCoordinate(nearest.DoubleLat, nearest.DoubleLon);
            }

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
                } else if (content.Visibility == Visibility.Visible) {
                    MessageBoxResult result = MessageBox.Show("Lihat navigasi menuju shelter?",
                        "Konfirmasi", MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        await ShowRoute();
                    }
                }
            }
            // to stop the event from going to the parent map control
            e.Handled = true;
        }

        private async Task<GeoCoordinate> GetLocation()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent") && !(bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"])
            {
                return null;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(2),
                    timeout: TimeSpan.FromSeconds(20)
                    );

                return new GeoCoordinate(
                    geoposition.Coordinate.Latitude,
                    geoposition.Coordinate.Longitude,
                    geoposition.Coordinate.Altitude ?? Double.NaN,
                    geoposition.Coordinate.Accuracy,
                    geoposition.Coordinate.AltitudeAccuracy ?? Double.NaN,
                    geoposition.Coordinate.Speed ?? Double.NaN,
                    geoposition.Coordinate.Heading ?? Double.NaN
                    );
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    MessageBox.Show("Location tidak diaktifkan. Aktifkan melalui pengaturan ponsel",
                        "Location", MessageBoxButton.OK);
                    
                }
                else
                {
                    MessageBox.Show("Error",
                        "Terjadi kesalahan.", MessageBoxButton.OK);
                }

                return null;
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

                    double shelterDistance = Math.Sqrt(Math.Pow(latDifference, 2) + Math.Pow(lonDifference,2));

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

        private async void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            await ShowRoute();
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
    }
}