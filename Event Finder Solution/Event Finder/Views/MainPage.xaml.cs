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
using System.Threading.Tasks;
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
        private Button textBlock;

        private double offset = 0.5;

        MessageDialog dialog = new MessageDialog("Could not get city name!");

        private String cityName = "";
        
        // List of events gathered from an area.
        private ObservableCollection<Event> PushpinCollection { get; set; }

        public ObservableCollection<Event> pushpinCollection
        {
            get
            {
                return PushpinCollection;
            }
        }

        // List of events attended by user.
        private ObservableCollection<Event> AttendingPushpinCollection { get; set; }

        public ObservableCollection<Event> attendingPushpinCollection
        {
            get
            {
                return AttendingPushpinCollection;
            }
        }

        List<Data> attendedEvents;


        public MainPage()
        {
            this.InitializeComponent();
            dialog.Options = MessageDialogOptions.AcceptUserInputAfterDelay;
            endRangeDateTimePicker.Date = DateTime.Today.AddDays(5);
            lController = new LocationController();
            fController = new FacebookViewModel();
            _locationIcon10m = new LocationIcon10m();
            _locationIcon100m = new LocationIcon100m();
            textBlock = CreateTextBlock();
            MainMap.Children.Add(textBlock);
            textBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            PushpinCollection = new ObservableCollection<Event>();
            AttendingPushpinCollection = new ObservableCollection<Event>();
            MainMap.Children.Add(_locationIcon100m);
            MainMap.Children.Add(_locationIcon10m);
            _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _locationIcon10m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            DataContext = this;
        }

        private void PositionUserOnMap(Geoposition myPosition) 
        {
            myLocation = new Location(myPosition.Coordinate.Point.Position.Latitude, myPosition.Coordinate.Point.Position.Longitude);
            
            try
            {
                // Default to IP level accuracy. We only show the region at this level - No icon is displayed.
                double zoomLevel = 13.0f;

                // if we have GPS level accuracy
                if (myPosition.Coordinate.Accuracy <= 10)
                {
                    // Add the 10m icon and zoom closer.
                   _locationIcon10m.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    MapLayer.SetPosition(_locationIcon10m, myLocation);
                    
                }
                // Else if we have Wi-Fi level accuracy.
                if (myPosition.Coordinate.Accuracy <= 100)
                {
                    // Add the 100m icon and zoom a little closer.
                    _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    MapLayer.SetPosition(_locationIcon100m, myLocation);
                    
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

            // initial user position
            Geoposition myPosition = await lController.GetCurrentLocation();
            PositionUserOnMap(myPosition);

            prog.IsActive = true;
            // get list of atteneded events by user.
            attendedEvents = await fController.getListOfEventsAttendedByUser(
                DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date));
            FillAttendedEventsByUserInCollection(attendedEvents);
            // Fill everythin needed for the app to function.
            ReloadALL();

            //context data
            MainMap.DataContext = this;

        }


        private void FillAttendedEventsByUserInCollection(List<Data> attendedEvents) 
        {

            // add attended events to collection.
            // loop through the list of results.
            string venueName1 = "";
            prog.IsActive = true;
            foreach (var result in attendedEvents)
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
                        Event ee = new Event
                            {
                                eid = itemEvent.eid,
                                name = itemEvent.name,
                                Location = new Location(Convert.ToDouble(itemEvent.venue["latitude"]), Convert.ToDouble(itemEvent.venue["longitude"])),
                                venueName = venueName1,
                                pic_square = itemEvent.pic_square,
                                pic_big = itemEvent.pic_big,
                                description = itemEvent.description,
                                start_time = itemEvent.start_time,
                                end_time = itemEvent.end_time,
                            };

                        // fill it in the item list of users event.
                        AttendingPushpinCollection.Add(ee);
                      
                    }
                    catch (Exception asda)
                    {

                    }

                }
            }
            prog.IsActive = false;

        
        }

        private void FillEventsInPushPinCollection(List<Data> results)
        {
            // a workaround for my events.
            foreach (Event itemEvent in AttendingPushpinCollection)
            {
                PushpinCollection.Add(itemEvent);
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

        async private void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            prog.IsActive = true;
            // get list of atteneded events by user.
            attendedEvents = await fController.getListOfEventsAttendedByUser(DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date));
            FillAttendedEventsByUserInCollection(attendedEvents);
            ReloadALL();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Event selectedEvent = (Event)e.ClickedItem;
            LoadInfoBox(selectedEvent);
        }

        private void SetButtonToStatus(RSVP rsvp)
        {

            if (rsvp != null)
            {
                if (rsvp.rsvp_status == "attending")
                {
                    AttendButton.IsEnabled = false;
                    DeclineButton.IsEnabled = true;
                    MaybeButton.IsEnabled = true;
                }
                else if (rsvp.rsvp_status == "unsure")
                {
                    DeclineButton.IsEnabled = true;
                    AttendButton.IsEnabled = true;
                    MaybeButton.IsEnabled = false;
                }
                else if (rsvp.rsvp_status == "declined")
                {
                    DeclineButton.IsEnabled = false;
                    AttendButton.IsEnabled = true;
                    MaybeButton.IsEnabled = true;
                }

            }
            else
            {
                DeclineButton.IsEnabled = true;
                AttendButton.IsEnabled = true;
                MaybeButton.IsEnabled = true;

            }
        }

        private void Pushpin_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Pushpin selectedPushpin = (Pushpin)sender;
            Event selectedEvent = (Event)selectedPushpin.DataContext;
            LoadInfoBox(selectedEvent);
        
        }


        async private void LoadInfoBox(Event selectedEvent) 
        {
            MainMap.SetView(selectedEvent.Location, 15.0f);

            // see RSVP status of event.
            RootObject rsvp = await fController.GetRSVPStatusForUser(selectedEvent.eid);
            if (rsvp.data.Count != 0)
            {
                SetButtonToStatus(rsvp.data[0]);
            }
            else 
            {
                // enable all buttons.
                SetButtonToStatus(null);
            }
            //Ensure there is content to be displayed before modifying the infobox control
            if (!String.IsNullOrEmpty(selectedEvent.name) || !String.IsNullOrEmpty(selectedEvent.description))
            {
                Infobox.DataContext = selectedEvent;

                Infobox.Visibility = Visibility.Visible;

                MapLayer.SetPosition(Infobox, MapLayer.GetPosition(selectedEvent.Location));
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

            bool attending = await fController.RSVPEvent(selectedEvent.eid, "attending");
            if (attending == true)
            {
                SetButtonToStatus(new RSVP { rsvp_status = "attending" });
                // check if the event is in the list of the attended events.
                if (!AttendingPushpinCollection.Contains(selectedEvent))
                {
                    AttendingPushpinCollection.Add(selectedEvent);
                }
            }else
            {
                dialog.Content = "Could not RSVP for Event";
                await dialog.ShowAsync();
            }
        }

        async private void MaybeButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;
            bool maybe = false;

            maybe = await fController.RSVPEvent(selectedEvent.eid, "maybe");

            if (maybe == true)
            {

                SetButtonToStatus(new RSVP { rsvp_status = "unsure" });
                // check if the event is in the list of the attended events.
                if (!AttendingPushpinCollection.Contains(selectedEvent)) { 
                    AttendingPushpinCollection.Add(selectedEvent);
                }
            }
            else 
            {
                dialog.Content = "Could not RSVP for Event";
                await dialog.ShowAsync();
            }

        }

        async private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;

            bool decline = await fController.RSVPEvent(selectedEvent.eid, "declined");

            if (decline == true) 
            {
                SetButtonToStatus(new RSVP { rsvp_status = "declined" });
                // check if the event is in the list of the attended events.
                if (!AttendingPushpinCollection.Contains(selectedEvent))
                {
                    AttendingPushpinCollection.Add(selectedEvent);
                }
            }
            else
            {
                dialog.Content= "Could not RSVP for Event";
                await dialog.ShowAsync();
            }

        }

        private void MainMap_Holding(object sender, HoldingRoutedEventArgs e)
        {

        }

        // show reposition option
        private void MainMap_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            
            textBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;

            this.MainMap.TryPixelToLocation(e.GetPosition(this.MainMap), out myLocation);
            MapLayer.SetPosition(textBlock, myLocation);

            textBlock.Click += btn_Click;
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            AttendingListView.IsEnabled = false;
            textBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            
            // position me there 
            _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MapLayer.SetPosition(_locationIcon100m, myLocation);

            ReloadALL();
            AttendingListView.IsEnabled = true;
            prog.IsActive = true;
            
        }

        /// <summary>
        ///  a function that will reload all items displayed on screen.
        /// </summary>
        /// <param name="myPosition"></param>
        async private void ReloadALL()
        {
            prog.IsActive = true;
            System.ArgumentOutOfRangeException ex = null;
            bool open = false;   
            pushpinCollection.Clear();
           
            List<Data> results;
            // get the city name from reverse geocodeing
            try
            {
                cityName = await lController.ReverseGeocodePoint(
                    new Location(myLocation.Latitude, myLocation.Longitude));
            }
            catch (System.ArgumentOutOfRangeException argumentOutOfRangeException)
            {
                _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                dialog.Content = "Could not find city:" + argumentOutOfRangeException.Data;
                dialog.ShowAsync();

            }
            catch (System.TimeoutException timeoutException) 
            {
                dialog.Content = "Could not connect to the internet:" + timeoutException.Data;
                dialog.ShowAsync();
            } 
            
            prog.IsActive = true;
            // get list of events. 
            results = await fController.GetAllEvents(cityName, offset, myLocation.Latitude,
               myLocation.Longitude,
                DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date));
            prog.IsActive = true;
            // position events on map.
            FillEventsInPushPinCollection(results);
        
        }

        private Button CreateTextBlock() 
        {
            
            Button x = new Button();
            x.Height = 80;
            x.Width = 150;
            x.Margin = new Thickness(-90, -110, 0, 0);//left top right bottom
            x.Style = (Style)Application.Current.Resources["setPos"];

            return x;
        
        }
       
        
  
    

        
    }
}
