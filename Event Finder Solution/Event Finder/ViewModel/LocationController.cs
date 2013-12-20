using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Event_Finder.geocodeservice;

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

        async public Task<String> ReverseGeocodePoint(Bing.Maps.Location l)
        {
          
            
            string key = "AqzQTQg1GrHIoL2a5Ycf08czzcxAooMpXiADdOgZQYPBtwpuSSf8Fd4y7MUTJo-h";
            ReverseGeocodeRequest reverseGeocodeRequest = new ReverseGeocodeRequest();

            // Set the credentials using a valid Bing Maps key
            reverseGeocodeRequest.Credentials = new geocodeservice.Credentials();
            reverseGeocodeRequest.Credentials.ApplicationId = key;

            // Set the point to use to find a matching address
            geocodeservice.Location point = new geocodeservice.Location();

            point.Latitude = l.Latitude;
            point.Longitude = l.Longitude;

            reverseGeocodeRequest.Location = point;
            
            // Make the reverse geocode request
            GeocodeServiceClient geocodeService = new GeocodeServiceClient(geocodeservice.GeocodeServiceClient.EndpointConfiguration.BasicHttpBinding_IGeocodeService);
            GeocodeResponse geocodeResponse = await geocodeService.ReverseGeocodeAsync(reverseGeocodeRequest);
           
            return geocodeResponse.Results[0].Address.Locality;
        }
    }
}
