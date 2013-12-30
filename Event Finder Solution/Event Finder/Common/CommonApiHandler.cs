    using Bing.Maps;
    using Event_Finder.Models;
    using Event_Finder.ViewModel;
    using Event_Finder.Views;
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

        public ObservableCollection<Friend> friendList = new ObservableCollection<Friend>();
        
        // controller for facebook functions
        public FacebookViewModel facebookApi;
        // controller for geolocation. 
        public LocationController lController;
        // application location
        public TaskCompletionSource<bool> GettingEventsFinished = new TaskCompletionSource<bool>();
       

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
            App.AttendingCollection = new ObservableCollection<Event>();
            App.ItemEventsList = new ObservableCollection<Event>();
        }

        async public Task<String> QueryForUserEvents()
        {
            List<Data> results;
            try { 
                results = await App.commonApiHandler.facebookApi.getListOfEventsAttendedByUser(DateTimeConverter.DateTimeToUnixTimestamp(App.startRange),
                    DateTimeConverter.DateTimeToUnixTimestamp(App.endRange));
            }
            catch (System.Threading.Tasks.TaskCanceledException) 
            {
                return "Internet connection lost";
            }
               // get list of atteneded events by user.
            App.commonApiHandler.FillAttendedEventsByUserInCollection(results);
            
            return null;
        }
     
            /// <summary>
        ///  a function that will reload all items displayed on screen.
        /// </summary>
        /// <param name="myPosition"></param>
        async public Task<String> QueryForEventsWithinAnArea(double offset, double startRange, double endRange)
        {
         
            List<Data> results;
            // get the city name from reverse geocodeing
            try
            {
                cityName = await lController.ReverseGeocodePoint(
                    new Location(App.myLocation.Latitude, App.myLocation.Longitude));
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return "Could not find city Name";
            }
            catch (System.TimeoutException)
            {
                return "Could not connect to the internet";
            }
            catch (System.ServiceModel.EndpointNotFoundException) 
            {
                return "Could not connect to endpoint";
            }

            try
            {
                // get list of events. 
                results = await facebookApi.GetAllEvents(SafeDBString(cityName), App.offset, App.myLocation.Latitude,
                    App.myLocation.Longitude,
                    startRange,
                    endRange);

                // fill the collections 
                FillEventsCollection(results);
            }
            catch (Facebook.WebExceptionWrapper) 
            {
                return "Could not connect to internet"; 
            }

            GettingEventsFinished.TrySetResult(true);
            return null;
            
        }

        string SafeDBString(string inputValue)
        {
            return inputValue.Replace("'", " ");
        }

        public void FillFriendsAttendingCollection(FriendRoot fr)
        {   
            int i;
            
            for (i = 0; i < fr.data.Count; i++)
            {
                friendList.Add(fr.data[i]);
            }
            
        }

        private void FillEventsCollection(List<Data> results)
        {
        
            foreach (var result in results)
            {
                foreach (var itemEvent in result.data)
                {
                    if (itemEvent.venue != null) { 

                    if (CheckEventForLatitudeAndLongitude(itemEvent))
                    {
                        if (CheckEventForVenueName(itemEvent))
                        {
                            itemEvent.venueName = itemEvent.venue["name"];
                        }
                        else 
                        {
                            itemEvent.venueName = "Unknown";
                        }
                        if (itemEvent.start_time != null){itemEvent.startTimeObject = DateTime.Parse(itemEvent.start_time);}
                        if (itemEvent.end_time != null) { itemEvent.endTimeObject = DateTime.Parse(itemEvent.end_time); }
                        itemEvent.Location = new Location(Convert.ToDouble(itemEvent.venue["latitude"]), Convert.ToDouble(itemEvent.venue["longitude"]));
                        // fill it in item lsit of events
                        App.ItemEventsList.Add(itemEvent);
                    }

                    }
                }

            }
          
        }

        private bool CheckEventForLatitudeAndLongitude(Event Item) 
        {
            return (Item.venue.ContainsKey("latitude") && Item.venue.ContainsKey("longitude"));
        }

        private bool CheckEventForVenueName(Event Item) 
        {
            return Item.venue.ContainsKey("name");
        }


        private void FillAttendedEventsByUserInCollection(List<Data> attendedEvents)
        {

            // add attended events to collection.
            
            foreach (var result in attendedEvents)
            {
                foreach (var itemEvent in result.data)
                {
                    if (itemEvent.venue != null) { 
                    if (CheckEventForLatitudeAndLongitude(itemEvent))
                    {
                        // check for venue name
                        if (CheckEventForVenueName(itemEvent)) 
                        {
                            itemEvent.venueName = itemEvent.venue["name"];
                        }
                        else
                        {
                            itemEvent.venueName = "Unknown";
                        }

                        if (itemEvent.start_time != null) { itemEvent.startTimeObject = DateTime.Parse(itemEvent.start_time); }
                        if (itemEvent.end_time != null) { itemEvent.endTimeObject = DateTime.Parse(itemEvent.end_time); }

                        // create the instance of location 
                        itemEvent.Location = new Location(Convert.ToDouble(itemEvent.venue["latitude"]), Convert.ToDouble(itemEvent.venue["longitude"]));

                        // fill it in the item list of users event.
                        App.AttendingCollection.Add(itemEvent);

                    }
                    }
                }
            }
            


        }
    }
    }
