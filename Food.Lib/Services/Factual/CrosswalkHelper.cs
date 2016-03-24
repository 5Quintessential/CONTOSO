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

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual
{
    /// <summary>Get Third Party ID from Factual ID</summary>
    /// <see cref="http://developer.factual.com/display/docs/Places+API+-+Crosswalk">Developer Documentation</see>

    public enum FactualCrosswalkNamespaces { Yelp, Foursquare }

    public class CrosswalkHelper : Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual.ICrosswalkHelper
    {
        private const string FACTUAL_URL_ROOT = "http://api.v3.factual.com";

        private OAuthManager OAuth;

        public CrosswalkHelper(OAuthManager oAuth)
        {
            this.OAuth = oAuth;
        }

        public CrosswalkHelper(string factualOAuthConsumerKey, string factualOAuthConsumerSecret)
        {
            if (string.IsNullOrWhiteSpace(factualOAuthConsumerKey))
                throw new ArgumentNullException("factualOAuthConsumerKey");
            if (string.IsNullOrWhiteSpace(factualOAuthConsumerSecret))
                throw new ArgumentNullException("factualOAuthConsumerSecret");
            OAuth = new OAuthManager(factualOAuthConsumerKey, factualOAuthConsumerSecret);
        }

        /// <summary>
        /// Given a namespace (Yelp or Factual) and an IEnumerable of factual ids, returns matching 3rd party ids
        /// </summary>
        /// <param name="namespaceIds">The 3rd party ids for the restaurants</param>
        /// <param name="crosswalkNamespace">The namespace (Yelp or Factual) the ids are coming from</param>
        /// <returns>IEnumerable of Result objects containing 3rd party ids or null if the id cannot be found</returns>
        public async Task<IEnumerable<Result>> SearchOtherAsync(FactualCrosswalkNamespaces crosswalkNamespace, IEnumerable<string> factualIds)
        {
            var _Results = new List<Result>();
            foreach (var item in factualIds.AsParallel())
                _Results.Add(await SearchOtherAsync(item, crosswalkNamespace));
            return _Results;

            //use MULTI
            //var _Batch = new MultiQueryHelper(OAuth);
            //var _List = factualIds.Select(x => new MultiQueryHelper.QueryInfo { Name = x, Query = Querystring(x) });
            //var _Results = await _Batch.ProcessMultiAsync(_List);
            //return null;
            // now what?
        }

        /// <summary>
        /// Given a namespace (Yelp or Factual) and an IEnumerable of 3rd party restaurant ids, returns matching Factual Ids
        /// </summary>
        /// <param name="crosswalkNamespace">The namespace (Yelp or Factual) the ids are coming from</param>
        /// <param name="namespaceIds">The 3rd party ids for the restaurants</param>
        /// <returns>IEnumerable of Result objects containing Factual ids or null if the id cannot be found</returns>
        public async Task<IEnumerable<Result>> SearchFactualAsync(IEnumerable<string> namespaceIds, FactualCrosswalkNamespaces crosswalkNamespace)
        {
            var _Results = new List<Result>();
            foreach (var item in namespaceIds.AsParallel())
                _Results.Add(await SearchFactualAsync(item, crosswalkNamespace));
            return _Results;

            //use MULTI
            //var _Batch = new MultiQueryHelper(OAuth);
            //var _List = factualIds.Select(x => new MultiQueryHelper.QueryInfo { Name = x, Query = Querystring(x) });
            //var _Results = await _Batch.ProcessMultiAsync(_List);
            //return null;
            // now what?
        }

        /// <summary>
        /// Given a factual id and a namespace (Yelp or Foursquare), returns the macthing 3rd party id
        /// </summary>
        /// <param name="namespaceId">The factual id for the restaurant</param>
        /// <param name="crosswalkNamespace">The namespace (Yelp or Factual) for the id we want to match</param>
        /// <returns>Result object containing 3rd party id or null if the id cannot be found</returns>
        public async Task<Result> SearchOtherAsync(string factualId, FactualCrosswalkNamespaces crosswalkNamespace)
        {
            var _Url = BuildSearchOtherQuery(factualId, crosswalkNamespace);
            var _Result = await FetchAsync(_Url);
            if (_Result == null)
                return null;
            return _Result;
        }

        /// <summary>
        /// Given a namespace (Yelp or Factual) and the 3rd party's restaurant id, returns a Factual Id
        /// </summary>
        /// <param name="crosswalkNamespace">The namespace (Yelp or Factual) the id is coming from</param>
        /// <param name="namespaceId">The 3rd party's id for the restaurant</param>
        /// <returns>Result object containing Factual id or null if the id cannot be found</returns>
        public async Task<Result> SearchFactualAsync(string namespaceId, FactualCrosswalkNamespaces crosswalkNamespace)
        {
            string _Url;
            _Url = BuildSearchFactualQuery(namespaceId, crosswalkNamespace);

            var _Result = await FetchAsync(_Url);
            if (_Result == null)
                return null;
            return _Result;
        }

        private string BuildSearchOtherQuery(string factualId, FactualCrosswalkNamespaces crosswalkNamespace)
        {
            // OLD STYLE: "{0}/places/crosswalk?only={1}&factual_id={2}"
            StringBuilder query = new StringBuilder();
            query.Append("http://api.v3.factual.com/t/crosswalk?filters=");

            StringBuilder filterString = new StringBuilder();
            filterString.Append("{");
            filterString.AppendFormat("\"namespace\":\"{0}\",\"factual_id\":\"{1}\"",
                crosswalkNamespace.ToString().ToLowerInvariant(),
                System.Uri.EscapeDataString(factualId));
            filterString.Append("}");

            query.Append(filterString.ToString());
            return query.ToString();
        }

        private string BuildSearchFactualQuery(string namespaceId, FactualCrosswalkNamespaces crosswalkNamespace)
        {
            // OLD STYLE: "{0}/places/crosswalk?namespace={1}&only={1}&namespace_id={2}"
            StringBuilder query = new StringBuilder();
            query.Append("http://api.v3.factual.com/t/crosswalk?filters=");

            StringBuilder filterString = new StringBuilder();
            filterString.Append("{");
            filterString.AppendFormat("\"namespace\":\"{0}\",\"namespace_id\":\"{1}\"",
                crosswalkNamespace.ToString().ToLowerInvariant(),
                System.Uri.EscapeDataString(namespaceId));
            filterString.Append("}");

            query.Append(filterString.ToString());
            return query.ToString();
        }

        private string BuildYelpIdToFactualIdQuery(string yelpId)
        {
            StringBuilder query = new StringBuilder();
            query.Append("http://api.v3.factual.com/t/crosswalk?filters=");

            StringBuilder filterString = new StringBuilder();
            filterString.Append("{\"url\":{\"$eq\":\"");
            filterString.AppendFormat("http://www.yelp.com/biz/{0}",
                yelpId);
            filterString.Append("\"}}");

            query.Append(Uri.EscapeDataString(filterString.ToString()));
            return query.ToString();
        }

        private async Task<Result> FetchAsync(string url)
        {
            string _JsonString = string.Empty;
            try
            {
                // call service
                var _Request = WebRequest.CreateHttp(url);
                _Request.Method = "GET";
                _Request.Headers["Authorization"] = OAuth.GenerateCredsHeader(url, "GET", "");

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
                    return _Result.response.data.OrderByDescending(x => x.Key).FirstOrDefault();
                }
            }
            catch (InvalidSomethingException) { throw; }
            catch (Exception e) { throw new InvalidSomethingException(e) { JSON = _JsonString, Query = url }; }
        }

        public class InvalidSomethingException : Exception
        {
            public InvalidSomethingException(string message) : base(message) { }
            public InvalidSomethingException(Exception e) : base(string.Empty, e) { }
            public string JSON { get; set; }
            public string Query { get; set; }
        }

        [DataContract(Name = "Datum")]
        public class Result
        {
            [DataMember(Name = "factual_id")]
            public string FactualId { get; set; }
            [DataMember(Name = "@namespace")]
            public string Namespace { get; set; }
            [DataMember(Name = "namespace_id")]
            public string Key { get; set; }
            [DataMember(Name = "url")]
            public string Url { get; set; }
        }

        public class Response
        {
            public List<Result> data { get; set; }
            public int included_rows { get; set; }
            public int total_row_count { get; set; }
        }

        public class RootObject
        {
            public int version { get; set; }
            public string status { get; set; }
            public Response response { get; set; }
        }
    }
}
