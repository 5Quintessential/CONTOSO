using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Common.OAuthLib;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp
{
    /// <summary>
    /// Search Yahoo Business
    /// </summary>
    /// <see cref="http://www.yelp.com/developers/documentation/v2/business">Online documentation</see>
    public class BusinessHelper : Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp.IBusinessHelper
    {
        private OAuthManager OAuth;

        public BusinessHelper(OAuthManager oAuth)
        {
            this.OAuth = oAuth;
        }

        public BusinessHelper(string yelpOAuthConsumerKey, string yelpOAuthConsumerSecret, string yelpOAuthToken, string yelpOAuthTokenSecret)
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

        public async Task<IEnumerable<Business>> SearchAsync(IEnumerable<string> businessIds)
        {
            var _List = new List<Business>();
            foreach (var item in businessIds.AsParallel())
                _List.Add(await SearchAsync(item));
            return _List;
        }

        public async Task<Business> SearchAsync(string businessId)
        {
            // build url
            var _Url = string.Format("http://api.yelp.com/v2/business/{0}", System.Uri.EscapeDataString(businessId));
            return await SearchAsync(new Uri(_Url));
        }

        public async Task<Business> SearchAsync(Uri uri)
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
                    throw new InvalidSomethingException("Error in JSON") { JSON = _JsonString, URL = uri };

                // deserialize json to objects
                var _JsonBytes = Encoding.Unicode.GetBytes(_JsonString);
                using (MemoryStream _MemoryStream = new MemoryStream(_JsonBytes))
                {
                    var _JsonSerializer = new DataContractJsonSerializer(typeof(Business));
                    var _Result = (Business)_JsonSerializer.ReadObject(_MemoryStream);
                    return _Result;
                }
            }
            catch (Exceptions.FoodLibServiceException) { throw; }
            catch (Exception e) { throw new Exceptions.FoodLibServiceException(e) { JSON = _JsonString, Query = uri }; }
        }

        public class InvalidSomethingException : Exception
        {
            public InvalidSomethingException(string message) : base(message) { }
            public InvalidSomethingException(Exception e) : base(string.Empty, e) { }
            public string JSON { get; set; }
            public Uri URL { get; set; }
        }

        public class Option
        {
            public string formatted_original_price { get; set; }
            public string formatted_price { get; set; }
            public bool? is_quantity_limited { get; set; }
            public int original_price { get; set; }
            public int price { get; set; }
            public string purchase_url { get; set; }
            public int remaining_count { get; set; }
            public string title { get; set; }
        }

        public class Deal
        {
            public string currency_code { get; set; }
            public string image_url { get; set; }
            public List<Option> options { get; set; }
            public string url { get; set; }
            public bool? is_popular { get; set; }
            public int time_start { get; set; }
            public string title { get; set; }
        }

        public class Coordinate
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class Location
        {
            public List<string> address { get; set; }
            public string city { get; set; }
            public Coordinate coordinate { get; set; }
            public string country_code { get; set; }
            public string cross_streets { get; set; }
            public List<string> display_address { get; set; }
            public int geo_accuracy { get; set; }
            public List<string> neighborhoods { get; set; }
            public string postal_code { get; set; }
            public string state_code { get; set; }
        }

        public class User
        {
            public string id { get; set; }
            public string image_url { get; set; }
            public string name { get; set; }
        }

        public class Review
        {
            public string excerpt { get; set; }
            public string id { get; set; }
            public int rating { get; set; }
            public string rating_image_large_url { get; set; }
            public string rating_image_small_url { get; set; }
            public string rating_image_url { get; set; }
            public int time_created { get; set; }
            public User user { get; set; }
        }

        [DataContract(Name = "RootData")]
        public class Business
        {
            [DataMember]
            public List<List<string>> categories { get; set; }
            [DataMember]
            public List<Deal> deals { get; set; }
            [DataMember]
            public string display_phone { get; set; }
            [DataMember]
            public string id { get; set; }
            [DataMember]
            public string image_url { get; set; }
            [DataMember]
            public bool is_claimed { get; set; }
            [DataMember]
            public bool is_closed { get; set; }
            [DataMember]
            public Location location { get; set; }
            [DataMember]
            public string mobile_url { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string phone { get; set; }
            [DataMember]
            public double rating { get; set; }
            [DataMember]
            public string rating_img_url { get; set; }
            [DataMember]
            public string rating_img_url_large { get; set; }
            [DataMember]
            public string rating_img_url_small { get; set; }
            [DataMember]
            public int review_count { get; set; }
            [DataMember]
            public List<Review> reviews { get; set; }
            [DataMember]
            public string snippet_image_url { get; set; }
            [DataMember]
            public string snippet_text { get; set; }
            [DataMember]
            public string url { get; set; }

            // custom for this app
            public string FactualId { get; set; }
        }
    }
}
