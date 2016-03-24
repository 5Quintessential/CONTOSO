using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.Exceptions;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.BingSearch
{
    public class ImageSearchHelper
    {
        #region Properties

        public String AccountId { get; private set; }
        public String ApiKey { get; private set; }

        #endregion Properties

        #region Constructors

        public ImageSearchHelper(string accountId, string apiKey)
        {
            this.AccountId = accountId;
            this.ApiKey = apiKey;
        }

        #endregion Constructors

        #region Public Nested Classes

        //[DataContract]
        //public class Query
        //{
        //    [DataMember]
        //    public string SearchTerms { get; set; }
        //}

        //[DataContract]
        //public class Thumbnail
        //{
        //    [DataMember]
        //    public string Url { get; set; }
        //    [DataMember]
        //    public string ContentType { get; set; }
        //    [DataMember]
        //    public int Width { get; set; }
        //    [DataMember]
        //    public int Height { get; set; }
        //    [DataMember]
        //    public int FileSize { get; set; }
        //}

        //[DataContract]
        //public class Result
        //{
        //    [DataMember]
        //    public string Title { get; set; }
        //    [DataMember]
        //    public string MediaUrl { get; set; }
        //    [DataMember]
        //    public string Url { get; set; }
        //    [DataMember]
        //    public string DisplayUrl { get; set; }
        //    [DataMember]
        //    public int Width { get; set; }
        //    [DataMember]
        //    public int Height { get; set; }
        //    [DataMember]
        //    public int FileSize { get; set; }
        //    [DataMember]
        //    public Thumbnail Thumbnail { get; set; }
        //}

        //[DataContract]
        //public class Image
        //{
        //    [DataMember]
        //    public int Total { get; set; }
        //    [DataMember]
        //    public int Offset { get; set; }
        //    [DataMember]
        //    public List<Result> Results { get; set; }
        //}

        //[DataContract]
        //public class SearchResponse
        //{
        //    [DataMember]
        //    public string Version { get; set; }
        //    [DataMember]
        //    public Query Query { get; set; }
        //    [DataMember]
        //    public Image Image { get; set; }
        //}

        //[DataContract]
        //public class RootObject
        //{
        //    [DataMember]
        //    public SearchResponse SearchResponse { get; set; }
        //}

        public class Metadata
        {
            public string uri { get; set; }
            public string type { get; set; }
        }

        public class Metadata2
        {
            public string type { get; set; }
        }

        public class Thumbnail
        {
            public Metadata2 __metadata { get; set; }
            public string MediaUrl { get; set; }
            public string ContentType { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string FileSize { get; set; }
        }

        public class Result
        {
            public Metadata __metadata { get; set; }
            public string ID { get; set; }
            public string Title { get; set; }
            public string MediaUrl { get; set; }
            public string SourceUrl { get; set; }
            public string DisplayUrl { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string FileSize { get; set; }
            public string ContentType { get; set; }
            public Thumbnail Thumbnail { get; set; }
        }

        public class D
        {
            public List<Result> results { get; set; }
            public string __next { get; set; }
        }

        public class RootObject
        {
            public D d { get; set; }
        }

        #endregion  Public Nested Classes

        public async Task<IEnumerable<Result>> GetImages(Restaurant restaurant, int count)
        {
            // ImageFilters does not support an array of filters yet, once enabled change to ImageFilters='Size:Large;Style:Photo;Face:Other'
            string searchPattern =
                "https://api.datamarket.azure.com/Bing/Search/Image?Query='{0}'&$top={1}&$format=json&ImageFilters='Size:Large'";

            string searchUrl =
                string.Format(searchPattern,
                    Uri.EscapeDataString(
                        string.Format("{0} {1} {2},{3}",
                        restaurant.Name.ToLowerInvariant().Contains("restaurant") ?
                            restaurant.Name :
                            restaurant.Name + " Restaurant",
                        restaurant.Address,
                        restaurant.City,
                        restaurant.State)),
                    count);


            string _JsonString = string.Empty;
            try
            {
                // call service
                var _Request = WebRequest.CreateHttp(searchUrl);
                _Request.Credentials = new NetworkCredential(this.AccountId, this.ApiKey);
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
                        null != _result.d &&
                        null != _result.d.results)
                    {
                        return _result.d.results;
                    }
                }

                return null;
            }
            catch (FoodLibServiceException) { throw; }
            catch (Exception e) { throw new FoodLibServiceException(e) { JSON = _JsonString, Query = new Uri(searchUrl) }; }
        }
    }
}
