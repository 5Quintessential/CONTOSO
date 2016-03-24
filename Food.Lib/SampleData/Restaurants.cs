using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.SampleData
{
    public static class Restaurants
    {
        private static IEnumerable<Restaurant> _restaurants;
        public static async Task<IEnumerable<Restaurant>> LoadAsync()
        {
            if (null == _restaurants)
            {
                var _File = await Package.Current.InstalledLocation.GetFileAsync(@"Food.Lib\SampleData\Restaurants.json");
                var _Text = await FileIO.ReadTextAsync(_File);
                using (MemoryStream _Stream = new MemoryStream(Encoding.Unicode.GetBytes(_Text)))
                {
                    var _Serializer = new DataContractJsonSerializer(typeof(List<Restaurant>));
                    _restaurants = (List<Restaurant>)_Serializer.ReadObject(_Stream);
                }
            }
            return _restaurants;
        }

        private static async Task<IEnumerable<Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual.SearchHelper.Restaurant>> FactualRecords()
        {
            return (await Restaurants.LoadAsync()).Select(x => x.FactualRecord);
        }

        private static async Task<IEnumerable<Microsoft.DPE.ReferenceApps.Food.Lib.Services.OpenMenu.MenuHelper.Root>> OpenmenuRecords()
        {
            return (await Restaurants.LoadAsync()).Select(x => x.OpenmenuRecord);
        }

        public static async Task<IEnumerable<Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp.V1Helper.Business>> YelpRecords()
        {
            return (await Restaurants.LoadAsync()).Select(x => x.YelpRecord);
        }

    }
}
