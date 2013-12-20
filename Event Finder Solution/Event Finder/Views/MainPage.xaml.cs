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
using Windows.UI.Popups;
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
        // application location
        public Location myLocation;
        //collection of pins
        private TextBlock textBlock;

        private double offset = 1;

        private String cityName = "";
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
            endRangeDateTimePicker.Date = DateTime.Today.AddDays(5);
            lController = new LocationController();
            fController = new FacebookViewModel();
            _locationIcon10m = new LocationIcon10m();
            _locationIcon100m = new LocationIcon100m();
            textBlock = CreateTextBlock();
            MainMap.Children.Add(textBlock);
            textBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            PushpinCollection = new ObservableCollection<Event>();
            MainMap.Children.Add(_locationIcon100m);
            _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            DataContext = this;
        }

        private void PositionUserOnMap(Geoposition myPosition) 
        {
            myLocation = new Location(myPosition.Coordinate.Point.Position.Latitude, myPosition.Coordinate.Point.Position.Longitude);
            
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
            prog.IsActive = true;

            List<Data> results;
            Geoposition myPosition = await lController.GetCurrentLocation();
            PositionUserOnMap(myPosition);
            // get the city name from reverse geocodeing
            cityName = await lController.ReverseGeocodePoint(
                new Location(myPosition.Coordinate.Point.Position.Latitude, myPosition.Coordinate.Point.Position.Longitude));
            

            results = await fController.GetAllEvents(cityName, offset, myPosition.Coordinate.Point.Position.Latitude,
                myPosition.Coordinate.Point.Position.Longitude,
                DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date));
            PositionEventsInTheMap(results);
           
            //context data
            MainMap.DataContext = this;
            prog.IsActive = false;

        }


        private void PositionEventsInTheMap(List<Data> results)
        {
            // loop through the list of results. 
            if (PushpinCollection.Count != 0) 
            {
                PushpinCollection.Clear();
            }
            string venueName1 = "";
            prog.IsActive = true;
            foreach (var result in results)
            {
                foreach (var itemEvent in result.data)
                {
                    try
                    {
                        venueName1 = itemEvent.venue["name"];
                    }
                    catch (Exception e) { }
                    try
                    {

                        PushpinCollection.Add(
                            new Event { 
                                eid = itemEvent.eid,
                                name = itemEvent.name, 
                                Location = new Location(Convert.ToDouble(itemEvent.venue["latitude"]), Convert.ToDouble(itemEvent.venue["longitude"])),
                                venueName = venueName1,
                                pic_square = itemEvent.pic_square,
                                pic_big = itemEvent.pic_big,
                                description = itemEvent.description,
                                start_time = itemEvent.start_time,
                                end_time = itemEvent.end_time,
                            });
                    }
                    catch (Exception asda)
                    {
                       
                    }
                    
                }
            }
            prog.IsActive = false;

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
            prog.IsActive = true;

            if (cityName == null)
            {
                cityName = await lController.ReverseGeocodePoint(
                    new Location(myLocation.Latitude, myLocation.Longitude));
            }

            List<Data> results = await fController.GetAllEvents(cityName, offset,
                myLocation.Latitude,
                myLocation.Longitude, 
                DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date)
                );
            PositionEventsInTheMap(results);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Event selectedEvent = (Event)e.ClickedItem;
            MainMap.SetView(selectedEvent.Location, 15.0f);

            if (!String.IsNullOrEmpty(selectedEvent.name) || !String.IsNullOrEmpty(selectedEvent.description))
            {
                Infobox.DataContext = selectedEvent;

                Infobox.Visibility = Visibility.Visible;

                MapLayer.SetPosition(Infobox, selectedEvent.Location);
            }
            else
            {
                Infobox.Visibility = Visibility.Collapsed;
            }

        }



        async private void Pushpin_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Pushpin selectedPushpin = (Pushpin)sender;
            Event selectedEvent = (Event)selectedPushpin.DataContext;
            MainMap.SetView(selectedEvent.Location, 15.0f);
            

            // to be handeled later.
            RootObject d = await fController.GetRSVPStatusForUser(selectedEvent.eid);
            //Ensure there is content to be displayed before modifying the infobox control
            if (!String.IsNullOrEmpty(selectedEvent.name) || !String.IsNullOrEmpty(selectedEvent.description))
            {
                Infobox.DataContext = selectedEvent;

                Infobox.Visibility = Visibility.Visible;

                MapLayer.SetPosition(Infobox, MapLayer.GetPosition(selectedPushpin));
            }
            else
            {
                Infobox.Visibility = Visibility.Collapsed;
            }
        
        }



        private void CloseInfobox_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Infobox.Visibility = Visibility.Collapsed;
        }

        async private void AttendButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;

            bool attending = await fController.attendEvent(selectedEvent.eid);
        }

        async private void MaybeButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;

            bool maybe = await fController.maybeEvent(selectedEvent.eid);

        }

        async private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;

            bool decline = await fController.declineEvent(selectedEvent.eid);

        }

        private void MainMap_Holding(object sender, HoldingRoutedEventArgs e)
        {

        }

        private void MainMap_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            
            textBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;

            this.MainMap.TryPixelToLocation(e.GetPosition(this.MainMap), out myLocation);
            MapLayer.SetPosition(textBlock, myLocation);

            textBlock.PointerPressed += btn_Click;
        }

        async void btn_Click(object sender, RoutedEventArgs e)
        {
            // clear events on the map. 
            PushpinCollection.Clear();

            textBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            
            // position me there 
            _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MapLayer.SetPosition(_locationIcon100m, myLocation);

            try
            {
                cityName = await lController.ReverseGeocodePoint(
                    new Location(myLocation.Latitude, myLocation.Longitude));
            }
            catch (System.ArgumentOutOfRangeException ArgumentOutOfRangeException) 
            {
                _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
         
                String message = "Could not get city name!";
                MessageDialog dialog = new MessageDialog(message);
                dialog.ShowAsync();
            }
            prog.IsActive = true;

            List<Data> results = await fController.GetAllEvents(cityName, offset,
                myLocation.Latitude,
                myLocation.Longitude,
                DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date)
                );

            PositionEventsInTheMap(results);
            
        }

        private TextBlock CreateTextBlock() 
        {
            
            return new TextBlock
            {
                Width = 60,
                Height = 40,
                Visibility = Windows.UI.Xaml.Visibility.Visible,
                Text = "Set Position to this location",
            };
        
        }

       
        /*
        async private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            
            if (e.Key == Windows.System.VirtualKey.Enter) 
            {
                List<Data> results = await fController.SearchEventsFromFacebook(searchBox.QueryText, 3, myPosition.Coordinate.Point.Position.Latitude, myPosition.Coordinate.Point.Position.Longitude, DateTimeConverter.DateTimeToUnixTimestamp(dateTimePicker.Date.Date));
                PositionEventsInTheMap(results);
            }
        }
        */
      

        
    }
}
