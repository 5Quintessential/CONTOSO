using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp
{
    /// <summary>Serch Yelp (API v1)</summary>
    /// <see cref="http://www.yelp.com/developers/documentation/search_api">Yelp v1 API for develoeprs web site</see>
    public class V1Helper : Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp.IV1Helper
    {
        /*
         * var _Helper = new YelpHelper("gmsU5votvNzGcbCR8ODWfg");
         * var _Results = _Helper.Search("coffee", _Latitude, _Longitude);
         */

        private string YelpKey;
        public V1Helper(string YelpKey)
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
        public async Task<IEnumerable<Business>> SearchAsync(string query, double latitude, double longitude, string category = null, int limit = 20, int radius = 25)
        {
            var _Url = "http://api.yelp.com/business_review_search?";
            _Url += string.Format("ywsid={0}&", YelpKey);
            _Url += string.Format("lat={0}&", latitude);
            _Url += string.Format("long={0}&", longitude);
            _Url += string.Format("limit={0}&", limit);
            _Url += string.Format("radius={0}&", radius);
            if (!string.IsNullOrWhiteSpace(category))
                _Url += string.Format("category={0}&", System.Uri.EscapeDataString(category));
            if (!string.IsNullOrWhiteSpace(query))
                _Url += string.Format("term={0}&", System.Uri.EscapeDataString(query));
            _Url = _Url.TrimEnd('&');

            string _JsonString = string.Empty;
            try
            {
                // fetch from rest service
                var _HttpClient = new System.Net.Http.HttpClient();
                var _HttpResponse = await _HttpClient.GetAsync(_Url);
                _JsonString = await _HttpResponse.Content.ReadAsStringAsync();

                // check for error
                if (_JsonString.Contains("Exceeded max daily requests"))
                {
                    Debug.WriteLine("Yelp.V1Helper.SearchAsync: Exceeded max daily requests");
                    Debugger.Break();
                    throw new ExceededLimitException { JSON = _JsonString };
                }
                if (_JsonString.Contains("{\"Error\":"))
                    throw new InvalidResultException { JSON = _JsonString };

                // deserialize json to objects
                var _JsonBytes = Encoding.Unicode.GetBytes(_JsonString);
                using (MemoryStream _MemoryStream = new MemoryStream(_JsonBytes))
                {
                    var _JsonSerializer = new DataContractJsonSerializer(typeof(RootObject));
                    var _Result = (RootObject)_JsonSerializer.ReadObject(_MemoryStream);
                    var _List = new ObservableCollection<Business>();
                    if (_Result == null)
                        return _List;
                    foreach (var item in _Result.businesses)
                        _List.Add(item);
                    return _List;
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
        public class ExceededLimitException : InvalidSomethingException { }
        public class InvalidResultException : InvalidSomethingException { }

        // JSON

        [DataContract]
        public class Message
        {
            [DataMember]
            public string text { get; set; }
            [DataMember]
            public int code { get; set; }
            [DataMember]
            public string version { get; set; }
        }

        [DataContract]
        public class Category
        {
            [DataMember]
            public string category_filter { get; set; }
            [DataMember]
            public string search_url { get; set; }
            [DataMember]
            public string name { get; set; }
        }

        [DataContract]
        public class Review
        {
            [DataMember]
            public string rating_img_url_small { get; set; }
            [DataMember]
            public string user_photo_url_small { get; set; }
            [DataMember]
            public string rating_img_url { get; set; }
            [DataMember]
            public double rating { get; set; }
            [DataMember]
            public string user_url { get; set; }
            [DataMember]
            public string url { get; set; }
            [DataMember]
            public string mobile_uri { get; set; }
            [DataMember]
            public string text_excerpt { get; set; }
            [DataMember]
            public string user_photo_url { get; set; }
            [DataMember]
            public string date { get; set; }
            [DataMember]
            public string user_name { get; set; }
            [DataMember]
            public string id { get; set; }
        }

        [DataContract]
        public class Neighborhood
        {
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string url { get; set; }

            public override string ToString()
            {
                return name;
            }
        }

        public class Business
        {
            [DataMember]
            public string rating_img_url { get; set; }
            [DataMember]
            public string country_code { get; set; }
            [DataMember]
            public string id { get; set; }
            [DataMember]
            public bool is_closed { get; set; }
            [DataMember]
            public string city { get; set; }
            [DataMember]
            public string mobile_url { get; set; }
            [DataMember]
            public int review_count { get; set; }
            [DataMember]
            public string zip { get; set; }
            [DataMember]
            public string state { get; set; }
            [DataMember]
            public double latitude { get; set; }
            [DataMember]
            public string rating_img_url_small { get; set; }
            [DataMember]
            public string address1 { get; set; }
            [DataMember]
            public string address2 { get; set; }
            [DataMember]
            public string address3 { get; set; }
            [DataMember]
            public string phone { get; set; }
            [DataMember]
            public string state_code { get; set; }
            [DataMember]
            public List<Category> categories { get; set; }
            [DataMember]
            public string photo_url { get; set; }
            [DataMember]
            public double distance { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public List<Neighborhood> neighborhoods { get; set; }
            [DataMember]
            public string url { get; set; }
            [DataMember]
            public string country { get; set; }
            [DataMember]
            public double? avg_rating { get; set; }
            public double longitude { get; set; }
            [DataMember]
            public string nearby_url { get; set; }
            [DataMember]
            public List<Review> reviews { get; set; }
            [DataMember]
            public string photo_url_small { get; set; }
        }

        [DataContract]
        public class RootObject
        {
            [DataMember]
            public Message message { get; set; }
            [DataMember]
            public List<Business> businesses { get; set; }
        }
    }
}
