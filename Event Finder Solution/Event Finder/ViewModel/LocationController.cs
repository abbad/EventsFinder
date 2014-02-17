using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Event_Finder.geocodeservice;
using System.Net;
using System.Xml.Linq;

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

        async public Task<String> ReverseGeocodeGoogle(double longitude, double latitude) 
        {
            string key = "AIzaSyBPZWRyiAem6zsb2b08dzNS-O0A0n-NTkQ";
            
            var requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?latlng={0},{1}&sensor=true&key={2}", longitude.ToString(), latitude.ToString(), key);
            
            var request = WebRequest.Create(requestUri);
            var response = await request.GetResponseAsync();
            var xdoc = XDocument.Load(response.ToString());

            var result = xdoc.Element("GeocodeResponse").Element("result");
            var locationElement = result.Element("long_name");
            //var lat = locationElement.Element("lat");
            //var lng = locationElement.Element("lng");
            
            return null;
        }

        async public Task<String> ReverseGeocodePoint(Bing.Maps.Location l)
        {
            //return await ReverseGeocodeGoogle(l.Longitude, l.Latitude);
            
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
           
            // sometimes reverseGeocode is not getting me the right location so we will try to call the service a couple of times until it works, after a bit of tryouts we just return an error. 
            int numberOfTries = 10;
            GeocodeResponse geocodeResponse = await geocodeService.ReverseGeocodeAsync(reverseGeocodeRequest);
            
            while (geocodeResponse.Results.Count == 0) {
                geocodeResponse = await geocodeService.ReverseGeocodeAsync(reverseGeocodeRequest);
                
                if (numberOfTries == 0)
                    break;
                numberOfTries--;
            }
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
