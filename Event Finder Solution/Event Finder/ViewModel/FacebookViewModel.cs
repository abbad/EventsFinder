using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

      
        public FacebookViewModel() 
        {
        }

        async public Task<RootObject> GetRSVPStatusForUser(String eID) 
        {
            String result = await CallFacebookFQL(makeQueryRSVPStatus(eID));

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
            String result = await CallFacebookFQL(makeQueryWithContains(offset, latitude, longitude, dt, dtEndRange, searchString));
            results.Add(ParseEvents(result));
            result = await CallFacebookFQL(MakeQueryForMeAndFriendsEvents(offset, latitude, longitude, dt, dtEndRange));
            results.Add(ParseEvents(result));
            return results;
            
        }

        async public Task<List<Data>> SearchEventsFromFacebook(string searchString, double offset, double latitude, double longitude, double dt, double dtEndRange)
        { 
            List<Data> results = new List<Data>();
            String result = await CallFacebookFQL(makeQueryWithContains(offset, latitude, longitude, dt, dtEndRange, searchString));
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

        public Data ParseEvents(String jsonListOfEvents)
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


        /// <summary>
        ///  Function that will try to reserve a person in an event.
        /// </summary>
        /// <param name="eID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        async public Task<bool> RSVPEvent(string eID, String status) {
            var fb = new Facebook.FacebookClient(App.FacebookSessionClient.CurrentSession.AccessToken);
            var parameters = new Dictionary<string, object>();
            parameters["access_token"] = App.FacebookSessionClient.CurrentSession.AccessToken;
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
            var fb = new Facebook.FacebookClient(App.FacebookSessionClient.CurrentSession.AccessToken);

            var result = await fb.GetTaskAsync("fql",
                new
                {
                    q = Query
                });

            return result.ToString(); 
        }


        private String makeQueryWithContains(double offset, double latitude, double longitude, double dt, double dtEndRange, String searchString)
        {
            //AND venue.longitude < \''. ($long+$offset) .'\' AND venue.latitude < \''. ($lat+$offset) .'\' AND venue.longitude > \''. ($long-$offset) .'\' AND venue.latitude > \''. ($lat-$offset) .'\' ORDER BY start_time ASC '. $limit
            return String.Format(@"SELECT eid, start_time, end_time, pic_big, pic_square, name, description, venue FROM event WHERE contains('""{5}""') AND venue.latitude > ""{0}"" AND venue.latitude < ""{1}"" AND venue.longitude > ""{2}"" AND venue.longitude < ""{3}"" AND start_time > ""{4}"" and start_time < ""{6}"" ORDER BY start_time ASC",
                                       (latitude - offset).ToString(),
                                       (latitude + offset).ToString(),
                                       (longitude- offset).ToString(),
                                       (longitude + offset).ToString(),
                                       dt.ToString(),
                                       searchString,
                                       dtEndRange);
                                        
        }

        private String makeQueryRSVPStatus(String eid) 
        {
            return String.Format("SELECT rsvp_status FROM event_member WHERE eid={0} AND uid=me()", eid);
        }

        private String MakeQueryForMeAndFriendsEvents(double offset, double latitude, double longitude, double dt, double dtEndRange)
        {
            return String.Format(@"SELECT eid, start_time, end_time, pic_big,pic_square, name, description, venue FROM event WHERE eid IN (SELECT eid FROM event_member WHERE uid IN (SELECT uid2 FROM friend WHERE uid1 = me()) OR uid = me()) AND venue.latitude > ""{0}"" AND venue.latitude < ""{1}"" AND venue.longitude > ""{2}"" AND venue.longitude < ""{3}"" AND start_time > ""{4}"" and start_time < ""{5}""  ORDER BY start_time ASC", 
                                       (latitude - offset).ToString(),
                                       (latitude + offset).ToString(),
                                       (longitude - offset).ToString(),
                                       (longitude + offset).ToString(),
                                       dt.ToString(),
                                       dtEndRange.ToString());
                                        
        }

        private String MakeQueryForUserEvents(double dt, double dtEndRange) 
        {
            return String.Format(@"SELECT eid, start_time, end_time, pic_big, pic_square, name, description, venue FROM event WHERE eid IN (
                SELECT eid FROM event_member WHERE uid = me()) and start_time > {0} and start_time < {1}", dt.ToString(),
                                       dtEndRange.ToString());
        }

       

    }
}
