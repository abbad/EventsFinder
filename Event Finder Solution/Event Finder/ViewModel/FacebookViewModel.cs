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

        async public Task<List<Data>> SearchEventsFromFacebook(string searchString, double offset, double latitude, double longitude, double dt)
        { 
            List<Data> results = new List<Data>();
            String result = await CallFacebookFQL(makeQueryWithContains(offset, latitude, longitude, dt, searchString));
            results.Add(ParseEvents(result));
            return results;
        }

        async public Task<List<Data>> GetEventsFromFacebook(double offset, double latitude, double longitude, double dt, double dtEndRange) 
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

        async public Task<bool> attendEvent(string eID) {
            var fb = new Facebook.FacebookClient(App.FacebookSessionClient.CurrentSession.AccessToken);
            var parameters = new Dictionary<string, object>();
            parameters["access_token"] = App.FacebookSessionClient.CurrentSession.AccessToken;
            bool result = (bool)await fb.PostTaskAsync(String.Format(@"/{0}/attending", eID), parameters);

            return result;
        }

        async public Task<bool> declineEvent(string eID) {
            var fb = new Facebook.FacebookClient(App.FacebookSessionClient.CurrentSession.AccessToken);
            var parameters = new Dictionary<string, object>();
            parameters["access_token"] = App.FacebookSessionClient.CurrentSession.AccessToken;
            bool result = (bool)await fb.PostTaskAsync(String.Format(@"/{0}/declined", eID), parameters);

            return result;
        }

        async public Task<bool> maybeEvent(string eID) {
            var fb = new Facebook.FacebookClient(App.FacebookSessionClient.CurrentSession.AccessToken);
            var parameters = new Dictionary<string, object>();
            parameters["access_token"] = App.FacebookSessionClient.CurrentSession.AccessToken;
            bool result = (bool)await fb.PostTaskAsync(String.Format(@"/{0}/maybe", eID), parameters);

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


        private String makeQueryWithContains(double offset, double latitude, double longitude, double dt, String searchString)
        {
            return String.Format(@"SELECT eid, start_time, end_time, pic_big, pic_square, name, description, venue FROM event WHERE contains('""{5}""') AND venue.latitude > ""{0}"" AND venue.latitude < ""{1}"" AND venue.longitude > ""{2}"" AND venue.longitude < ""{3}"" AND start_time > ""{4}"" LIMIT 40 ORDER BY start_time ASC",
                                       (offset - latitude).ToString(),
                                       (offset + latitude).ToString(),
                                       (offset - longitude).ToString(),
                                       (offset + longitude).ToString(),
                                       dt.ToString(),
                                       searchString);
                                        
        }

        private String makeQueryRSVPStatus(String eid) 
        {
            return String.Format("SELECT rsvp_status FROM event_member WHERE eid={0} AND uid=me()", eid);
        }

        private String MakeQueryForMeAndFriendsEvents(double offset, double latitude, double longitude, double dt, double dtEndRange)
        {
            return String.Format(@"SELECT eid, start_time, end_time, pic_big,pic_square, name, description, venue FROM event WHERE eid IN (SELECT eid FROM event_member WHERE uid IN (SELECT uid2 FROM friend WHERE uid1 = me()) AND start_time > now() OR uid = me()) AND venue.latitude > ""{0}"" AND venue.latitude < ""{1}"" AND venue.longitude > ""{2}"" AND venue.longitude < ""{3}"" AND start_time > ""{4}"" and start_time < ""{5}""  ORDER BY start_time ASC", 
                                        (offset - latitude).ToString(),
                                       (offset + latitude).ToString(),
                                       (offset - longitude).ToString(),
                                       (offset + longitude).ToString(),
                                       dt.ToString(),
                                       dtEndRange.ToString());
                                        
        }

    }
}
