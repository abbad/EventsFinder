using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Event_Finder_Core
{
    public class Facebook
    {

        /// <summary>
        /// This function will get you the list of events happening in an area. It will use facebook api to get the information. 
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public String GetListOfEventS(String latitude, String longitude, float offset)
        {
            // The query 
            String events = "SELECT pic_big, name, venue, location, start_time, eid FROM event WHERE eid IN (SELECT eid FROM event_member WHERE uid IN (SELECT uid2 FROM friend WHERE uid1 = me()) AND start_time > '. $created_time .' OR uid = me()) AND start_time > '. $created_time .' AND venue.longitude < \''. ($long+$offset) .'\' AND venue.latitude < \''. ($lat+$offset) .'\' AND venue.longitude > \''. ($long-$offset) .'\' AND venue.latitude > \''. ($lat-$offset) .'\' ORDER BY start_time ASC '. $limit";

            return null;
        }

    }
}
