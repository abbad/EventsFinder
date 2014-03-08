using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event_Finder.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.ObjectModel;
using System.Collections;
using Facebook;

namespace Event_Finder.ViewModel
{
    public class FacebookViewModel
    {

        async public Task<FriendRoot> GetFriendsAttendingEvent(string eid) 
        {
            String result = await CallFacebookFQL(MakeQueryForFriendsAttendingEvent(eid));
            return ParseFriends(result);
        }

        async public Task<RootObject> GetRSVPStatusForUser(String eID) 
        {
            String result = await CallFacebookFQL(MakeQueryRSVPStatus(eID));

            List<string> errors = new List<string>();
            return JsonConvert.DeserializeObject<RootObject>(result,
                new JsonSerializerSettings
                {
                    Error = delegate(object sender, ErrorEventArgs args)
                    {
                        errors.Add(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }
                });
        }

        async public Task<List<Data>> getListOfEventsAttendedByUser(double dt, double dtEndRange)
        {
            List<Data> results = new List<Data>();
            String result = await CallFacebookFQL(MakeQueryForUserEvents(dt, dtEndRange));
            results.Add(ParseEvents(result));
            return results;
        
        }

        async public Task<List<Data>> GetAllEvents(string searchString, double offset, double latitude, double longitude, double dt, double dtEndRange)
        { 
            List<Data> results = new List<Data>();
            String result = await CallFacebookFQL(MakeQueryWithContains(offset, latitude, longitude, dt, dtEndRange, searchString));
            results.Add(ParseEvents(result));
            result = await CallFacebookFQL(MakeQueryForMeAndFriendsEvents(offset, latitude, longitude, dt, dtEndRange));
            results.Add(ParseEvents(result));
            return results;
            
        }

        async public Task<List<Data>> SearchEventsFromFacebook(string searchString, double offset, double latitude, double longitude, double dt, double dtEndRange)
        { 
            List<Data> results = new List<Data>();
            String result = await CallFacebookFQL(MakeQueryWithContains(offset, latitude, longitude, dt, dtEndRange, searchString));
            results.Add(ParseEvents(result));
            return results;
        }

        async public Task<List<Data>> GetEventsFromFriendsFacebook(double offset, double latitude, double longitude, double dt, double dtEndRange) 
        {
            List<Data> results = new List<Data>();
            String result = await CallFacebookFQL(MakeQueryForMeAndFriendsEvents(offset, latitude, longitude, dt, dtEndRange));
            results.Add(ParseEvents(result));
            return results; 
        }

        private Data ParseEvents(String jsonListOfEvents)
        {
            List<string> errors = new List<string>();
            return JsonConvert.DeserializeObject<Data>(jsonListOfEvents,
                new JsonSerializerSettings {
                    Error = delegate(object sender, ErrorEventArgs args)
                    {
                          errors.Add(args.ErrorContext.Error.Message);
                           args.ErrorContext.Handled = true;
                    }
                });
        }

        private FriendRoot ParseFriends(String listOfFriends) 
        {
            List<string> errors = new List<string>();
            return JsonConvert.DeserializeObject<FriendRoot>(listOfFriends,
                new JsonSerializerSettings
                {
                    Error = delegate(object sender, ErrorEventArgs args)
                    {
                        errors.Add(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }
                });
            
        
        }

        /// <summary>
        ///  Function that will try to reserve a person in an event.
        /// </summary>
        /// <param name="eID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        async public Task<bool> RSVPEvent(string eID, String status) {
            var fb = new Facebook.FacebookClient(App.AccessToken);
            var parameters = new Dictionary<string, object>();
            parameters["access_token"] = App.AccessToken;
            bool result;
            try
            {
                result = (bool)await fb.PostTaskAsync(String.Format(@"/{0}/{1}", eID, status), parameters);
            }
            catch (Facebook.FacebookOAuthException)
            {
                result = false;
            }
            return result;
        }



        async private Task<string> CallFacebookFQL(String Query)
        {
            var fb = new Facebook.FacebookClient(App.AccessToken);

            var result = await fb.GetTaskAsync("fql",
                new
                {
                    q = Query
                });

            return result.ToString(); 
        }


        private String MakeQueryWithContains(double offset, double latitude, double longitude, double dt, double dtEndRange, String searchString)
        {
            return String.Format(@"SELECT eid, start_time, end_time, pic_big, pic_square, name, host, description, venue FROM event WHERE contains('""{5}""') AND venue.latitude > ""{0}"" AND venue.latitude < ""{1}"" AND venue.longitude > ""{2}"" AND venue.longitude < ""{3}"" AND start_time >= ""{4}"" and start_time <= ""{6}"" ORDER BY start_time ASC",
                                       (latitude - offset).ToString(),
                                       (latitude + offset).ToString(),
                                       (longitude- offset).ToString(),
                                       (longitude + offset).ToString(),
                                       dt.ToString(),
                                       searchString,
                                       dtEndRange);
                                        
        }

        private String MakeQueryRSVPStatus(String eid) 
        {
            return String.Format("SELECT rsvp_status FROM event_member WHERE eid={0} AND uid=me()", eid);
        }

        private String MakeQueryForMeAndFriendsEvents(double offset, double latitude, double longitude, double dt, double dtEndRange)
        {
            return String.Format(@"SELECT eid, start_time, end_time, host, pic_big, pic_square, pic_cover, name, description, venue FROM event WHERE eid IN (SELECT eid FROM event_member WHERE uid IN (SELECT uid2 FROM friend WHERE uid1 = me() limit 300) limit 2500) AND venue.latitude > ""{0}"" AND venue.latitude < ""{1}"" AND venue.longitude > ""{2}"" AND venue.longitude < ""{3}"" AND start_time >= ""{4}"" and start_time <= ""{5}""  ORDER BY start_time ASC LIMIT 100", 
                                       (latitude - offset).ToString(),
                                       (latitude + offset).ToString(),
                                       (longitude - offset).ToString(),
                                       (longitude + offset).ToString(),
                                       dt.ToString(),
                                       dtEndRange.ToString());
                                        
        }

        private String MakeQueryForUserEvents(double dt, double dtEndRange) 
        {
            return String.Format(@"SELECT eid, start_time, end_time, host, pic_cover,pic_big, pic_square, name, description, venue FROM event WHERE eid IN (SELECT eid FROM event_member WHERE uid = me()) and start_time >= {0} and start_time <= {1}", dt.ToString(),
                                       dtEndRange.ToString());
        }

        private String MakeQueryForFriendsAttendingEvent(string eid) 
        {
            return String.Format(@"SELECT first_name, last_name, pic_big FROM user WHERE uid IN (SELECT uid FROM event_member WHERE eid = {0}  AND rsvp_status = 'attending' AND uid IN (SELECT uid2 FROM friend WHERE uid1 = me()))", eid);
        }

    }
}
