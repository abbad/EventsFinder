using Bing.Maps;
using Event_Finder.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
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
        
        // x bool for enabling positioning
        bool x = false;

        // icons for the locaition
        LocationIcon100m _locationIcon100m;

        MessageDialog dialog = new MessageDialog("Could not get city name!");
        private void addInitialChildrenToMap()
        {
            MainMap.Children.Add(_locationIcon100m);  
        }

        private void setInitialItemsToCollapsed() 
        {
            _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
        public MainPage()
        { 
            this.InitializeComponent();
       
            _locationIcon100m = new LocationIcon100m();
            setInitialItemsToCollapsed();
            addInitialChildrenToMap();
            endRangeDateTimePicker.Date = DateTime.Today.AddDays(5);
            DataContext = this;
            
            dialog.Commands.Add(new UICommand("Cancel", (uiCommand) => { }));
            dialog.CancelCommandIndex = 1;

        }

        
        async private void PositionUserOnMap() 
        {   
            // Default to IP level accuracy. We only show the region at this level - No icon is displayed.
            double zoomLevel = 13.0f;
            await App.GettingPositionFinished.Task; 
            MapLayer.SetPosition(_locationIcon100m, App.myLocation);
            _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Visible;
                
            MainMap.SetView(App.myLocation, zoomLevel);
        }

        async private void OnLoad(object sender, RoutedEventArgs e)
        {

            // check if error happend before.
            if (App.errorOccured) 
            {
                try
                {
                    dialog.Content = App.errorMessage;
                    await dialog.ShowAsync();
                }
                catch (Exception){ }
            }

            prog.IsIndeterminate = true;
            PositionUserOnMap();
            
            if (App.myEventsSelected)
            {
                pushpinsItemsControl.ItemsSource = App.AttendingCollection;
                myEventsButton.Label = "View All Events";
            }
            else 
            {
                pushpinsItemsControl.ItemsSource = App.ItemEventsList;
                myEventsButton.Label = "My Events";
            }
            await App.commonApiHandler.GettingEventsFinished.Task;
            prog.IsIndeterminate = false;
            
        }

        private void clearAllCollections() 
        {
            App.AttendingCollection.Clear();
            App.ItemEventsList.Clear();
        }
        
        async private void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            clearAllCollections();
            prog.IsIndeterminate = true;
            App.startRange = startRangeDateTimePicker.Date.Date;
            App.endRange = endRangeDateTimePicker.Date.Date;

            // get user events.
            String error = await App.commonApiHandler.QueryForUserEvents();
                
           
            // QueryForEventsWithinAnArea
            error = await App.commonApiHandler.QueryForEventsWithinAnArea(App.offset, DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
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
            prog.IsIndeterminate = false; 
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
            App.commonApiHandler.friendList.Clear();
            // see RSVP status of event.
            try
            {
                RootObject rsvp = await App.commonApiHandler.facebookApi.GetRSVPStatusForUser(selectedEvent.eid);
                if (rsvp.data.Count != 0)
                {
                    SetButtonToStatus(rsvp.data[0]);
                }
                else
                {
                    // enable all buttons.
                    SetButtonToStatus(null);
                }
                // git list of friends.
            }
            catch (System.Threading.Tasks.TaskCanceledException){ App.errorOccured = true;}
            catch (Facebook.WebExceptionWrapper) { App.errorOccured = true; }
            if (App.errorOccured) 
            {
                try
                {
                    dialog.Content = "Internet connection lost";
                    await dialog.ShowAsync();
                }
                catch (Exception){}
                App.errorOccured = false;
            }

            
            //Ensure there is content to be displayed before modifying the infobox control
            if (!String.IsNullOrEmpty(selectedEvent.name) || !String.IsNullOrEmpty(selectedEvent.description))
            {
                Infobox.DataContext = selectedEvent;

                Infobox.Visibility = Visibility.Visible;

                MapLayer.SetPosition(Infobox, MapLayer.GetPosition(selectedEvent.Location));
                FriendRoot vsx = await App.commonApiHandler.facebookApi.GetFriendsAttendingEvent(selectedEvent.eid);
                App.commonApiHandler.FillFriendsAttendingCollection(vsx);
                attendFr.ItemsSource = App.commonApiHandler.friendList;
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

        async private void AttendButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;

            bool attending = await App.commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "attending");
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

            maybe = await App.commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "maybe");

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

            bool decline = await App.commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "declined");

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

      
        private void appBarNavigateButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GridViewPage));
        }

        private void myEventsButton_Click(object sender, RoutedEventArgs e)
        {
          
            string toggle = myEventsButton.Label;
            if (toggle =="My Events")
            {
                myEventsButton.Label = "View All Events";

                pushpinsItemsControl.ItemsSource = App.AttendingCollection;
                App.myEventsSelected = true;
            }
            else 
            {
                pushpinsItemsControl.ItemsSource = App.ItemEventsList;
                myEventsButton.Label = "My Events";
                App.myEventsSelected = false;
            }
            
        }

        private void setPositionButton_Click(object sender, RoutedEventArgs e)
        {
            x = true;
            setPositionButton.IsEnabled = false;
        }

        async void MainMap_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
           
            if (x) {
                App.ItemEventsList.Clear();
                x = false;
                prog.IsIndeterminate = true;
                setPositionButton.IsEnabled = true;
                Location loc = new Location();
                this.MainMap.TryPixelToLocation(e.GetCurrentPoint(MainMap).Position, out loc);
                App.myLocation = loc;
              
                // position me there 
                _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Visible;
                MapLayer.SetPosition(_locationIcon100m, App.myLocation);

                // get all events withing an area. 
                String error = await App.commonApiHandler.QueryForEventsWithinAnArea(App.offset, DateTimeConverter.DateTimeToUnixTimestamp(App.startRange),
                    DateTimeConverter.DateTimeToUnixTimestamp(App.endRange));

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
                prog.IsIndeterminate = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Infobox.Visibility = Visibility.Collapsed;
        }

    }
}
