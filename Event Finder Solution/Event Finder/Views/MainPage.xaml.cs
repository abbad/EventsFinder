using Bing.Maps;
using Event_Finder.Models;
using Newtonsoft.Json;
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
using Event_Finder.ViewModel;
using Event_Finder.Icons;
using System.ComponentModel;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Event_Finder.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // icons for the locaition
        LocationIcon10m _locationIcon10m;
        LocationIcon100m _locationIcon100m;
        // controller for facebook functions
        public FacebookViewModel fController;
        // controller for geolocation. 
        LocationController lController;
        // myPosition
        public Geoposition myPosition;
        //collection of pins
        private ObservableCollection<Event> PushpinCollection { get; set; }

        public ObservableCollection<Event> pushpinCollection
        {
            get
            {
                return PushpinCollection;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            lController = new LocationController();
            fController = new FacebookViewModel();
            _locationIcon10m = new LocationIcon10m();
            _locationIcon100m = new LocationIcon100m();

            PushpinCollection = new ObservableCollection<Event>();

            DataContext = this;
        }

        /// <summary>
        /// Function to clean map from children.
        /// </summary>
        private void CleanMap()
        {
           
            if (MainMap.Children.Count > 0)
            {
                MainMap.Children.RemoveAt(0);
            }
        }

        private void PositionUserOnMap() 
        {
            Location myLocation = new Location(myPosition.Coordinate.Point.Position.Latitude, myPosition.Coordinate.Point.Position.Longitude);

            try
            {
                // Default to IP level accuracy. We only show the region at this level - No icon is displayed.
                double zoomLevel = 11.0f;

                // if we have GPS level accuracy
                if (myPosition.Coordinate.Accuracy <= 10)
                {
                    // Add the 10m icon and zoom closer.
                    MainMap.Children.Add(_locationIcon10m);
                    MapLayer.SetPosition(_locationIcon10m, myLocation);
                    zoomLevel = 13.0f;
                }
                // Else if we have Wi-Fi level accuracy.
                if (myPosition.Coordinate.Accuracy <= 100)
                {
                    // Add the 100m icon and zoom a little closer.
                    MainMap.Children.Add(_locationIcon100m);
                    MapLayer.SetPosition(_locationIcon100m, myLocation);
                    zoomLevel = 13.0f;
                }

                // Set the map to the given location and zoom level.
                MainMap.SetView(myLocation, zoomLevel);


            }
            catch (System.UnauthorizedAccessException)
            {
               
            }
        }

        async private void OnLoad(object sender, RoutedEventArgs e)
        {
            myPosition = await lController.GetCurrentLocation();
            PositionUserOnMap();
            
            // get list of events from facebook. 
            List<Data> results = await fController.GetEventsFromFacebook(3, myPosition.Coordinate.Point.Position.Latitude, myPosition.Coordinate.Point.Position.Longitude, DateTimeConverter.DateTimeToUnixTimestamp(dateTimePicker.Date.Date));

            PositionEventsInTheMap(results);
            //PushpinCollection = new ObservableCollection<Event>();
            
            //context data
            MainMap.DataContext = this;

        }


        private void PositionEventsInTheMap(List<Data> results)
        {
            // loop through the list of results. 
            if (PushpinCollection.Count != 0) 
            {
                PushpinCollection.Clear();
            }
            foreach (var result in results)
            {
                foreach (var itemEvent in result.data)
                {                    
                    try
                    {
                        PushpinCollection.Add(new Event { name = itemEvent.name, Location = new Location(Convert.ToDouble(itemEvent.venue["latitude"]), Convert.ToDouble(itemEvent.venue["longitude"])) });
                    }
                    catch (Exception asda)
                    {
                       
                    }
                    
                }
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

        async private void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            //ListOfItems.Visibility = Windows.UI.Xaml.Visibility.Visible;
            List<Data> results =  await fController.GetEventsFromFacebook(3, myPosition.Coordinate.Point.Position.Latitude, myPosition.Coordinate.Point.Position.Longitude, DateTimeConverter.DateTimeToUnixTimestamp(dateTimePicker.Date.Date));
            PositionEventsInTheMap(results);
        }

        async private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            
            if (e.Key == Windows.System.VirtualKey.Enter) 
            {
                List<Data> results = await fController.SearchEventsFromFacebook(searchBox.QueryText, 3, myPosition.Coordinate.Point.Position.Latitude, myPosition.Coordinate.Point.Position.Longitude, DateTimeConverter.DateTimeToUnixTimestamp(dateTimePicker.Date.Date));
                PositionEventsInTheMap(results);
            }
        }

        // Omar my region

  

        

/*       private void pushPin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!Image.IsOpen) { Image.IsOpen = true; }
            var pushpinData = (sender as Pushpin).DataContext as tryPush;
            String file = pushpinData.ToString();
            // use image in popup here
        }*/
        
    }
}
