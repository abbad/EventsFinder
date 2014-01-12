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

        private static ObservableCollection<Event> _userEvents { get; set; }

        public ObservableCollection<Event> UserEvents
        {
            get
            {
                return _userEvents;
            }
        }

        // List of events queried for. 
        private ObservableCollection<Event> _queriedEvents { get; set; }

        public ObservableCollection<Event> QueriedEvents
        {
            get
            {
                return _queriedEvents;
            }
        }


        public ObservableCollection<Friend> friendList = new ObservableCollection<Friend>();
        
        // controller for facebook functions
        public FacebookViewModel facebookApi;
        // controller for geolocation. 
        public LocationController lController;
        // application location
        public TaskCompletionSource<bool> GettingEventsFinished;
       

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
            _userEvents = new ObservableCollection<Event>();
            _queriedEvents = new ObservableCollection<Event>();
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

                        // check for the events with the same title. 
                        if (!_queriedEvents.Any(p => p.name.Contains(itemEvent.name)))
                        {
                            // if two events are having the same cordinates make them differ in location so they can show up on map. 
                            if (_queriedEvents.Any(p => p.Location.Longitude == itemEvent.Location.Longitude &&
                                                                p.Location.Latitude == itemEvent.Location.Latitude)) 
                            {
                                itemEvent.Location.Longitude += new Random().NextDouble() * 0.001; 
                                itemEvent.Location.Latitude += new Random().NextDouble() * 0.001; 
                            }

                            // fill it in item lsit of events
                            _queriedEvents.Add(itemEvent);
                        }
                       
                        
                     
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
                        _userEvents.Add(itemEvent);

                    }
                    }
                }
            }
            


        }
    }
    }
