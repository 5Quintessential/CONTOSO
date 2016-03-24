using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp
{
    public class MetadataHelper 
    {
        public class RarseResult
        {
            public Uri Uri { get; set; }
            public string Result { get; set; }
        }

        public async Task<IEnumerable<RarseResult>> ParseAsync(IEnumerable<Uri> uri)
        {
            var _List = new List<RarseResult>();
            foreach (var _Uri in uri)
            {
                var _Result = await ParseAsync(_Uri);
                _List.Add(new RarseResult { Uri = _Uri, Result = _Result });
            }
            return _List;
        }

        public async Task<string> ParseAsync(Uri uri)
        {
            string _Pattern = "(<meta property=\"og:url\" content=\"http://www.yelp.com/biz/)(.{22,22})(\">)";
            string _Result = string.Empty;
            try
            {
                // fetch from rest service
                var _HttpClient = new System.Net.Http.HttpClient();
                var _HttpResponse = await _HttpClient.GetAsync(uri.ToString());
                _Result = await _HttpResponse.Content.ReadAsStringAsync();

                // check for error
                if (!_Result.Contains("<meta property=\"og:url\""))
                    throw new InvalidSomethingException("Missing og:url in result") { HTML = _Result, URL = uri };

                // regex
                var _Matches = Regex.Matches(_Result, _Pattern);
                if (_Matches == null || _Matches.Count != 1 || _Matches[0].Groups.Count != 4)
                    throw new InvalidSomethingException("Expected Matches not found") { HTML = _Result, URL = uri };
                return _Matches[0].Groups[2].Value;
            }
            catch (InvalidSomethingException) { throw; }
            catch (Exception e) { throw new InvalidSomethingException(e) { HTML = _Result, URL = uri }; }
        }

        public class InvalidSomethingException : Exception
        {
            public InvalidSomethingException(string message) : base(message) { }
            public InvalidSomethingException(Exception e) : base(string.Empty, e) { }
            public string HTML { get; set; }
            public Uri URL { get; set; }
        }
    }
}
