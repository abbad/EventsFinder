﻿using Bing.Maps;
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
        
        // 

        // text block for repositioning.
        private Button textBlock;

        // icons for the locaition
        LocationIcon10m _locationIcon10m;
        LocationIcon100m _locationIcon100m;

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
       
        }

        

        private void PositionUserOnMap() 
        {
            
            try
            {
                // Default to IP level accuracy. We only show the region at this level - No icon is displayed.
                double zoomLevel = 13.0f;

                // if we have GPS level accuracy
                if (App.myPosition.Coordinate.Accuracy <= 10)
                {
                    // Add the 10m icon and zoom closer.
                   _locationIcon10m.Visibility = Windows.UI.Xaml.Visibility.Visible;
                   MapLayer.SetPosition(_locationIcon10m, App.myLocation);
                    
                }
                // Else if we have Wi-Fi level accuracy.
                if (App.myPosition.Coordinate.Accuracy <= 100)
                {
                    // Add the 100m icon and zoom a little closer.
                    _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    MapLayer.SetPosition(_locationIcon100m, App.myLocation);
                    
                }

                // Set the map to the given location and zoom level.
                MainMap.SetView(App.myLocation, zoomLevel);


            }
            catch (System.UnauthorizedAccessException )
            {
                dialog.Content = "Could not find location";
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            prog.IsActive = true;
            PositionUserOnMap();
            MainMap.DataContext = this;
            pushpinsItemsControl.ItemsSource = App.PushpinCollection;
            prog.IsActive = false;
            /*
          

            if (App.errorOccured)
            {
                dialog.Content = App.errorMessage;
                _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                try
                {
                    await dialog.ShowAsync();
                }
                catch (Exception) { }
            }
            else 
            {
               
            }
         

            //context data
          
           */
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

            App.startRange = startRangeDateTimePicker.Date.Date;
            App.endRange = endRangeDateTimePicker.Date.Date;
            prog.IsActive = true;
            // get list of atteneded events by user.
            App.commonApiHandler.FillAttendedEventsByUserInCollection(await App.commonApiHandler.facebookApi.getListOfEventsAttendedByUser(DateTimeConverter.DateTimeToUnixTimestamp(App.startRange),
                DateTimeConverter.DateTimeToUnixTimestamp(App.endRange)));
            // QueryForEventsWithinAnArea
            String error = await App.commonApiHandler.QueryForEventsWithinAnArea(App.offset, DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
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

        private void MainMap_Holding(object sender, HoldingRoutedEventArgs e)
        {

        }

        // show reposition option
        private void MainMap_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            
            textBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Location loc = new Location();
            this.MainMap.TryPixelToLocation(e.GetPosition(this.MainMap), out loc);
            App.myLocation = loc;
            MapLayer.SetPosition(textBlock, App.myLocation);

            textBlock.Click += btn_Click;
        }

        async private void btn_Click(object sender, RoutedEventArgs e)
        {
            App.PushpinCollection.Clear();
            pushpinsItemsControl.ItemsSource = App.PushpinCollection;
            prog.IsActive = true;
            textBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            
            // position me there 
            _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MapLayer.SetPosition(_locationIcon100m, App.myLocation);

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
