using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.OpenMenu
{
    public class MenuHelper : Microsoft.DPE.ReferenceApps.Food.Lib.Services.OpenMenu.IMenuHelper
    {
        private string Key;
        public MenuHelper(string key) { Key = key; }

        public async Task<IEnumerable<Root>> SearchAsync(IEnumerable<string> openMenuIds)
        {
            var _List = new List<Root>();
            foreach (var item in openMenuIds)
                _List.Add(await SearchAsync(item));
            return _List;
        }

        public async Task<Root> SearchAsync(string openMenuId)
        {
            var _Url = "http://openmenu.com/menu/{1}";
            _Url = string.Format(_Url, Key, openMenuId);
            string _Xml = string.Empty;
            try
            {
                // fetch from rest service
                var _HttpClient = new System.Net.Http.HttpClient();
                var _HttpResponse = await _HttpClient.GetAsync(_Url);
                _Xml = await _HttpResponse.Content.ReadAsStringAsync();

                // handle "There is an error in XML document (2, 1)"
                var _Element = new XmlDocument();
                _Element.LoadXml(_Xml);
                _Xml = _Element.DocumentElement.GetXml();

                // deserialize json to strings
                var _Bytes = Encoding.Unicode.GetBytes(_Xml);
                using (var _Stream = new MemoryStream(_Bytes))
                {
                    // _Reader.posio
                    var _Serializer = new XmlSerializer(typeof(Root));
                    var _Result = (Root)_Serializer.Deserialize(_Stream);
                    return _Result;
                }

            }
            catch (InvalidSomethingException) { throw; }
            catch (Exception e) { throw new InvalidSomethingException(e) { JSON = _Xml, URL = _Url }; }
        }

        public class InvalidSomethingException : Exception
        {
            public InvalidSomethingException() { }
            public InvalidSomethingException(Exception e) : base(string.Empty, e) { }
            public string JSON { get; set; }
            public string URL { get; set; }
        }

        [XmlRoot("omf")]
        [DataContract]
        public partial class Root
        {
            [XmlAttribute("uuid")]
            [DataMember]
            public string OpenmenuId { get; set; }
            [XmlAttribute("created_date")]
            [DataMember]
            public System.DateTime CreatedOn { get; set; }
            [XmlAttribute("accuracy")]
            [DataMember]
            public string Accuracy { get; set; }
            [XmlElement("openmenu")]
            [DataMember]
            public Openmenu Openmenu { get; set; }
            [XmlArray("menus")]
            [DataMember]
            public List<Menu> Menus { get; set; }
            [XmlElement("restaurant_info")]
            [DataMember]
            public Restaurant Restaurant { get; set; }
        }

        [XmlType("openmenu")]
        [DataContract]
        public partial class Openmenu
        {
            [XmlElement("version")]
            [DataMember]
            public string Version { get; set; }
            [XmlArray("crosswalks")]
            [DataMember]
            public List<Crosswalk> Crosswalks { get; set; }
        }

        [XmlType("crosswalk")]
        [DataContract]
        public partial class Crosswalk
        {
            [XmlElement("crosswalk_id")]
            [DataMember]
            public string Id { get; set; }
            [XmlElement("crosswalk_company")]
            [DataMember]
            public string Company { get; set; }
            [XmlElement("crosswalk_url")]
            [DataMember]
            public string Url { get; set; }
        }

        [XmlType("restaurant_info")]
        [DataContract]
        public partial class Restaurant
        {
            [XmlElement("restaurant_name")]
            [DataMember]
            public string Name { get; set; }
            [XmlElement("business_type")]
            [DataMember]
            public string Type { get; set; }
            [XmlElement("brief_description")]
            [DataMember]
            public string DescriptionShort { get; set; }
            [XmlElement("full_description")]
            [DataMember]
            public string DescriptionLong { get; set; }
            [XmlElement("location_id")]
            [DataMember]
            public string LocationId { get; set; }
            [XmlElement("mobile")]
            [DataMember]
            public string Mobile { get; set; }
            [XmlElement("longitude")]
            [DataMember]
            public decimal Longitude { get; set; }
            [XmlElement("latitude")]
            [DataMember]
            public decimal Latitude { get; set; }
            [XmlElement("utc_offset")]
            [DataMember]
            public string TimeZone { get; set; }
            [XmlElement("address_1")]
            [DataMember]
            public string Address1 { get; set; }
            [XmlElement("address_2")]
            [DataMember]
            public string Address2 { get; set; }
            [XmlElement("city_town")]
            [DataMember]
            public string City { get; set; }
            [XmlElement("state_province")]
            [DataMember]
            public string State { get; set; }
            [XmlElement("postal_code")]
            [DataMember]
            public string Zip { get; set; }
            [XmlElement("country")]
            [DataMember]
            public string Country { get; set; }
            [XmlElement("region_area")]
            [DataMember]
            public string Region { get; set; }
            [XmlElement("phone")]
            [DataMember]
            public string Phone { get; set; }
            [XmlElement("fax")]
            [DataMember]
            public string Fax { get; set; }
            [XmlElement("website_url")]
            [DataMember]
            public string Website { get; set; }
            [XmlElement("omf_file_url")]
            [DataMember]
            public string FileUrl { get; set; }
            [XmlElement("logo_urls")]
            [DataMember]
            public string LogoUrl { get; set; }
            [XmlElement("parent_company")]
            [DataMember]
            public Parent Parent { get; set; }
            [XmlElement("environment")]
            [DataMember]
            public Environment Environment { get; set; }
            //[XmlElement]
            //public string contacts { get; set; }
        }

        [XmlType("environment")]
        [DataContract]
        public partial class Environment
        {
            [XmlElement("seating_qty")]
            [DataMember]
            public string SeatingQuantity { get; set; }
            [XmlElement("max_group_size")]
            [DataMember]
            public string MaxGroupSize { get; set; }
            [XmlElement("age_level_preference")]
            [DataMember]
            public string AgeLevelPreference { get; set; }
            [XmlElement("smoking_allowed")]
            [DataMember]
            public string SmokingAllowance { get; set; }
            [XmlElement("takeout_available")]
            [DataMember]
            public string TakeoutAvailable { get; set; }
            [XmlElement("delivery_available")]
            [DataMember]
            public Delivery DeliveryAvailable { get; set; }
            [XmlElement("catering_available")]
            [DataMember]
            public string CateringAvailable { get; set; }
            [XmlElement("reservations")]
            [DataMember]
            public string AllowsReservations { get; set; }
            [XmlElement("alcohol_type")]
            [DataMember]
            public string AlcoholType { get; set; }
            [XmlElement("music_type")]
            [DataMember]
            public string MusicType { get; set; }
            [XmlElement("pets_allowed")]
            [DataMember]
            public string PetsAllowed { get; set; }
            [XmlElement("wheelchair_accessible")]
            [DataMember]
            public string WheelchairAccessible { get; set; }
            [XmlElement("dress_code")]
            [DataMember]
            public string DressCode { get; set; }
            [XmlElement("cuisine_type_primary")]
            [DataMember]
            public string CuisineTypePrimary { get; set; }
            [XmlElement("cuisine_type_secondary")]
            [DataMember]
            public string CuisineTypeSecondary { get; set; }
            [XmlElement("seating_locations")]
            [DataMember]
            public string SeatingLocations { get; set; }
            [XmlElement("accepted_currencies")]
            [DataMember]
            public string AcceptedCurrencies { get; set; }
            [XmlElement("online_reservations")]
            [DataMember]
            public string OnlineReservations { get; set; }
            [XmlElement("online_ordering")]
            [DataMember]
            public string OnlineOrdering { get; set; }
            [XmlElement("parking")]
            [DataMember]
            public string HasParking { get; set; }
            [XmlArray]
            [DataMember]
            public List<Operation> operating_days { get; set; }
        }

        [XmlType("delivery_available")]
        [DataContract]
        public partial class Delivery
        {
            [XmlAttribute("radius")]
            [DataMember]
            public string Radius { get; set; }
            [XmlAttribute("fee")]
            [DataMember]
            public string Fee { get; set; }
        }

        [XmlType("operating_day")]
        [DataContract]
        public partial class Operation
        {
            [XmlElement("day_of_week")]
            [DataMember]
            public byte Weekday { get; set; }
            [XmlElement("open_time")]
            [DataMember]
            public string Open { get; set; }
            [XmlElement("close_time")]
            [DataMember]
            public string Close { get; set; }
        }

        [XmlType("parent_company")]
        [DataContract]
        public partial class Parent
        {
            [XmlElement("parent_company_name")]
            [DataMember]
            public string Name { get; set; }
            [XmlElement("parent_company_website")]
            [DataMember]
            public string Website { get; set; }
            [XmlElement("address_1")]
            [DataMember]
            public string Address1 { get; set; }
            [XmlElement("address_2")]
            [DataMember]
            public string Address2 { get; set; }
            [XmlElement("city_town")]
            [DataMember]
            public string City { get; set; }
            [XmlElement("state_province")]
            [DataMember]
            public string State { get; set; }
            [XmlElement("postal_code")]
            [DataMember]
            public string Zip { get; set; }
            [XmlElement("country")]
            [DataMember]
            public string Country { get; set; }
            [XmlElement("phone")]
            [DataMember]
            public string Phone { get; set; }
            [XmlElement("fax")]
            [DataMember]
            public string Fax { get; set; }
        }

        [XmlType("menu")]
        [DataContract]
        public partial class Menu
        {
            [XmlElement("menu_description")]
            [DataMember]
            public string Description { get; set; }
            [XmlElement("menu_note")]
            [DataMember]
            public string Note { get; set; }
            [XmlElement("menu_duration")]
            [DataMember]
            public Duration Duration { get; set; }
            [XmlArray("menu_groups")]
            [DataMember]
            public List<Group> Groups { get; set; }
            [XmlAttribute("name")]
            [DataMember]
            public string Name { get; set; }
            [XmlAttribute("currency_symbol")]
            [DataMember]
            public string Currency { get; set; }
            [XmlAttribute("language")]
            [DataMember]
            public string Language { get; set; }
            [XmlAttribute("uid")]
            [DataMember]
            public string Id { get; set; }
        }

        [XmlType("menu_duration")]
        [DataContract]
        public partial class Duration
        {
            [XmlElement("menu_duration_name")]
            [DataMember]
            public string Name { get; set; }
            [XmlElement("menu_duration_time_start")]
            [DataMember]
            public string Start { get; set; }
            [XmlElement("menu_duration_time_end")]
            [DataMember]
            public string End { get; set; }
        }

        [XmlType("menu_group")]
        [DataContract]
        public partial class Group
        {
            [XmlElement("menu_group_description")]
            [DataMember]
            public string Description { get; set; }
            [XmlElement("menu_group_note")]
            [DataMember]
            public string Note { get; set; }
            [XmlArray("menu_items")]
            [DataMember]
            public List<Item> Items { get; set; }
            [XmlAttribute("name")]
            [DataMember]
            public string Name { get; set; }
            [XmlAttribute("uid")]
            [DataMember]
            public string Id { get; set; }
        }

        [XmlType("menu_item")]
        [DataContract]
        public partial class Item
        {
            [XmlElement("menu_item_name")]
            [DataMember]
            public string Name { get; set; }
            [XmlElement("menu_item_description")]
            [DataMember]
            public string Description { get; set; }
            [XmlElement("menu_item_price")]
            [DataMember]
            public string Price { get; set; }
            [XmlAttribute("uid")]
            [DataMember]
            public string Id { get; set; }
        }

    }
}
