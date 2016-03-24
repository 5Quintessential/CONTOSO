using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Common;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Client.ViewModels
{
    [DataContract]
    public class FilterFlyoutViewModel : BindableBase
    {
        public enum OrderByOptions
        {
            Distance, Rating, Restaurant_Name
        }
        public enum GroupByOptions
        {
            None, Cuisine, Price_Range, Rating
        }

        private static FilterFlyoutViewModel _current;
        [IgnoreDataMember]
        public static FilterFlyoutViewModel Current
        {
            get { if (_current == null) _current = new FilterFlyoutViewModel(); return _current; }
        }

        public static void ResetCurrent()
        {
            _current = new FilterFlyoutViewModel();
        }

        [DataMember]
        public static IEnumerable<string> GroupByList
        {
            get
            {
                return Enum.GetNames(typeof(FilterFlyoutViewModel.GroupByOptions)).Select(s => s.Replace('_', ' '));
            }
        }

        [DataMember]
        public static IEnumerable<string> OrderByList
        {
            get
            {
                return Enum.GetNames(typeof(FilterFlyoutViewModel.OrderByOptions)).Select(s => s.Replace('_', ' '));
            }
        }

        private List<string> _CuisineList = null;
        [DataMember]
        public List<string> CuisineList
        {
            get
            {
                if (null != _CuisineList)
                    return new List<string>() { "any" }.Concat(_CuisineList).ToList();
                else
                    return new List<string>() { "any" };
            }
            set
            {
                _CuisineList = value;
            }
        }

        [DataMember]
        public static IEnumerable<string> PriceRangeList
        {
            get
            {
                return new string[] { "Any", "Under $15", "$15 - $30", "$30 - $50", "$50 - $75", "$75 and higher" };
            }
        }

        private OrderByOptions _OrderBy = OrderByOptions.Distance;

        [DataMember]
        public int OrderBy
        {
            get
            {
                return (int)_OrderBy;
            }
            set
            {
                SetProperty(ref _OrderBy, (OrderByOptions)value);
            }
        }

        private GroupByOptions _GroupBy = GroupByOptions.None;

        [DataMember]
        public int GroupBy
        {
            get
            {
                return (int)_GroupBy;
            }
            set
            {
                SetProperty(ref _GroupBy, (GroupByOptions)value);
            }
        }

        private string _Cuisine = null;

        [DataMember]
        public int Cuisine // 1 based getter because of the "Any" literal added to the list
        {
            get
            {
                return _Cuisine == null ? 0 : _CuisineList.IndexOf(_Cuisine) + 1;
            }
            set
            {
                SetProperty(ref _Cuisine, value == 0 ? null : _CuisineList[(value - 1)]);
            }
        }

        private PriceRanges _PriceRange = PriceRanges.Unknown;

        [DataMember]
        public int PriceRange
        {
            get
            {
                return (int)_PriceRange;
            }
            set
            {
                SetProperty(ref _PriceRange, (PriceRanges)value);
            }
        }

        private bool _ReservationRequired = false;

        [DataMember]
        public bool ReservationRequired
        {
            get { return _ReservationRequired; }
            set { SetProperty(ref _ReservationRequired, value); }
        }

        private bool _Takeout = false;

        [DataMember]
        public bool Takeout
        {
            get { return _Takeout; }
            set { SetProperty(ref _Takeout, value); }
        }

        private bool _Delivery = false;

        [DataMember]
        public bool Delivery
        {
            get { return _Delivery; }
            set { SetProperty(ref _Delivery, value); }
        }

        private bool _Alcohol = false;

        [DataMember]
        public bool Alcohol
        {
            get { return _Alcohol; }
            set { SetProperty(ref _Alcohol, value); }
        }

        private bool _Vegetarian = false;

        [DataMember]
        public bool Vegetarian
        {
            get { return _Vegetarian; }
            set { SetProperty(ref _Vegetarian, value); }
        }

        private bool _Parking = false;

        [DataMember]
        public bool Parking
        {
            get { return _Parking; }
            set { SetProperty(ref _Parking, value); }
        }

        private bool _KidFriendly = false;

        [DataMember]
        public bool KidFriendly
        {
            get { return _KidFriendly; }
            set { SetProperty(ref _KidFriendly, value); }
        }

        private bool _Dirty = false;

        [DataMember]
        public bool Dirty
        {
            get { return _Dirty; }
            set { _Dirty = value; }
        }

        private bool _showOrderBy = true;
        [DataMember]
        public bool ShowOrderBy
        {
            get { return this._showOrderBy; }
            set { this.SetProperty<bool>(ref this._showOrderBy, value); }
        }

        public FilterFlyoutViewModel()
        {
            this.PropertyChanged += (s, e) =>
              {
                  this.Dirty = true;
              };
        }
    }



}
