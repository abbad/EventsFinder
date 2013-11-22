using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Event_Finder.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Event_Finder.ViewModel
{
    public class FacebookController
    {

        public List<Data> results;

        public FacebookController() 
        {
            results = new List<Data>();
        }

        async public void GetEventsFromFacebook(double offset, double latitude, double longitude, double dt) 
        {
            String result = await CallFacebookFQL(makeQueryWithContains(offset, latitude, longitude, dt));
            results.Add(ParseEvents(result));
            result = await CallFacebookFQL(MakeQueryForMeAndFriendsEvents(offset, latitude, longitude, dt));
            results.Add(ParseEvents(result));
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

        private String makeQueryWithContains(double offset, double latitude, double longitude, double dt) 
        {
            return String.Format(@"SELECT eid, name, description, venue FROM event WHERE contains('amman') OR contains('irbid') AND venue.latitude > ""{0}"" AND venue.latitude < ""{1}"" AND venue.longitude > ""{2}"" AND venue.longitude < ""{3}"" AND start_time > ""{4}"" LIMIT 40",
                                       (offset - latitude).ToString(),
                                       (offset + latitude).ToString(),
                                       (offset - longitude).ToString(),
                                       (offset + longitude).ToString(),
                                       dt.ToString());        
        }

        private String MakeQueryForMeAndFriendsEvents(double offset, double latitude, double longitude, double dt)
        {
            return String.Format(@"SELECT eid, name, description, venue FROM event WHERE eid IN (SELECT eid FROM event_member WHERE uid IN (SELECT uid2 FROM friend WHERE uid1 = me()) AND start_time > now() OR uid = me()) AND venue.latitude > ""{0}"" AND venue.latitude < ""{1}"" AND venue.longitude > ""{2}"" AND venue.longitude < ""{3}"" AND start_time > ""{4}"" ", (offset - latitude).ToString(),
                                       (offset + latitude).ToString(),
                                       (offset - longitude).ToString(),
                                       (offset + longitude).ToString(),
                                       dt.ToString());   
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


    }
}
