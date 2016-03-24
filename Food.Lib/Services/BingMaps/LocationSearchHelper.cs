using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.Exceptions;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.BingMaps
{
    public class LocationSearchHelper
    {
        #region Properties

        public String ApiKey { get; private set; }

        #endregion Properties

        #region Constructors

        public LocationSearchHelper(string apiKey)
        {
            this.ApiKey = apiKey;
        }

        #endregion Constructors

        #region Public Nested Classes

        public class Point
        {
            public string type { get; set; }
            public List<double> coordinates { get; set; }
        }

        public class Address
        {
            public string addressLine { get; set; }
            public string adminDistrict { get; set; }
            public string adminDistrict2 { get; set; }
            public string countryRegion { get; set; }
            public string formattedAddress { get; set; }
            public string locality { get; set; }
            public string postalCode { get; set; }
        }

        public class GeocodePoint
        {
            public string type { get; set; }
            public List<double> coordinates { get; set; }
            public string calculationMethod { get; set; }
            public List<string> usageTypes { get; set; }
        }

        public class Resource
        {
            public string __type { get; set; }
            public List<double> bbox { get; set; }
            public string name { get; set; }
            public Point point { get; set; }
            public Address address { get; set; }
            public string confidence { get; set; }
            public string entityType { get; set; }
            public List<GeocodePoint> geocodePoints { get; set; }
            public List<string> matchCodes { get; set; }
        }

        public class ResourceSet
        {
            public int estimatedTotal { get; set; }
            public List<Resource> resources { get; set; }
        }

        public class RootObject
        {
            public string authenticationResultCode { get; set; }
            public string brandLogoUri { get; set; }
            public string copyright { get; set; }
            public List<ResourceSet> resourceSets { get; set; }
            public int statusCode { get; set; }
            public string statusDescription { get; set; }
            public string traceId { get; set; }
        }

        #endregion  Public Nested Classes

        #region Public Methods

        public async Task<ResourceSet> GetLocation(string locationText, int? maxResults)
        {
            string searchPattern =
                "http://dev.virtualearth.net/REST/v1/Locations?key={0}&q={1}&maxResults={3}";

            string searchUrl =
                string.Format(searchPattern,
                    this.ApiKey,
                    Uri.EscapeDataString(locationText),
                    maxResults.HasValue && maxResults.Value <= 20 ? maxResults.Value : 20 );


            string _JsonString = string.Empty;
            try
            {
                // call service
                var _Request = WebRequest.CreateHttp(searchUrl);
                _Request.Method = "GET";

                // process response
                var _Response = (HttpWebResponse)await _Request.GetResponseAsync();
                using (var _Reader = new StreamReader(_Response.GetResponseStream()))
                {
                    _JsonString = _Reader.ReadToEnd();
                }

                // check for error
                if (_JsonString.Contains("{\"Error\":"))
                    throw new FoodLibServiceException("Error in JSON") { JSON = _JsonString };

                // deserialize json to objects
                var _JsonBytes = Encoding.Unicode.GetBytes(_JsonString);
                using (MemoryStream _MemoryStream = new MemoryStream(_JsonBytes))
                {
                    var _JsonSerializer = new DataContractJsonSerializer(typeof(RootObject));
                    var _result = (RootObject)_JsonSerializer.ReadObject(_MemoryStream);

                    if (null != _result &&
                        null != _result.resourceSets)
                    {
                        return _result.resourceSets.FirstOrDefault();
                    }
                }

                return null;
            }
            catch (FoodLibServiceException) { throw; }
            catch (Exception e) { throw new FoodLibServiceException(e) { JSON = _JsonString, Query = new Uri(searchUrl) }; }
        }

        #endregion Public Methods
    }
}
