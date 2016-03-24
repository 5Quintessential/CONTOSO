using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.ObjectModel;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services;
using Microsoft.DPE.ReferenceApps.Food.Lib.Helpers;
using System.Runtime.Serialization;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Models
{
    [DataContract]
    public class Restaurant : BaseModel, IVariableGridItem
    {
        public ObservableCollection<Deal> Deals { get; private set; }
        public ObservableCollection<Review> Reviews { get; private set; }
        public ObservableCollection<String> Categories { get; private set; }

        public Restaurant() { }

        public Restaurant(Food.Lib.Services.Yelp.V1Helper.Business yelpRecord)
        {
            Deals = new ObservableCollection<Deal>();
            Reviews = new ObservableCollection<Review>();
            Categories = new ObservableCollection<string>();
            FactualRecordLoaded = OpenmenuRecordLoaded = YelpRecordLoaded = false;
            this.YelpRecord = yelpRecord;
            ColSpan = RowSpan = 1;
        }

        public Restaurant ToClone(int index)
        {
            var _Restaurant = new Restaurant(this.YelpRecord) { Index = index + 1 };
            if (this.FactualRecordLoaded)
                _Restaurant.FactualRecord = this.FactualRecord;
            if (this.YelpRecordLoaded)
                _Restaurant.YelpRecord = this.YelpRecord;
            if (this.OpenmenuRecordLoaded)
                _Restaurant.OpenmenuRecord = this.OpenmenuRecord;
            if (this.YelpBusinessRecordLoaded)
                _Restaurant.YelpBusinessRecord = this.YelpBusinessRecord;
            return _Restaurant;
        }

        public double Distance { 
            get { return Math.Round(YelpRecord.distance, 2); } }


        public int ReviewCount { get { return Reviews.Count(); } }
        public PriceRanges? PriceRange
        {
            get
            {
                if (FactualRecord == null)
                    return null;
                return (PriceRanges)FactualRecord.price;
            }
        }

        private List<Lib.Models.Cuisines> _cuisines;
        public List<Lib.Models.Cuisines> Cuisines
        {
            get
            {
                if (null == this._cuisines && 
                    null != this.FactualRecord && 
                    !string.IsNullOrWhiteSpace(this.FactualRecord.cuisine))
                {
                    string[] vals = this.FactualRecord.cuisine.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    Cuisines cuisine;
                    if (null != vals && 1 < vals.Length)
                    {
                        this._cuisines = new List<Cuisines>();

                        foreach (string val in vals)
                            if (Enum.TryParse<Cuisines>(val.Trim().Replace(" ", "_"), out cuisine))
                                this._cuisines.Add(cuisine);
                    }
                }

                return _cuisines;
            }
        }

        public string Key { get { return YelpRecord.id; } }
        public string Name { get { return YelpRecord.name; } }
        public string Address { get { return YelpRecord.address1; } }
        public string City { get { return YelpRecord.city; } }
        public string State { get { return YelpRecord.state; } }
        public string Zip { get { return YelpRecord.zip; } }
        public string Phone { get { return YelpRecord.phone; } }
        public double? AverageRating { get { return YelpRecord.avg_rating; } }
        public double Latitude { get { return YelpRecord.latitude; } }
        public double Longitude { get { return YelpRecord.longitude; } }
        public string ImagePath { get { return (YelpRecord == null) ? string.Empty : YelpRecord.photo_url; } }
        public bool HasLunchDeal { get { return false; } }
        public DateTime NewestReviewDate { get { return (!Reviews.Any()) ? DateTime.MinValue : Reviews.Max(x => x.Date); } }
        public List<Services.OpenMenu.MenuHelper.Menu> Menus { get { return (null == OpenmenuRecord || null == OpenmenuRecord.Menus || 1 > OpenmenuRecord.Menus.Count) ? null : OpenmenuRecord.Menus; } }
        public string ReasonFound { get; set; }

        public bool FactualRecordLoaded { get; set; }
        private Food.Lib.Services.Factual.SearchHelper.Restaurant m_FactualRecord = null;
        [DataMember]
        public Food.Lib.Services.Factual.SearchHelper.Restaurant FactualRecord
        {
            get { return m_FactualRecord; }
            set
            {
                FactualRecordLoaded = true;
                m_FactualRecord = value;
                RaisePropertyChanged(this);
            }
        }

        public bool OpenmenuRecordLoaded { get; set; }
        private Food.Lib.Services.OpenMenu.MenuHelper.Root m_OpenmenuRecord = null;
        [DataMember]
        public Food.Lib.Services.OpenMenu.MenuHelper.Root OpenmenuRecord
        {
            get { return m_OpenmenuRecord; }
            set
            {
                OpenmenuRecordLoaded = true;
                m_OpenmenuRecord = value;
                RaisePropertyChanged(this);
            }
        }

        public bool YelpBusinessRecordLoaded { get; set; }
        private Food.Lib.Services.Yelp.BusinessHelper.Business m_YelpBusinessRecord = null;
        [DataMember]
        public Food.Lib.Services.Yelp.BusinessHelper.Business YelpBusinessRecord
        {
            get { return m_YelpBusinessRecord; }
            set
            {
                YelpBusinessRecordLoaded = true;
                m_YelpBusinessRecord = value;
                if (value == null)
                    return;

                // when deserialized, constructor does not instantiate the list
                Deals = Deals ?? new ObservableCollection<Deal>();
                Deals.Clear();
                if (value.deals != null)
                    foreach (var item in value.deals)
                        Deals.Add(new Deal(item));

                RaisePropertyChanged(this);
            }
        }

        public bool YelpRecordLoaded { get; set; }
        private Food.Lib.Services.Yelp.V1Helper.Business m_YelpRecord = null;
        [DataMember]
        public Food.Lib.Services.Yelp.V1Helper.Business YelpRecord
        {
            get { return m_YelpRecord; }
            set
            {
                YelpRecordLoaded = true;
                m_YelpRecord = value;
                if (value == null)
                    return;

                // when deserialized, constructor does not instantiate the list
                Reviews = Reviews ?? new ObservableCollection<Review>();
                    Reviews.Clear();
                if (value.reviews != null)
                    foreach (var item in value.reviews)
                        Reviews.Add(new Review(item));

                // when deserialized, constructor does not instantiate the list
                Categories = Categories ?? new ObservableCollection<string>();
                Categories.Clear();
                if (value.categories != null)
                    foreach (var item in value.categories.Select(x => x.name).Distinct())
                        Categories.Add(item);

                RaisePropertyChanged(this);
            }
        }

        // used by XAML.VariableGridView
        public int ColSpan { get; set; }
        public int RowSpan { get; set; }
        public int Index { get; set; }
    }

    public class RestauarantEqualityComparer : IEqualityComparer<Restaurant>
    {
        public bool Equals(Restaurant x, Restaurant y)
        {
            return x.Key.Equals(y.Key);
        }

        public int GetHashCode(Restaurant obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}
