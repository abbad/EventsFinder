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
using Event_Finder.Common;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Event_Finder.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        

        // text block for repositioning.
        private Button textBlock;

        private double offset = 0.5;

        // icons for the locaition
        LocationIcon10m _locationIcon10m;
        LocationIcon100m _locationIcon100m;

        private CommonApiHandler commonApiHandler = new CommonApiHandler();

        MessageDialog dialog = new MessageDialog("Could not get city name!");
        private void addInitialChildrenToMap() 
        {
            MainMap.Children.Add(_locationIcon100m);
            MainMap.Children.Add(_locationIcon10m);
            MainMap.Children.Add(textBlock);
        }

        private void setInitialItemsToCollapsed() 
        {
            _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _locationIcon10m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            textBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
        public MainPage()
        { 
            this.InitializeComponent();
            textBlock = CreateButton();
            _locationIcon10m = new LocationIcon10m();
            _locationIcon100m = new LocationIcon100m();
            setInitialItemsToCollapsed();
            addInitialChildrenToMap();
            endRangeDateTimePicker.Date = DateTime.Today.AddDays(5);
            DataContext = this;
          
           
            dialog.Commands.Add(new UICommand("Cancel", (uiCommand) => { }));
            dialog.CancelCommandIndex = 1;
          
            

            // create objec of common 
            CommonApiHandler commonApiHandler = new CommonApiHandler();
        }

        private void PositionUserOnMap(Geoposition myPosition) 
        {
            commonApiHandler.myLocation = new Location(myPosition.Coordinate.Point.Position.Latitude, myPosition.Coordinate.Point.Position.Longitude);
            
            try
            {
                // Default to IP level accuracy. We only show the region at this level - No icon is displayed.
                double zoomLevel = 13.0f;

                // if we have GPS level accuracy
                if (myPosition.Coordinate.Accuracy <= 10)
                {
                    // Add the 10m icon and zoom closer.
                   _locationIcon10m.Visibility = Windows.UI.Xaml.Visibility.Visible;
                   MapLayer.SetPosition(_locationIcon10m, commonApiHandler.myLocation);
                    
                }
                // Else if we have Wi-Fi level accuracy.
                if (myPosition.Coordinate.Accuracy <= 100)
                {
                    // Add the 100m icon and zoom a little closer.
                    _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    MapLayer.SetPosition(_locationIcon100m, commonApiHandler.myLocation);
                    
                }

                // Set the map to the given location and zoom level.
                MainMap.SetView(commonApiHandler.myLocation, zoomLevel);


            }
            catch (System.UnauthorizedAccessException )
            {
                dialog.Content = "Could not find location";
            }
        }

        async private void OnLoad(object sender, RoutedEventArgs e)
        {
            


            prog.IsActive = true;

            // initial user position
            Geoposition myPosition = await commonApiHandler.lController.GetCurrentLocation();
            PositionUserOnMap(myPosition);

            prog.IsActive = true;

            // get list of atteneded events by user.
            commonApiHandler.FillAttendedEventsByUserInCollection(await commonApiHandler.facebookApi.getListOfEventsAttendedByUser(
                DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date)));
            
            // QueryForEventsWithinAnArea
            String error = await commonApiHandler.QueryForEventsWithinAnArea(offset, DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date));

            if (error != null) 
            {
                dialog.Content = error; 
                _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                try
                {
                    await dialog.ShowAsync();
                }
                catch (Exception) { }
             
            }
         

            //context data
            MainMap.DataContext = this;
            pushpinsItemsControl.ItemsSource = App.PushpinCollection;
            prog.IsActive = false;
        }

        

        private void clearAllCollections() 
        {
            App.PushpinCollection.Clear();
            App.AttendingCollection.Clear();
            App.ItemEventsList.Clear();
        }
        
        async private void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
             clearAllCollections();
            prog.IsActive = true;
            // get list of atteneded events by user.
            commonApiHandler.FillAttendedEventsByUserInCollection(await commonApiHandler.facebookApi.getListOfEventsAttendedByUser(DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date)));
            // QueryForEventsWithinAnArea
            String error = await commonApiHandler.QueryForEventsWithinAnArea(offset, DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date));

            if (error != null)
            {
                dialog.Content = error;
                _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                try
                {
                    await dialog.ShowAsync();
                }
                catch (Exception) { }
            }
            prog.IsActive = false;
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
            RootObject rsvp = await commonApiHandler.facebookApi.GetRSVPStatusForUser(selectedEvent.eid);
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

            bool attending = await commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "attending");
            if (attending == true)
            {
                SetButtonToStatus(new RSVP { rsvp_status = "attending" });
                // check if the event is in the list of the attended events.
                if (!App.AttendingCollection.Contains(selectedEvent))
                {
                    App.AttendingCollection.Add(selectedEvent);
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

            maybe = await commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "maybe");

            if (maybe == true)
            {

                SetButtonToStatus(new RSVP { rsvp_status = "unsure" });
                // check if the event is in the list of the attended events.
                if (!App.AttendingCollection.Contains(selectedEvent))
                {
                    App.AttendingCollection.Add(selectedEvent);
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

            bool decline = await commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "declined");

            if (decline == true) 
            {
                SetButtonToStatus(new RSVP { rsvp_status = "declined" });
                // check if the event is in the list of the attended events.
                if (!App.AttendingCollection.Contains(selectedEvent))
                {
                    App.AttendingCollection.Add(selectedEvent);
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
            Location loc = new Location();
            this.MainMap.TryPixelToLocation(e.GetPosition(this.MainMap), out loc);
             commonApiHandler.myLocation =  loc;
            MapLayer.SetPosition(textBlock, commonApiHandler.myLocation);

            textBlock.Click += btn_Click;
        }

        async private void btn_Click(object sender, RoutedEventArgs e)
        {
            prog.IsActive = true;
            textBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            
            // position me there 
            _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MapLayer.SetPosition(_locationIcon100m, commonApiHandler.myLocation);

            String error = await commonApiHandler.QueryForEventsWithinAnArea(offset, DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date));

            if (error != null)
            {
                dialog.Content = error;
                _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                try
                {
                    await dialog.ShowAsync();
                }
                catch (Exception) { }

            }
            prog.IsActive = false;
            
        }

      
        private Button CreateButton() 
        {
            Button x = new Button();
            x.Height = 80;
            x.Width = 150;
            x.Margin = new Thickness(-90, -110, 0, 0);//left top right bottom
            x.Style = (Style)Application.Current.Resources["setPos"];

            return x;
        
        }

        private void appBarNavigateButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GridViewPage));
        }
      
    

        
    }
}
