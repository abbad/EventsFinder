using Bing.Maps;
using Event_Finder.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event_Finder.Common
{
    class CommonApiHandler
    {
       
        
        // controller for facebook functions
        public FacebookViewModel facebookApi;
        // controller for geolocation. 
        LocationController lController;
        // application location
        public Location myLocation;

        private String cityName = "";
        public CommonApiHandler()
        {
            initializeObjects();
            initializeCollections();
        }

        private void initializeObjects()
        {
            lController = new LocationController();
            facebookApi = new FacebookViewModel();
        }

        private void initializeCollections()
        {
            App.PushpinCollection = new ObservableCollection<Event>();
            App.AttendingCollection = new ObservableCollection<Event>();
            App.ItemEventsList = new ObservableCollection<Event>();
        }

        

          /// <summary>
        ///  a function that will reload all items displayed on screen.
        /// </summary>
        /// <param name="myPosition"></param>
        async private void QueryForEventsWithinAnArea()
        {
          
            bool handeled = false;
            bool exceptionOccured = false;
           
            List<Data> results;
            // get the city name from reverse geocodeing
            try
            {
                cityName = await lController.ReverseGeocodePoint(
                    new Location(myLocation.Latitude, myLocation.Longitude));
            }
            catch (System.ArgumentOutOfRangeException)
            {
                _locationIcon100m.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                dialog.Content = "Could not find city name"; 
                exceptionOccured = true;
                

            }
            catch (System.TimeoutException) 
            {
                dialog.Content = "Could not connect to the internet";
                exceptionOccured = true;
            }if (exceptionOccured)
            { 
               
                try
                {
                    if (!handeled) {
                        await dialog.ShowAsync();
                        handeled = true;
                    }
                   
                }
                catch (Exception){}
                
            }
            
            // get list of events. 
            results = await facebookApi.GetAllEvents(SafeDBString(cityName), offset, myLocation.Latitude,
               myLocation.Longitude,
                DateTimeConverter.DateTimeToUnixTimestamp(startRangeDateTimePicker.Date.Date),
                DateTimeConverter.DateTimeToUnixTimestamp(endRangeDateTimePicker.Date.Date));
            
            // position events on map.
            FillEventsCollection(results);
        
        }

        string SafeDBString(string inputValue)
        {
            return inputValue.Replace("'", " ");
        }


        private void FillEventsCollection(List<Data> results)
        {
            prog.IsActive = true;
            foreach (var result in results)
            {
                foreach (var itemEvent in result.data)
                {

                    if (CheckEventForLatitudeAndLongitude(itemEvent))
                    {
                        itemEvent.Location = new Location(Convert.ToDouble(itemEvent.venue["latitude"]), Convert.ToDouble(itemEvent.venue["longitude"]));
                        // fill it in item lsit of events
                        App.ItemEventsList.Add(itemEvent);

                        // add them to pushpin collection
                        App.PushpinCollection.Add(itemEvent);
                    }
                }

            }
            prog.IsActive = false;

        }

        private void FillAttendedEventsByUserInCollection(List<Data> attendedEvents)
        {

            // add attended events to collection.
            prog.IsActive = true;
            foreach (var result in attendedEvents)
            {
                foreach (var itemEvent in result.data)
                {
                    if (CheckEventForLatitudeAndLongitude(itemEvent))
                    {
                        // create the instance of location 
                        itemEvent.Location = new Location(Convert.ToDouble(itemEvent.venue["latitude"]), Convert.ToDouble(itemEvent.venue["longitude"]));

                        // fill it in the item list of users event.
                        App.AttendingCollection.Add(itemEvent);

                        // and add them to pushpin collection
                        App.PushpinCollection.Add(itemEvent);
                    }
                }
            }
            prog.IsActive = false;


        }
    }
}
