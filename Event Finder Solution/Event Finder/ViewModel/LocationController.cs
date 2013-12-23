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

        /// <summary>
        /// This function will give the status of your position.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private string GetStatusString(PositionStatus status)
        {
            var strStatus = "";

            switch (status)
            {
                case PositionStatus.Ready:

                    strStatus = "Location is available.";
                    break;

                case PositionStatus.Initializing:
                    strStatus = "Geolocation service is initializing.";
                    break;

                case PositionStatus.NoData:
                    strStatus = "Location service data is not available.";
                    break;

                case PositionStatus.Disabled:
                    strStatus = "Location services are disabled. Use the " +
                                "Settings charm to enable them.";
                    break;

                case PositionStatus.NotInitialized:
                    strStatus = "Location status is not initialized because " +
                                "the app has not yet requested location data.";
                    break;

                case PositionStatus.NotAvailable:
                    strStatus = "Location services are not supported on your system.";
                    break;

                default:
                    strStatus = "Unknown PositionStatus value.";
                    break;
            }

            return (strStatus);

        }
    }
}
