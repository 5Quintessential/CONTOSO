using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Food.Lib.Services
{
    /// <summary>Serch Yelp (API v1)</summary>
    /// <see cref="http://www.yelp.com/developers/documentation/search_api">Yelp v1 API for develoeprs web site</see>
    public class YelpHelper
    {
        /*
         * var _Location = _LocationHelper.Position;
         * var _Latitude = _Location.Latitude;
         * var _Longitude = _Location.Longitude;
         * var _Helper = new YelpHelper("gmsU5votvNzGcbCR8ODWfg");
         * var _Results = _Helper.Search("coffee", _Latitude, _Longitude);
         * // TODO: use results
         */

        private string YelpKey;
        public YelpHelper(string YelpKey)
        {
            this.YelpKey = YelpKey;
        }

        /// <summary>
        /// Enables searching for business review information within a certain radius of a particular location.
        /// </summary>
        /// <param name="query">String representing the name of business or search term being requested. (E.G. "bars") </param>
        /// <param name="latitude">Latitude of geo-point to search near. </param>
        /// <param name="longitude">Longitude of geo-point to search near. </param>
        /// <param name="limit">Specifies the number of businesses to return in the result set. Default is 10. Minimum value is 1 and maximum value is 20 </param>
        /// <param name="radius">Radius to use while searching around specified geo-point. Maximum value = 25. If a radius is not specified, it will be chosen based on the specificity of the location.</param>
        /// <param name="category">This parameter allows filtering of result set by a particular category. You can search in multiple categories by separating them with a plus character, e.g category=bars+poolhalls </param>
        /// <returns></returns>
        public async Task<ObservableCollection<Business>> SearchAsync(string query, double latitude, double longitude, string category = null, int limit = 20, int radius = 25)
        {
            var _Url = "http://api.yelp.com/business_review_search?";
            _Url += string.Format("ywsid={0}&", YelpKey);
            _Url += string.Format("lat={0}&", latitude);
            _Url += string.Format("long={0}&", longitude);
            _Url += string.Format("limit={0}&", limit);
            _Url += string.Format("radius={0}&", radius);
            _Url += string.Format("term={0}&", System.Uri.EscapeDataString(query));
            _Url = _Url.TrimEnd('&');
            var _Result = await CallService(new Uri(_Url));
            if (_Result == null)
                return null;
            var _List = new ObservableCollection<Business>();
            foreach (var item in _Result.businesses)
                _List.Add(item);
            return _List;
        }

        private async Task<RootObject> GetWebResult<T>(Uri uri)
        {
            // fetch from rest service
            var _HttpClient = new System.Net.Http.HttpClient();
            var _HttpResponse = await _HttpClient.GetAsync(uri.ToString());
            var _JsonString = await _HttpResponse.Content.ReadAsStringAsync();

            // deserialize json to objects
            var _JsonBytes = Encoding.Unicode.GetBytes(_JsonString);
            using (MemoryStream _MemoryStream = new MemoryStream(_JsonBytes))
            {
                var _JsonSerializer = new DataContractJsonSerializer(typeof(RootObject));
                var _Result = (RootObject)_JsonSerializer.ReadObject(_MemoryStream);
                return _Result;
            }
        }

        // helper method
        private async Task<RootObject> CallService(Uri uri)
        {
            string _JsonString = string.Empty;
            try
            {
                // fetch from rest service
                var _HttpClient = new System.Net.Http.HttpClient();
                var _HttpResponse = await _HttpClient.GetAsync(uri.ToString());
                _JsonString = await _HttpResponse.Content.ReadAsStringAsync();

                // check for error
                if (_JsonString.Contains("{\"Error\":"))
                    throw new InvalidResultException { JSON = _JsonString };

                // deserialize json to objects
                var _JsonBytes = Encoding.Unicode.GetBytes(_JsonString);
                using (MemoryStream _MemoryStream = new MemoryStream(_JsonBytes))
                {
                    var _JsonSerializer = new DataContractJsonSerializer(typeof(RootObject));
                    var _Result = (RootObject)_JsonSerializer.ReadObject(_MemoryStream);
                    return _Result;
                }
            }
            catch (InvalidSomethingException) { throw; }
            catch (Exception e) { throw new InvalidSomethingException(e) { JSON = _JsonString }; }
        }

        public class InvalidSomethingException : Exception
        {
            public InvalidSomethingException() { }
            public InvalidSomethingException(Exception e) : base(string.Empty, e) { }
            public string JSON { get; set; }
        }
        public class InvalidCredentialsException : InvalidSomethingException { }
        public class InvalidResultException : InvalidSomethingException { }

        // JSON

        public class Message
        {
            public string text { get; set; }
            public int code { get; set; }
            public string version { get; set; }
        }

        public class Category
        {
            public string category_filter { get; set; }
            public string search_url { get; set; }
            public string name { get; set; }
        }

        public class Review
        {
            public string rating_img_url_small { get; set; }
            public string user_photo_url_small { get; set; }
            public string rating_img_url { get; set; }
            public int rating { get; set; }
            public string user_url { get; set; }
            public string url { get; set; }
            public string mobile_uri { get; set; }
            public string text_excerpt { get; set; }
            public string user_photo_url { get; set; }
            public string date { get; set; }
            public string user_name { get; set; }
            public string id { get; set; }
        }

        public class Business
        {
            public string rating_img_url { get; set; }
            public string country_code { get; set; }
            public string id { get; set; }
            public bool is_closed { get; set; }
            public string city { get; set; }
            public string mobile_url { get; set; }
            public int review_count { get; set; }
            public string zip { get; set; }
            public string state { get; set; }
            public double latitude { get; set; }
            public string rating_img_url_small { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string address3 { get; set; }
            public string phone { get; set; }
            public string state_code { get; set; }
            public List<Category> categories { get; set; }
            public string photo_url { get; set; }
            public double distance { get; set; }
            public string name { get; set; }
            public List<object> neighborhoods { get; set; }
            public string url { get; set; }
            public string country { get; set; }
            public double avg_rating { get; set; }
            public double longitude { get; set; }
            public string nearby_url { get; set; }
            public List<Review> reviews { get; set; }
            public string photo_url_small { get; set; }
        }

        public class RootObject
        {
            public Message message { get; set; }
            public List<Business> businesses { get; set; }
        }
    }
}
