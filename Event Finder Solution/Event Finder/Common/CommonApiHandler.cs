﻿    using Bing.Maps;
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
                friendList.Add(fr.data[0]);
            }
            
        }

        private void FillEventsCollection(List<Data> results)
        {
        
            foreach (var result in results)
            {
                foreach (var itemEvent in result.data)
                {

                    if (CheckEventForLatitudeAndLongitude(itemEvent))
                    {
                        itemEvent.Location = new Location(Convert.ToDouble(itemEvent.venue["latitude"]), Convert.ToDouble(itemEvent.venue["longitude"]));
                        // fill it in item lsit of events
                        App.ItemEventsList.Add(itemEvent);
                    }
                }

            }
          

        }

        private bool CheckEventForLatitudeAndLongitude(Event Item) 
        {
            return (Item.venue.ContainsKey("latitude") && Item.venue.ContainsKey("longitude"));
        }

    

        public void FillAttendedEventsByUserInCollection(List<Data> attendedEvents)
        {

            // add attended events to collection.
            
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

                    }
                }
            }
            


        }
    }
    }
