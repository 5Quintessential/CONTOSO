using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Common.OAuthLib;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual
{
    public class SearchHelper : Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual.ISearchHelper
    {
        private OAuthManager OAuth;

        public SearchHelper(OAuthManager oAuth)
        {
            this.OAuth = oAuth;
        }

        public SearchHelper(string factualOAuthConsumerKey, string factualOAuthConsumerSecret)
        {
            if (string.IsNullOrWhiteSpace(factualOAuthConsumerKey))
                throw new ArgumentNullException("factualOAuthConsumerKey");
            if (string.IsNullOrWhiteSpace(factualOAuthConsumerSecret))
                throw new ArgumentNullException("factualOAuthConsumerSecret");
            OAuth = new OAuthManager(factualOAuthConsumerKey, factualOAuthConsumerSecret);
        }

        public async Task<IEnumerable<Restaurant>> SearchAsync(IEnumerable<string> factualIds)
        {
            var _List = new List<Restaurant>();
            foreach (var item in factualIds)
                _List.Add(await SearchAsync(item));
            return _List;
        }

        public async Task<Restaurant> SearchAsync(string factualId)
        {
            StringBuilder searchUrl = new StringBuilder();
            searchUrl.Append("http://api.v3.factual.com/t/restaurants-us?filters=");

            StringBuilder filterString = new StringBuilder();
            filterString.Append("{\"factual_id\":{\"$eq\":\"");
            filterString.Append(Uri.EscapeDataString(factualId));
            filterString.Append("\"}}");

            searchUrl.Append(Uri.EscapeDataString(filterString.ToString()));
            var _results = await FetchAsync(searchUrl.ToString());
            return _results.FirstOrDefault();
        }

        public async Task<IEnumerable<Restaurant>> SearchAsync(string search, double latitude, double longitude, double minRating = 0d, Lib.Models.Cuisines[] cuisine = null, int limit = 25, int miles = 10)
        {
            // q=X&
            var _Search = string.Empty;
            if (!string.IsNullOrWhiteSpace(search))
                _Search = string.Format("q={0}&", System.Uri.EscapeDataString(search));

            // filters={}&
            var _FilterObject = new FactualFilterPart(minRating, cuisine);
            var _FilterJson = Serialize(_FilterObject);
            var _FilterInfo = "filters=" + System.Uri.EscapeDataString(_FilterJson);

            // geo={}&
            var _MilesAsMeters = miles * 1.609344 * 1000;
            var _GeoObject = new FactualGeoPart(latitude, longitude, (int)_MilesAsMeters);
            var _GeoJson = Serialize(_GeoObject);
            var _GeoInfo = "geo=" + System.Uri.EscapeDataString(_GeoJson);

            // build url
            var _QueryString = string.Format("{0}{1}&{2}&limit={3}", _Search, _FilterInfo, _GeoInfo,
                System.Uri.EscapeDataString(limit.ToString()));
            var _Url = "http://api.v3.factual.com/t/restaurants-us?" + _QueryString;

            return await FetchAsync(_Url);
        }

        private async Task<IEnumerable<Restaurant>> FetchAsync(string uri)
        {
            string _JsonString = string.Empty;
            try
            {
                // call service
                var _Request = WebRequest.CreateHttp(uri);
                _Request.Method = "GET";
                _Request.Headers["Authorization"] = OAuth.GenerateCredsHeader(uri, "GET", "");

                // process response
                var _Response = (HttpWebResponse)await _Request.GetResponseAsync();
                using (var _Reader = new StreamReader(_Response.GetResponseStream()))
                {
                    _JsonString = _Reader.ReadToEnd();
                }

                // check for error
                if (_JsonString.Contains("{\"Error\":"))
                {
                    Debugger.Break();
                    throw new InvalidSomethingException("Error in JSON") { JSON = _JsonString };
                }

                // deserialize json to objects
                var _JsonBytes = Encoding.Unicode.GetBytes(_JsonString);
                using (MemoryStream _MemoryStream = new MemoryStream(_JsonBytes))
                {
                    var _JsonSerializer = new DataContractJsonSerializer(typeof(RootObject));
                    var _Result = (RootObject)_JsonSerializer.ReadObject(_MemoryStream);
                    return _Result.response.data;
                }
            }
            catch (InvalidSomethingException) { throw; }
            catch (Exception e) { throw new InvalidSomethingException(e) { JSON = _JsonString, Query = new Uri(uri) }; }
        }

        [DataContract]
        private class FactualGeoPart
        {
            // geo={"$circle":{"$center":[35.001,-18.221],"$meters":5000}}

            public FactualGeoPart(double latitude, double longitude, int meters) { this.Detail = new CircleInfo(latitude, longitude, meters); }

            [DataMember(Name = "$circle")]
            public CircleInfo Detail { get; set; }

            [DataContract]
            public class CircleInfo
            {
                public CircleInfo(double latitude, double longitude, int meters)
                {
                    this.Latitude = latitude;
                    this.Longitude = longitude;
                    this.Meters = meters;
                }

                [IgnoreDataMember]
                public double Latitude { get; private set; }
                [IgnoreDataMember]
                public double Longitude { get; private set; }
                [DataMember(Name = "$center")]
                public double[] Search { get { return new double[] { Latitude, Longitude }; } set { /* nothing */ } }
                [DataMember(Name = "$meters")]
                public int Meters { get; private set; }
            }
        }

        [DataContract]
        [KnownType(typeof(RatingInfo))]
        [KnownType(typeof(CuisineInfo))]
        private class FactualFilterPart
        {
            // filter={"$and":[{"rating":{"$gte":0}},{"cuisine":{"$or":[{"$search":"American"},{"$search":"Mexican"},{"$search":"Italian"},{"$search":"Chinese"}]}}]}

            public FactualFilterPart(double rating, Lib.Models.Cuisines[] cuisine)
            {
                var _Rating = new RatingInfo(rating);
                if (cuisine == null)
                {
                    this.And = new object[] { _Rating };
                    return;
                }
                var _Cuisine = new CuisineInfo(cuisine);
                this.And = new object[] { _Rating, _Cuisine };
            }

            [DataMember(Name = "$and")]
            public object[] And { get; private set; }

            [DataContract]
            [KnownType(typeof(RatingDetails))]
            public class RatingInfo
            {
                public RatingInfo(double rating) { Details = new RatingDetails(rating); }

                [DataMember(Name = "rating")]
                public RatingDetails Details { get; private set; }

                [DataContract]
                public class RatingDetails
                {
                    public RatingDetails(double rating) { this.Rating = rating; }

                    [DataMember(Name = "$gte")]
                    public double Rating { get; private set; }
                }
            }

            [DataContract]
            [KnownType(typeof(CuisineOr))]
            public class CuisineInfo
            {
                public CuisineInfo(IEnumerable<Lib.Models.Cuisines> cuisines) { this.Or = this.Or = cuisines.Select(x => new CuisineOr(x)); }

                [DataMember(Name = "$or")]
                public IEnumerable<CuisineOr> Or { get; private set; }

                [DataContract]
                [KnownType(typeof(CuisineDetails))]
                public class CuisineOr
                {
                    public CuisineOr(Lib.Models.Cuisines cuisine) { this.Details = new CuisineDetails(cuisine); }

                    [DataMember(Name = "cuisine")]
                    public CuisineDetails Details { get; private set; }

                    [DataContract]
                    public class CuisineDetails
                    {
                        public CuisineDetails(Lib.Models.Cuisines cuisine) { this.Cuisine = cuisine.ToString(); }

                        [DataMember(Name = "$search")]
                        public string Cuisine { get; private set; }
                    }
                }
            }
        }

        private static string Serialize(object instance)
        {
            using (MemoryStream _Stream = new MemoryStream())
            {
                var _Serializer = new DataContractJsonSerializer(instance.GetType(), new DataContractJsonSerializerSettings { EmitTypeInformation = EmitTypeInformation.Never });
                _Serializer.WriteObject(_Stream, instance);
                _Stream.Position = 0;
                StreamReader _Reader = new StreamReader(_Stream);
                return _Reader.ReadToEnd(); 
            }
        }

        public class InvalidSomethingException : Exception
        {
            public InvalidSomethingException(string message) : base(message) { }
            public InvalidSomethingException(Exception e) : base(string.Empty, e) { }
            public string JSON { get; set; }
            public Uri Query { get; set; }
        }

        public class HoursOfOpertationEntry
        {
            public TimeSpan OpenTime { get; set; }
            public TimeSpan CloseTime { get; set; }
        }


        // Custom comparer for the Product class
        public class HoursOfOpertationEntryComparer : IEqualityComparer<HoursOfOpertationEntry>
        {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(HoursOfOpertationEntry x, HoursOfOpertationEntry y)
            {
                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                return x.OpenTime == y.OpenTime && x.CloseTime == y.CloseTime;
            }

            // If Equals() returns true for a pair of objects 
            // then GetHashCode() must return the same value for these objects.

            public int GetHashCode(HoursOfOpertationEntry entry)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(entry, null)) return 0;

                //Get hash code for the OpenTime field if it is not null.
                int hashOpenTime = entry.OpenTime == null ? 0 : entry.OpenTime.GetHashCode();

                //Get hash code for the CloseTime field if it is not null.
                int hashCloseTime = entry.CloseTime == null ? 0 : entry.CloseTime.GetHashCode();

                //Calculate the hash code for the entry.
                return hashOpenTime ^ hashCloseTime;
            }

        }

        [DataContract(Name = "Datum")]
        public class Restaurant
        {
            private Dictionary<string, List<HoursOfOpertationEntry>> _hoursOfOperation;
            public Dictionary<string, List<HoursOfOpertationEntry>> HoursOfOperation
            {
                get
                {
                    if (null == _hoursOfOperation)
                    {
                        _hoursOfOperation = new Dictionary<string, List<HoursOfOpertationEntry>>();

                        if (!string.IsNullOrWhiteSpace(hours))
                        {
                            if (hours.StartsWith("{") && hours.EndsWith("}"))
                            {
                                hours = hours.Substring(1, hours.Length - 2);
                                string[] entries = hours.Split(new string[]{ "]]," }, StringSplitOptions.RemoveEmptyEntries);
                                if (null != entries && entries.Length > 0)
                                {
                                    //hours={"1":[["11:30","16:00","Lunch"],["16:00","22:00","Dinner"]],"2":[["11:30","16:00","Lunch"],["16:00","22:00","Dinner"]],"3":[["11:30","16:00","Lunch"],["16:00","22:00","Dinner"]],"4":[["11:30","16:00","Lunch"],["16:00","22:00","Dinner"]],"5":[["11:30","16:00","Lunch"],["16:00","23:00"]],"6":[["11:30","16:00","Lunch"],["16:00","23:00"]],"7":[["11:30","15:00"],["15:00","22:00"]]}
                                    //"1":[["11:30","16:00","Lunch"],["16:00","22:00","Dinner"]]
                                    Regex allHoursRegEx = new Regex("\"([1-7])\"\\:\\[(.*)\\]\\]");
                                    Regex hoursRegEx = new Regex("\\[\"([0-9]+\\:[0-9]+)\"\\,\"([0-9]+\\:[0-9]+)\",?\"?(.*?)\"?]"); 
                                    string [] dayNames = new string[] { "Mon", "Tues", "Wed", "Thu", "Fri", "Sat", "Sun" };
                                    Match m;
                                    int dayIndex = 0;
                                    DayOfWeek day; ;
                                    string[] hrsEntries;
                                    char[] splitPtrn = new char[]{','};
                                    string hrs, temp;
                                    TimeSpan openTime, closeTime;
                                    HoursOfOpertationEntry he;

                                    foreach (string entry in entries)
                                    {
                                        temp = entry + "]]";
                                        m = allHoursRegEx.Match(temp);
                                        if (null != m  && m.Success)
                                        {
                                            if (int.TryParse(m.Groups[1].Value, out dayIndex))
                                            {
                                                hrs = m.Groups[2].Value + "]";
                                                hrs = hrs.Replace(",[", "~[");
                                                hrsEntries = hrs.Split(new char[] { '~' });
                                                List<HoursOfOpertationEntry> hourEntries = new List<HoursOfOpertationEntry>();
                                                foreach (string hrEntry in hrsEntries)
                                                {
                                                    m = hoursRegEx.Match(hrEntry);
                                                    if (null != m && m.Success)
                                                    {
                                                        if (m.Groups.Count >= 3)
                                                        {

                                                            if (TimeSpan.TryParse(m.Groups[1].Value, out openTime))
                                                            {
                                                                if (TimeSpan.TryParse(m.Groups[2].Value, out closeTime))
                                                                {
                                                                    he = hourEntries.FirstOrDefault(h => h.CloseTime == openTime);

                                                                    if (null != he)
                                                                    {
                                                                        hourEntries.Remove(he);
                                                                        he.CloseTime = closeTime;
                                                                    }
                                                                    else
                                                                    {

                                                                        he = new HoursOfOpertationEntry()
                                                                        {
                                                                            OpenTime = openTime,
                                                                            CloseTime = closeTime
                                                                        };
                                                                    }

                                                                    hourEntries.Add(he);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                var dups = 
                                                    _hoursOfOperation.Where(kv => 
                                                        kv.Value.Intersect(hourEntries, new HoursOfOpertationEntryComparer()).Count() > 0);
                                                if (dups != null && dups.Count() > 0)
                                                {
                                                    StringBuilder newDay = new StringBuilder();
                                                    List<string> removalKeys = 
                                                        new List<string>();
                                                    foreach(KeyValuePair<string, List<HoursOfOpertationEntry>> kvp in dups) 
                                                    {
                                                        newDay.Append(kvp.Key);
                                                        newDay.Append(", ");
                                                        removalKeys.Add(kvp.Key);
                                                    }

                                                    foreach(string key in removalKeys)
                                                        _hoursOfOperation.Remove(key);

                                                    newDay.Append(dayNames[dayIndex - 1]);

                                                    string newKey = newDay.ToString();
                                                    newKey = newKey.Replace("Mon - Sat, Sun", "Everyday");
                                                    newKey = newKey.Replace("Mon, Tues, Wed, Thu, Fri, Sat, Sun", "Everyday");
                                                    newKey = newKey.Replace("Mon - Fri, Sat", "Mon - Sat");
                                                    newKey = newKey.Replace("Mon, Tues, Wed, Thu, Fri, Sat", "Mon - Sat");
                                                    newKey = newKey.Replace("Mon - Thu, Fri", "Mon - Fri");
                                                    newKey = newKey.Replace("Mon, Tues, Wed, Thu, Fri", "Mon - Fri");
                                                    newKey = newKey.Replace("Mon - Wed, Thu", "Mon - Thu");
                                                    newKey = newKey.Replace("Mon, Tues, Wed, Thu", "Mon - Thu");
                                                    newKey = newKey.Replace("Mon, Tues, Wed", "Mon - Wed");
                                                    newKey = newKey.Replace("Tues - Sat, Sun", "Tues - Sun");
                                                    newKey = newKey.Replace("Tues, Wed, Thu, Fri, Sat, Sun", "Tues - Sun");
                                                    newKey = newKey.Replace("Tues - Fri, Sat", "Tues - Sat");
                                                    newKey = newKey.Replace("Tues, Wed, Thu, Fri, Sat", "Tues - Sat");
                                                    newKey = newKey.Replace("Tues - Thu, Fri", "Tues - Fri");
                                                    newKey = newKey.Replace("Tues, Wed, Thu, Fri", "Tues - Fri");
                                                    newKey = newKey.Replace("Tues, Wed, Thu", "Tues - Thu");
                                                    newKey = newKey.Replace("Wed - Sat, Sun", "Wed - Sun");
                                                    newKey = newKey.Replace("Wed, Thu, Fri, Sat, Sun", "Wed - Sun");
                                                    newKey = newKey.Replace("Wed - Fri, Sat", "Wed - Sat");
                                                    newKey = newKey.Replace("Wed, Thu, Fri, Sat", "Wed - Sat");
                                                    newKey = newKey.Replace("Wed, Thu, Fri", "Wed - Fri");
                                                    newKey = newKey.Replace("Thu - Sat, Sun", "Thu - Sun");
                                                    newKey = newKey.Replace("Thu, Fri, Sat, Sun", "Thu - Sun");
                                                    newKey = newKey.Replace("Thu, Fri, Sat", "Thu - Sat");

                                                    _hoursOfOperation.Add(newKey, hourEntries);
                                                } 
                                                else 
                                                {
                                                    _hoursOfOperation.Add(dayNames[dayIndex - 1], hourEntries);
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return _hoursOfOperation;
                }
            }

            private string _parkingAggregate;
            public string ParkingAggregate
            {
                get
                {
                    if (string.IsNullOrEmpty(_parkingAggregate))
                    {
                        if (this.parking.HasValue && this.parking.Value)
                        {
                            StringBuilder parkingString = new StringBuilder();
                            if (parking_free.HasValue && parking_free.Value)
                                parkingString.Append("Free");

                            if (parking_garage.HasValue && parking_garage.Value)
                            {
                                if (parkingString.Length > 0)
                                    parkingString.Append(", ");

                                parkingString.Append("Garage");
                            }

                            if (parking_lot.HasValue && parking_lot.Value)
                            {
                                if (parkingString.Length > 0)
                                    parkingString.Append(", ");

                                parkingString.Append("Lot");
                            }

                            if (parking_valet.HasValue && parking_valet.Value)
                            {
                                if (parkingString.Length > 0)
                                    parkingString.Append(", ");

                                parkingString.Append("Valet");
                            }

                            _parkingAggregate = parkingString.ToString();
                        }
                        else
                        {
                            _parkingAggregate = "None";
                        }
                    }

                    return _parkingAggregate;
                }
            }

            public string YelpId { get; set; }
            [DataMember(Name = "24")]
            public int TwentyFour { get; set; }
            [DataMember]
            public bool? open_24hrs { get; set; }
            [DataMember]
            public bool? accessible_wheelchair { get; set; }
            [DataMember]
            public string address { get; set; }
            [DataMember]
            public string address_extended { get; set; }
            [DataMember]
            public bool? alcohol { get; set; }
            [DataMember]
            public bool? alcohol_bar { get; set; }
            [DataMember]
            public bool? alcohol_beer_wine { get; set; }
            [DataMember]
            public bool? alcohol_byob { get; set; }
            [DataMember]
            public string attire { get; set; }
            [DataMember]
            public string attire_required { get; set; }
            [DataMember]
            public string category { get; set; }
            [DataMember]
            public string country { get; set; }
            [DataMember]
            public string cuisine { get; set; }
            [DataMember]
            public string factual_id { get; set; }
            [DataMember]
            public string founded { get; set; }
            [DataMember]
            public bool groups_goodfor { get; set; }
            [DataMember]
            public string hours { get; set; }
            [DataMember]
            public bool kids_goodfor { get; set; }
            [DataMember]
            public double latitude { get; set; }
            [DataMember]
            public string locality { get; set; }
            [DataMember]
            public double longitude { get; set; }
            [DataMember]
            public bool meal_cater { get; set; }
            [DataMember]
            public bool meal_deliver { get; set; }
            [DataMember]
            public bool meal_dinner { get; set; }
            [DataMember]
            public bool meal_lunch { get; set; }
            [DataMember]
            public bool meal_takeout { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public bool options_vegetarian { get; set; }
            [DataMember]
            public string owner { get; set; }
            [DataMember]
            public bool? parking { get; set; }
            [DataMember]
            public bool? parking_lot { get; set; }
            [DataMember]
            public bool payment_cashonly { get; set; }
            [DataMember]
            public string postcode { get; set; }
            [DataMember]
            public int price { get; set; }
            [DataMember]
            public double? rating { get; set; }
            [DataMember]
            public string region { get; set; }
            [DataMember]
            public bool reservations { get; set; }
            [DataMember]
            public bool seating_outdoor { get; set; }
            [DataMember]
            public string status { get; set; }
            [DataMember]
            public string tel { get; set; }
            [DataMember]
            public string website { get; set; }
            [DataMember(Name = "$distance")]
            public double Distance { get; set; }
            [DataMember]
            public bool? smoking { get; set; }
            [DataMember]
            public bool? options_lowfat { get; set; }
            [DataMember]
            public string email { get; set; }
            [DataMember]
            public bool? options_glutenfree { get; set; }
            [DataMember]
            public bool? options_organic { get; set; }
            [DataMember]
            public bool? room_private { get; set; }
            [DataMember]
            public bool? wifi { get; set; }
            [DataMember]
            public bool? meal_breakfast { get; set; }
            [DataMember]
            public bool? parking_garage { get; set; }
            [DataMember]
            public string fax { get; set; }
            [DataMember]
            public bool? options_healthy { get; set; }
            [DataMember]
            public bool? parking_free { get; set; }
            [DataMember]
            public bool? kids_menu { get; set; }
            [DataMember]
            public bool? parking_valet { get; set; }

            // this app specific
            public bool ViewAllPlaceholder = false;
        }

        public class Response
        {
            public List<Restaurant> data { get; set; }
            public int included_rows { get; set; }
        }

        public class RootObject
        {
            public int version { get; set; }
            public string status { get; set; }
            public Response response { get; set; }
        }
    }
}
