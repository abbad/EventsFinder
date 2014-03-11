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
using Windows.UI.ApplicationSettings;
using Windows.System;
using Windows.UI.Xaml.Documents;
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
        
        MessageDialog dialog = new MessageDialog("");
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
            endRangeDateTimePicker.Date = App.endRange;
            startRangeDateTimePicker.Date = App.startRange;
            DataContext = this;
            
            dialog.Commands.Add(new UICommand("Cancel", (uiCommand) => { }));
            dialog.CancelCommandIndex = 1;

            SettingsPane.GetForCurrentView().CommandsRequested += SettingsCommandsRequested;    
        }

        
        private void SettingsCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var privacyStatement = new SettingsCommand("privacy", "Privacy Statement", async x => await Launcher.LaunchUriAsync(new Uri("http://eventsfinderprivacypolicy.wordpress.com/2014/03/01/events-finder-privacy-policy-2/")));

            args.Request.ApplicationCommands.Clear();
            args.Request.ApplicationCommands.Add(privacyStatement);
        }
        
        
        async private void PositionUserOnMap() 
        {   
            // Default to IP level accuracy. We only show the region at this level - No icon is displayed.
           
            await App.GettingPositionFinished.Task;
            MapLayer.SetPosition(_locationIcon100m, App.myLocation);
            _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Visible;
                
            MainMap.SetView(App.myLocation, App.zoomLevel);
        }

        async private void map_loaded(object sender, RoutedEventArgs e)
        {
            prog.IsIndeterminate = true;
            await App.ErrorOccuredFinished.Task;
            // check if error happend before.
            if (App.errorOccured) 
            {
                try
                {
                    dialog.Content = App.errorMessage;
                    await dialog.ShowAsync();
                    App.errorOccured = false;
                    App.GettingPositionFinished.TrySetResult(true);
                    prog.IsIndeterminate = false;
                    return;
                }
                catch (Exception){ }
               
            }

            PositionUserOnMap();
            
            

            await App.commonApiHandler.GettingEventsFinished.Task;
            
            if (App.myEventsSelected)
            {
                pushpinsItemsControl.ItemsSource = App.commonApiHandler.UserEvents;
                myEventsButton.Label = "View All Events";
                setMapZoomForUserEvents();
            }
            else
            {
                pushpinsItemsControl.ItemsSource = App.commonApiHandler.QueriedEvents;
                myEventsButton.Label = "My Events";
                setMapZoomForQueriedEvents();
            }

            // set zoom level while considering nearest 5 places. 
            
            prog.IsIndeterminate = false;
            
        }

        private void setMapZoomForUserEvents() 
        {
            Bing.Maps.LocationCollection locationCollection = new Bing.Maps.LocationCollection();
            locationCollection.Add(App.myLocation);

            foreach (Event selectedEvent in App.commonApiHandler.UserEvents) {
                locationCollection.Add(selectedEvent.Location);
            }

            Bing.Maps.LocationRect locationRect = new Bing.Maps.LocationRect(locationCollection);
            MainMap.SetView(locationRect);
        }


        private void setMapZoomForQueriedEvents() {
            Bing.Maps.LocationCollection locationCollection = new Bing.Maps.LocationCollection();
            locationCollection.Add(App.myLocation);

            foreach (Event selectedEvent in App.commonApiHandler.QueriedEvents)
            {
                locationCollection.Add(selectedEvent.Location);
            }
            Bing.Maps.LocationRect locationRect = new Bing.Maps.LocationRect(locationCollection);
          
            MainMap.SetView(locationRect);
        
        }

        private void clearAllCollections() 
        {
            App.commonApiHandler.UserEvents.Clear();
            App.commonApiHandler.QueriedEvents.Clear();
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
                
            if (Infobox.Visibility != Windows.UI.Xaml.Visibility.Visible)
            {
                MainMap.Width = MainMap.ActualWidth / 2;
                MainMap.HorizontalAlignment = HorizontalAlignment.Left;
                Infobox.Width = MainMap.Width;
            }

            LoadInfoBox(selectedEvent);
        }

        async private void LoadInfoBox(Event selectedEvent) 
        {
            if (!App.IsInternet()) 
            {
                try
                {
                    dialog.Content = "Internet connection lost";
                    await dialog.ShowAsync();
                    return; 
                }
                catch (Exception) { }
                App.errorOccured = false;
            }
            //Facebook.WebExceptionWrapper
            InfoBoxProgressBar.IsEnabled = true;
            MainMap.SetView(selectedEvent.Location, MainMap.ZoomLevel);
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
            InfoBoxProgressBar.IsEnabled = false;
            MainMap.IsEnabled = true;
        }

        private void CloseInfo(object sender, RoutedEventArgs e)
        {
            MainMap.Width = MainMap.Width * 2;
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
            InfoBoxProgressBar.IsIndeterminate = true;
            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;

            bool attending = await App.commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "attending");
            if (attending == true)
            {
                SetButtonToStatus(new RSVP { rsvp_status = "attending" });
                // check if the event is in the list of the attended events.
                if (!App.commonApiHandler.UserEvents.Contains(selectedEvent))
                {
                    App.commonApiHandler.UserEvents.Add(selectedEvent);
                }
            }else
            {
                dialog.Content = "Could not RSVP for Event";
                await dialog.ShowAsync();
            }
            InfoBoxProgressBar.IsIndeterminate = false;
        }

        async private void MaybeButton_Click(object sender, RoutedEventArgs e)
        {
            InfoBoxProgressBar.IsIndeterminate = true;
            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;
            bool maybe = false;

            maybe = await App.commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "maybe");

            if (maybe == true)
            {

                SetButtonToStatus(new RSVP { rsvp_status = "unsure" });
                // check if the event is in the list of the attended events.
                if (!App.commonApiHandler.UserEvents.Contains(selectedEvent))
                {
                    App.commonApiHandler.UserEvents.Add(selectedEvent);
                }
            }
            else 
            {
                dialog.Content = "Could not RSVP for Event";
                await dialog.ShowAsync();
            }
            InfoBoxProgressBar.IsIndeterminate = false;
        }

        async private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            InfoBoxProgressBar.IsIndeterminate = true;
            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;

            bool decline = await App.commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "declined");

            if (decline == true) 
            {
                SetButtonToStatus(new RSVP { rsvp_status = "declined" });
                // check if the event is in the list of the attended events.
                if (!App.commonApiHandler.UserEvents.Contains(selectedEvent))
                {
                    App.commonApiHandler.UserEvents.Add(selectedEvent);
                }
            }
            else
            {
                dialog.Content= "Could not RSVP for Event";
                await dialog.ShowAsync();
            }
            InfoBoxProgressBar.IsIndeterminate = false;
        }

        private void appBarNavigateButton_Click(object sender, RoutedEventArgs e)
        {
            App.zoomLevel = MainMap.ZoomLevel;
            Frame.Navigate(typeof(GridViewPage));
        }

        private void myEventsButton_Click(object sender, RoutedEventArgs e)
        {
          
            string toggle = myEventsButton.Label;
            if (toggle =="My Events")
            {
                myEventsButton.Label = "View All Events";

                pushpinsItemsControl.ItemsSource = App.commonApiHandler.UserEvents;
                App.myEventsSelected = true;
                if (App.commonApiHandler.UserEvents.Count != 0) {
                    setMapZoomForUserEvents();
                }
            }
            else 
            {
                pushpinsItemsControl.ItemsSource = App.commonApiHandler.QueriedEvents;
                myEventsButton.Label = "My Events";
                App.myEventsSelected = false;
                if (App.commonApiHandler.QueriedEvents.Count != 0) {
                    setMapZoomForQueriedEvents();
                }
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
                App.commonApiHandler.QueriedEvents.Clear();
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            App.zoomLevel = MainMap.ZoomLevel; 
            Frame.Navigate(typeof(Settings));
        }

        async private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var richTB = sender as RichTextBlock;
            var textPointer = richTB.GetPositionFromPoint(e.GetPosition(richTB));
            Event selectEvent = (Event)richTB.DataContext;
            var element = textPointer.Parent as TextElement;
            while (element != null && !(element is Underline))
            {
                if (element.ContentStart != null
                    && element != element.ElementStart.Parent)
                {
                    element = element.ElementStart.Parent as TextElement;
                    
                }
                else
                {
                    element = null;
                }
            }

            if (element == null) return;

            var underline = element as Underline;
            if (underline.Name == "LinkToEventsPage")
            {
                
                await Launcher.LaunchUriAsync(new Uri(String.Format("https://www.facebook.com/{0}", selectEvent.eid)));
            }

        }

    }
}
