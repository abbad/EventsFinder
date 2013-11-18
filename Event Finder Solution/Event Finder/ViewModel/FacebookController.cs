using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Event_Finder.Models;

namespace Event_Finder.ViewModel
{
    class FacebookController
    {

        async public Task<Data> GetEventsFromFacebook(double offset, double latitude, double longitude, double dt) 
        {
            String result = await CallFacebookApiForEvents(offset, latitude, longitude, dt);
            return ParseEvents(result);

        }

        private Data ParseEvents(String jsonListOfEvents)
        {
            return JsonConvert.DeserializeObject<Data>(jsonListOfEvents);
        }

        private String makeQuery(double offset, double latitude, double longitude, double dt) 
        {
            return String.Format(@"SELECT eid, name, description, venue FROM event WHERE contains('amman') OR contains('irbid') AND venue.latitude > ""{0}"" AND venue.latitude < ""{1}"" AND venue.longitude > ""{2}"" AND venue.longitude < ""{3}"" AND start_time > ""{4}"" LIMIT 40",
                                       (offset - latitude).ToString(),
                                       (offset + latitude).ToString(),
                                       (offset - longitude).ToString(),
                                       (offset + longitude).ToString(),
                                       dt.ToString());        
        }

        async private Task<string> CallFacebookApiForEvents(double offset, double latitude, double longitude, double dt)
        {
            var fb = new Facebook.FacebookClient(App.FacebookSessionClient.CurrentSession.AccessToken);

            string query = makeQuery(offset, latitude, longitude, dt); 

            var result = await fb.GetTaskAsync("fql",
                new
                {
                    q = query
                });

            return result.ToString(); 
        }


    }
}
