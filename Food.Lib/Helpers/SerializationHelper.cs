using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Helpers
{
    public sealed class SerializationHelper
    {
        public static T Clone<T>(T original)
        {
            var _JSON = JSON.Serialize(original);
            var _Clone = JSON.Deserialize<T>(_JSON);
            return _Clone;
        }
        
        public static class JSON
        {
            public static T Deserialize<T>(string json)
            {
                var _Bytes = Encoding.Unicode.GetBytes(json);
                using (MemoryStream _Stream = new MemoryStream(_Bytes))
                {
                    var _Serializer = new DataContractJsonSerializer(typeof(T));
                    return (T)_Serializer.ReadObject(_Stream);
                }
            }

            public static string Serialize(object instance)
            {
                using (MemoryStream _Stream = new MemoryStream())
                {
                    var x = instance.GetType();
                    var _Serializer = new DataContractJsonSerializer(instance.GetType());
                    _Serializer.WriteObject(_Stream, instance);
                    _Stream.Position = 0;
                    StreamReader _Reader = new StreamReader(_Stream);
                    return _Reader.ReadToEnd(); 
                }
            }
        }

        public static class XML
        {
            public static T Deserialize<T>(string XML)
            {
                var _Bytes = Encoding.Unicode.GetBytes(XML);
                using (MemoryStream _Stream = new MemoryStream(_Bytes))
                {
                    var _Serializer = new XmlSerializer(typeof(T));
                    return (T)_Serializer.Deserialize(_Stream);
                }
            }

            public static string Serialize(object instance)
            {
                using (MemoryStream _Stream = new MemoryStream())
                {
                    var _Serializer = new XmlSerializer(instance.GetType());
                    _Serializer.Serialize(_Stream, instance);
                    _Stream.Position = 0;
                    StreamReader _Reader = new StreamReader(_Stream);
                    return _Reader.ReadToEnd(); 
                }
            }
        }
    }
}
