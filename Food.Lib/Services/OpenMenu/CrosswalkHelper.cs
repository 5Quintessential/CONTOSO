using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.OpenMenu
{
    public class CrosswalkHelper : Microsoft.DPE.ReferenceApps.Food.Lib.Services.OpenMenu.ICrosswalkHelper
    {
        private string Key;
        public CrosswalkHelper(string key) { Key = key; }

        public class MultiResult
        {
            public string FactualId { get; set; }
            public string OpenMenuId { get; set; }
        }

        public async Task<IEnumerable<MultiResult>> SearchAsync(IEnumerable<string> factualIds)
        {
            var _List = new List<MultiResult>();
            foreach (var item in factualIds)
                _List.Add(new MultiResult { FactualId = item, OpenMenuId = await SearchAsync(item) });
            return _List;
        }

        public async Task<string> SearchAsync(string factualId)
        {
            var _Url = "http://openmenu.com/api/v1/crosswalk?key={0}&crosswalk={1}";
            _Url = string.Format(_Url, Key, factualId);
            string _JsonString = string.Empty;
            try
            {
                //return string.Empty;
                // fetch from rest service
                var _HttpClient = new System.Net.Http.HttpClient();
                var _HttpResponse = await _HttpClient.GetAsync(_Url);

                // not found
                if (_HttpResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return string.Empty;
                _JsonString = await _HttpResponse.Content.ReadAsStringAsync();

                var _XmlDocument = new XmlDocument();
                _XmlDocument.LoadXml(_JsonString);
                _JsonString = _XmlDocument.DocumentElement.GetXml();

                // deserialize json to objects
                var _JsonBytes = Encoding.Unicode.GetBytes(_JsonString);
                using (MemoryStream _MemoryStream = new MemoryStream(_JsonBytes))
                {
                    var _Serializer = new XmlSerializer(typeof(Response));
                    var _Result = (Response)_Serializer.Deserialize(_MemoryStream);
                    var _Api = _Result.results.FirstOrDefault();
                    if (_Api == null)
                        return string.Empty;
                    return _Api.openmenu_id;
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

        [XmlRoot("response")]
        public class Response
        {
            [XmlArray]
            public List<Item> results { get; set; }
        }

        [XmlType("item")]
        public class Item
        {
            [XmlElement]
            public string openmenu_id { get; set; }
        }
    }
}
