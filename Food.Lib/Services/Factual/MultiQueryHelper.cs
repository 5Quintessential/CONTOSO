using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Helpers;
using Microsoft.DPE.ReferenceApps.Common.OAuthLib;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual
{
    public class MultiQueryHelper
    {
        private OAuthManager OAuth;

        public MultiQueryHelper(OAuthManager oAuth)
        {
            this.OAuth = oAuth;
        }

        public MultiQueryHelper(string factualOAuthConsumerKey, string factualOAuthConsumerSecret)
        {
            if (string.IsNullOrWhiteSpace(factualOAuthConsumerKey))
                throw new ArgumentNullException("factualOAuthConsumerKey");
            if (string.IsNullOrWhiteSpace(factualOAuthConsumerSecret))
                throw new ArgumentNullException("factualOAuthConsumerSecret");
            OAuth = new OAuthManager(factualOAuthConsumerKey, factualOAuthConsumerSecret);
        }

        public class QueryInfo
        {
            public string Name { get; set; }
            public string Query { get; set; }
        }

        public async Task<List<string>> SearchAsync(IEnumerable<QueryInfo> queries, int batchSize = 3)
        {
            var _Results = new List<string>();
            var _Queries = queries.Select(x => string.Format("\"{0}\":\"{1}\"", x.Name, x.Query));
            var _Blocks = _Queries
                .Select((value, i) => new { Index = i, Value = value })
                .GroupBy(item => item.Index / batchSize)
                .Select(chunk => chunk.Select(item => item.Value));
            foreach (var _Block in _Blocks)
            {
                var _Batch = string.Join(",", _Block.ToArray());
                var _Url = "http://api.v3.factual.com/multi?queries=" 
                    + System.Uri.EscapeDataString("{" + _Batch + "}");
                var _Result = await FetchAsync(_Url);
                _Results.Add(_Result);
            }
            // TODO: combine these before returning
            return _Results;
        }

        private async Task<string> FetchAsync(string url)
        {
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
                    return _Reader.ReadToEnd();
                }
            }
            catch (InvalidSomethingException) { throw; }
            catch (Exception e) { throw new InvalidSomethingException(e) { Query = url }; }
        }

        public class InvalidSomethingException : Exception
        {
            public InvalidSomethingException(string message) : base(message) { }
            public InvalidSomethingException(Exception e) : base(string.Empty, e) { }
            public string Query { get; set; }
        }


    }
}
