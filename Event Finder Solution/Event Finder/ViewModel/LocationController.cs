using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Event_Finder.ViewModel
{
    class LocationController
    {
        private Geolocator geolocator = null;
        public LocationController() 
        {
            geolocator = new Geolocator();
        }
        
        async public Task<Geoposition> GetCurrentLocation() 
        {
            // Get the location. 
            return await geolocator.GetGeopositionAsync();
            
        }
    }
}
