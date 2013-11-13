using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event_Finder.ViewModel
{
    class FacebookController
    {
        public void ParseEvents()
        {
        
        
        }

        async public Task<string> GetEventsFromFacebook(double offset, double latitude, double longitude, DateTime dt)
        {
            var fb = new Facebook.FacebookClient(App.FacebookSessionClient.CurrentSession.AccessToken);

            string query = String.Format(@"SELECT eid, name, description, venue FROM event WHERE contains ('irbid') OR contains('amman') AND venue.latitude > ""{0}"" AND venue.latitude < ""{1}"" AND venue.longitude > ""{2}"" AND venue.longitude < ""{3}"" AND start_time > now() LIMIT 40",
                                       (offset - latitude).ToString(),
                                       (offset + latitude).ToString(),
                                       (offset - longitude).ToString(),
                                       (offset + longitude).ToString());

            var result = await fb.GetTaskAsync("fql",
                new
                {
                    q = query
                });

            return result.ToString(); 
        }


    }
}
