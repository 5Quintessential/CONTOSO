using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Models
{
    public class Filter: Common.BindableBase
    {
        public Filter()
        {
            // defaults
            Search = string.Empty;
            MinRating = 0d;
            MaxResults = 10;
            RadiusInMiles = 25;
            Cuisine = null;
        }

        public string SortBy { get; set; }
        public string Category { get; set; }
        public string GroupBy { get; set; }
        public string PriceRange { get; set; }
        public string WorkingHours { get; set; }
        public bool ReservationsRequired { get; set; }
        public bool TakeOut { get; set; }
        public bool Alcohol { get; set; }
        public bool Vegetarian { get; set; }
        public bool CreditCard { get; set; }
        public bool Parking { get; set; }
        public bool Singles { get; set; }
        public bool KidFriendly { get; set; }
        public bool LunchDeals { get; set; }

        internal int ToHash()
        {
            var _String = Food.Lib.Helpers.SerializationHelper.JSON.Serialize(this);
            return _String.GetHashCode();
        }

        public Food.Lib.Models.Cuisines[] Cuisine { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Search { get; set; }
        public double MinRating { get; set; }
        public int RadiusInMiles { get; set; }
        public int MaxResults { get; set; }
    }
}
