using Bing.Maps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Event_Finder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Geolocator geolocator = null;
        // member variables to add
        LocationIcon10m _locationIcon10m;
        LocationIcon100m _locationIcon100m;

        public MainPage()
        {
            this.InitializeComponent();
            geolocator = new Geolocator();

            _locationIcon10m = new LocationIcon10m();
            _locationIcon100m = new LocationIcon100m();
        }

        async private void OnLoad(object sender, RoutedEventArgs e)
        {
            if (MainMap.Children.Count > 0)
            {
                MainMap.Children.RemoveAt(0);
            }

            try{
            
                // Get the location.
                Geoposition pos = await geolocator.GetGeopositionAsync();
                Location location = new Location(pos.Coordinate.Point.Position.Latitude, pos.Coordinate.Point.Position.Longitude);
                //TestingBlock.Text = GetStatusString(pos.Coordinate.)
                // Now set the zoom level of the map based on the accuracy of our location data.
                // Default to IP level accuracy. We only show the region at this level - No icon is displayed.
                double zoomLevel = 11.0f;


                // if we have GPS level accuracy
                if (pos.Coordinate.Accuracy <= 10)
                {
                    // Add the 10m icon and zoom closer.
                    MainMap.Children.Add(_locationIcon10m);
                    MapLayer.SetPosition(_locationIcon10m, location);
                    zoomLevel = 13.0f;
                }
                // Else if we have Wi-Fi level accuracy.
                else //if (pos.Coordinate.Accuracy <= 100)
                {
                    // Add the 100m icon and zoom a little closer.
                    MainMap.Children.Add(_locationIcon100m);
                    MapLayer.SetPosition(_locationIcon100m, location);
                    zoomLevel = 13.0f;
                }
                

                // Set the map to the given location and zoom level.
                MainMap.SetView(location, zoomLevel);

            }
            catch (System.UnauthorizedAccessException)
            {
                TestingBlock.Text = "No data";
            }
            
            
        }

        /// <summary>
        /// This function will give the status of your position.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private string GetStatusString(PositionStatus status)
        {
            var strStatus = "";

            switch (status)
            {
                case PositionStatus.Ready:
                    
                    strStatus = "Location is available.";
                    break;

                case PositionStatus.Initializing:
                    strStatus = "Geolocation service is initializing.";
                    break;

                case PositionStatus.NoData:
                    strStatus = "Location service data is not available.";
                    break;

                case PositionStatus.Disabled:
                    strStatus = "Location services are disabled. Use the " +
                                "Settings charm to enable them.";
                    break;

                case PositionStatus.NotInitialized:
                    strStatus = "Location status is not initialized because " +
                                "the app has not yet requested location data.";
                    break;

                case PositionStatus.NotAvailable:
                    strStatus = "Location services are not supported on your system.";
                    break;

                default:
                    strStatus = "Unknown PositionStatus value.";
                    break;
            }

            return (strStatus);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn1 = new Button();
            btn1.Content = "Musical Events";
            btn1.Width = 82;
            btn1.Height = 79;
            btn1.Margin = new Thickness(
                30, 54, 0, 506);
            MainGrid.Children.Add(btn1);
            
            //<Button Content="Button" Height="79" Width="82" Margin="7,54,0,605" Grid.Row="1" Click="Button_Click"/>
            }
        
    }
}
