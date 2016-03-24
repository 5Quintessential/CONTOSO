using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Common.OAuthLib;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp
{
    /// <summary>Search Yelp (v2 API)</summary>
    /// <see cref="http://www.yelp.com/developers/documentation/v2/search_api">Online documentation</see>
    public class SearchHelper
    {
        private OAuthManager OAuth;

        public SearchHelper(OAuthManager oAuth)
        {
            this.OAuth = oAuth;
        }

        public SearchHelper(string yelpOAuthConsumerKey, string yelpOAuthConsumerSecret, string yelpOAuthToken, string yelpOAuthTokenSecret)
        {
            if (string.IsNullOrWhiteSpace(yelpOAuthConsumerKey))
                throw new ArgumentNullException("yelpOAuthConsumerKey");
            if (string.IsNullOrWhiteSpace(yelpOAuthConsumerSecret))
                throw new ArgumentNullException("yelpOAuthConsumerSecret");
            if (string.IsNullOrWhiteSpace(yelpOAuthToken))
                throw new ArgumentNullException("yelpOAuthToken");
            if (string.IsNullOrWhiteSpace(yelpOAuthTokenSecret))
                throw new ArgumentNullException("yelpOAuthTokenSecret");
            OAuth = new OAuthManager(yelpOAuthConsumerKey, yelpOAuthConsumerSecret, yelpOAuthToken, yelpOAuthTokenSecret);
        }

        public async Task<IEnumerable<Business>> SearchAsync(string query, double latitude, double longitude, string category = null, int radius = 25, int limit = 20)
        {
            // build url
            var _Url = "http://api.yelp.com/v2/search?";
            _Url += string.Format("term={0}&", System.Uri.EscapeDataString(query));
            if (!string.IsNullOrWhiteSpace(category))
                _Url += string.Format("category_filter={0}&", System.Uri.EscapeDataString(category));
            _Url += string.Format("ll={0}&", System.Uri.EscapeDataString(latitude.ToString() + "," + longitude.ToString()));
            _Url += string.Format("radius={0}&", System.Uri.EscapeDataString(radius.ToString()));
            _Url += string.Format("limit={0}&", System.Uri.EscapeDataString(limit.ToString()));
            _Url = _Url.TrimEnd('&');

            return await SearchAsync(new Uri(_Url));
        }

        public async Task<IEnumerable<Business>> SearchAsync(Uri uri)
        {
            string _JsonString = string.Empty;
            try
            {
                // call service
                var _Request = WebRequest.CreateHttp(uri.ToString());
                _Request.Method = "GET";
                _Request.Headers["Authorization"] = OAuth.GenerateAuthzHeader(uri.ToString(), "GET");

                // process response
                var _Response = (HttpWebResponse)await _Request.GetResponseAsync();
                using (var _Reader = new StreamReader(_Response.GetResponseStream()))
                {
                    _JsonString = _Reader.ReadToEnd();
                }

                // check for error
                if (_JsonString.Contains("{\"Error\":"))
                    throw new InvalidSomethingException("Error in JSON") { JSON = _JsonString };

                // deserialize json to objects
                var _JsonBytes = Encoding.Unicode.GetBytes(_JsonString);
                using (MemoryStream _MemoryStream = new MemoryStream(_JsonBytes))
                {
                    var _JsonSerializer = new DataContractJsonSerializer(typeof(RootObject));
                    var _Result = (RootObject)_JsonSerializer.ReadObject(_MemoryStream);
                    return _Result.businesses;
                }
            }
            catch (InvalidSomethingException) { throw; }
            catch (Exception e) { throw new InvalidSomethingException(e) { JSON = _JsonString }; }
        }
        public class InvalidSomethingException : Exception
        {
            public InvalidSomethingException(string message) : base(message) { }
            public InvalidSomethingException(Exception e) : base(string.Empty, e) { }
            public string JSON { get; set; }
        }

        public class Span
        {
            public double latitude_delta { get; set; }
            public double longitude_delta { get; set; }
        }

        public class Center
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class Region
        {
            public Span span { get; set; }
            public Center center { get; set; }
        }

        public class Coordinate
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class Location
        {
            public string city { get; set; }
            public List<string> display_address { get; set; }
            public int geo_accuracy { get; set; }
            public string postal_code { get; set; }
            public string country_code { get; set; }
            public List<string> address { get; set; }
            public Coordinate coordinate { get; set; }
            public string state_code { get; set; }
        }

        public class Business
        {
            public double distance { get; set; }
            public string mobile_url { get; set; }
            public string rating_img_url { get; set; }
            public int review_count { get; set; }
            public string name { get; set; }
            public double rating { get; set; }
            public string url { get; set; }
            public Location location { get; set; }
            public string phone { get; set; }
            public string snippet_text { get; set; }
            public string image_url { get; set; }
            public string snippet_image_url { get; set; }
            public string display_phone { get; set; }
            public string rating_img_url_large { get; set; }
            public string id { get; set; }
            public List<List<string>> categories { get; set; }
            public string rating_img_url_small { get; set; }
        }

        public class RootObject
        {
            public Region region { get; set; }
            public int total { get; set; }
            public List<Business> businesses { get; set; }
        }
    }
}