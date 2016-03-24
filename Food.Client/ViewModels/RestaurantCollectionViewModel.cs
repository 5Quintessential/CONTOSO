using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Common;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual;

namespace Microsoft.DPE.ReferenceApps.Food.Client.ViewModels
{
    [DataContract]
    public class RestaurantCollectionViewModel : BindableBase
    {
        [IgnoreDataMember]
        public FilterFlyoutViewModel CurrentFilter
        {
            get { return FilterFlyoutViewModel.Current; }
        }

        private List<Restaurant> _sourceCollection = null;

        private ObservableCollection<Restaurant> _filteredCollection = null;
        [DataMember]
        public ObservableCollection<Restaurant> FilteredCollection
        {
            get { return _filteredCollection; }
            private set { this.SetProperty<ObservableCollection<Restaurant>>(ref this._filteredCollection, value); }
        }

        private bool _isFiltered = false;
        [DataMember]
        public bool IsFiltered
        {
            get { return this._isFiltered; }
            set { this.SetProperty<bool>(ref this._isFiltered, value); }
        }

        public RestaurantCollectionViewModel(IEnumerable<Restaurant> coll, bool sortIt)
        {
            if (null != coll)
                LoadDataOnNavigate(coll, sortIt);
            ApplyOptions();
        }

        public RestaurantCollectionViewModel(IEnumerable<Restaurant> coll)
            : this(coll, true)
        {
            ApplyOptions();
        }

        private async void LoadDataOnNavigate(IEnumerable<Restaurant> coll, bool sortIt)
        {
            if (sortIt)
            {
                _sourceCollection = coll.OrderBy(r => r.Distance).ToList();
            }
            else
            {
                _sourceCollection = coll.ToList();
            }

            SetProperty(ref _filteredCollection, new ObservableCollection<Restaurant>(_sourceCollection), "FilteredCollection");

            try
            {
                await Unity.FillFactualAsync(_sourceCollection);
            }
            catch (Exception Ex)
            {
                //do nothing - this is a bug in the Factual Crosswalk API from their side
            }

            //get unique set of cuisines for this collection
            CurrentFilter.CuisineList = _sourceCollection.Where(r => r.FactualRecord != null && !string.IsNullOrWhiteSpace(r.FactualRecord.cuisine))
              .SelectMany(r => r.FactualRecord.cuisine.Split(',').Select(s => s.Trim())).Distinct().ToList();

            return;
        }


        public void ApplyOptions()
        {
            bool NoFilter = ((PriceRanges)CurrentFilter.PriceRange == PriceRanges.Unknown)
                && (CurrentFilter.Cuisine == 0)
                && (CurrentFilter.ReservationRequired == false)
                && (CurrentFilter.Vegetarian == false)
                && (CurrentFilter.Parking == false)
                && (CurrentFilter.Alcohol == false)
                && (CurrentFilter.KidFriendly == false)
                && (CurrentFilter.Takeout == false)
                && (CurrentFilter.Delivery == false);

            this.IsFiltered = !NoFilter;

            var result =
              _sourceCollection
              .Where(r =>
                {
                    bool ret = (NoFilter ? true : r.FactualRecord != null)
                    && ((PriceRanges)CurrentFilter.PriceRange == PriceRanges.Unknown ? true : r.PriceRange == (PriceRanges)CurrentFilter.PriceRange)
                    && (CurrentFilter.Cuisine == 0 || null == r.FactualRecord.cuisine ? true : r.FactualRecord.cuisine.ToLowerInvariant().Contains((CurrentFilter.CuisineList[CurrentFilter.Cuisine]).ToLowerInvariant()))
                    && (CurrentFilter.ReservationRequired == false ? true : r.FactualRecord.reservations == CurrentFilter.ReservationRequired)
                    && (CurrentFilter.Vegetarian == false ? true : r.FactualRecord.options_vegetarian == CurrentFilter.Vegetarian)
                    && (CurrentFilter.Parking == false ? true : (r.FactualRecord.parking.HasValue && r.FactualRecord.parking == CurrentFilter.Parking))
                    && (CurrentFilter.Alcohol == false ? true : r.FactualRecord.alcohol == CurrentFilter.Alcohol)
                    && (CurrentFilter.KidFriendly == false ? true : r.FactualRecord.kids_goodfor == CurrentFilter.KidFriendly)
                    && (CurrentFilter.Takeout == false ? true : r.FactualRecord.meal_takeout == CurrentFilter.Takeout)
                    && (CurrentFilter.Delivery == false ? true : r.FactualRecord.meal_deliver == CurrentFilter.Delivery);
                    return ret;
                }).ToList();

            switch ((FilterFlyoutViewModel.OrderByOptions)CurrentFilter.OrderBy)
            {
                case FilterFlyoutViewModel.OrderByOptions.Distance:
                    result = result.OrderBy(r => r.Distance).ToList();
                    break;
                case FilterFlyoutViewModel.OrderByOptions.Rating:
                    result = result.OrderByDescending(r => r.AverageRating).ToList();
                    break;
                case FilterFlyoutViewModel.OrderByOptions.Restaurant_Name:
                    result = result.OrderBy(r => r.Name).ToList();
                    break;
                default:
                    result = result.OrderBy(r => r.Distance).ToList();
                    break;
            }

            this.FilteredCollection = new ObservableCollection<Restaurant>(result);
        }

        public async void RemoveFavorites(IEnumerable<Restaurant> favoritesToRemove)
        {
            var lst = favoritesToRemove.ToList();
            lst.All(r => { _sourceCollection.Remove(r); return true; });
            ApplyOptions();
            await AppData.Current.RemoveFavoritesAsync(lst);
        }

        public void ClearFavorites()
        {
            _sourceCollection.Clear();
            ApplyOptions();
            AppData.Current.ClearFavorites();
        }
    }
}
