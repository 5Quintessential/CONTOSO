using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Client.Common;
using Windows.UI.Xaml;

namespace Microsoft.DPE.ReferenceApps.Food.Client.ViewModels
{
    [DataContract]
    public class RestaurantListViewModel : Lib.Common.BindableBase
    {
        public enum RestaurantListTypes { Collection, SearchResults, Favorites }
        public enum RestaurantListGroupTypes { Cuisines, PriceRanges }

        #region Bindable Properties

        private RestaurantListTypes _listType = RestaurantListTypes.Collection;

        [IgnoreDataMember]
        public RestaurantListTypes ListType
        {
            get { return this._listType; }
            set 
            { 
                this.SetProperty<RestaurantListTypes>(ref this._listType, value); 

                if(RestaurantListTypes.Favorites == this._listType)
                    this.FavoritesVisibility = Windows.UI.Xaml.Visibility.Visible;
                else
                    this.FavoritesVisibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private string _listName = string.Empty;
        [DataMember]
        public string ListName
        {
            get { return this._listName; }
            set { this.SetProperty<string>(ref this._listName, value); }
        }

        private string _searchText = string.Empty;
        [DataMember]
        public string SearchText
        {
            get { return this._searchText; }
            set { this.SetProperty<string>(ref this._searchText, value); }
        }

        private string _subTitle = string.Empty;
        [DataMember]
        public string SubTitle
        {
            get { return this._subTitle; }
            set { this.SetProperty<string>(ref this._subTitle, value); }
        }

        private bool _restauarantsLoaded = false;
        [IgnoreDataMember]
        public bool RestauarantsLoaded
        {
            get { return this._restauarantsLoaded; }
            set { this.SetProperty<bool>(ref this._restauarantsLoaded, value); }
        }

        private bool _groupingEnabled = false;
        [IgnoreDataMember]
        public bool GroupingEnabled
        {
            get { return this._groupingEnabled; }
            set { this.SetProperty<bool>(ref this._groupingEnabled, value); }
        }

        private ObservableCollection<Lib.Models.Restaurant> _restaurantList;
        [IgnoreDataMember]
        public ObservableCollection<Lib.Models.Restaurant> RestaurantList
        {
            get { if (null == this._restaurantList) this._restaurantList = new ObservableCollection<Lib.Models.Restaurant>(); return this._restaurantList; }
            set { this.SetProperty<ObservableCollection<Lib.Models.Restaurant>>(ref this._restaurantList, value); CreateGroupings(); }
        }

        private ObservableCollection<Lib.Models.Restaurant> _groupedRestaurantList;
        [IgnoreDataMember]
        public ObservableCollection<Lib.Models.Restaurant> GroupedRestaurantList
        {
            get { return this._groupedRestaurantList; }
            set { this.SetProperty<ObservableCollection<Lib.Models.Restaurant>>(ref this._groupedRestaurantList, value); }
        }

        private Dictionary<Lib.Models.Cuisines, List<Lib.Models.Restaurant>> _restaurantsGroupedByCuisine;
        [IgnoreDataMember]
        public Dictionary<Lib.Models.Cuisines, List<Lib.Models.Restaurant>> RestaurantsGroupedByCuisine
        {
            get { return this._restaurantsGroupedByCuisine; }
            set { this.SetProperty<Dictionary<Lib.Models.Cuisines, List<Lib.Models.Restaurant>>>(ref this._restaurantsGroupedByCuisine, value); }
        }

        private Dictionary<Lib.Models.PriceRanges, List<Lib.Models.Restaurant>> _restaurantsGroupedByPriceRange;
        [IgnoreDataMember]
        public Dictionary<Lib.Models.PriceRanges, List<Lib.Models.Restaurant>> RestaurantsGroupedByPriceRange
        {
            get { return this._restaurantsGroupedByPriceRange; }
            set { this.SetProperty<Dictionary<Lib.Models.PriceRanges, List<Lib.Models.Restaurant>>>(ref this._restaurantsGroupedByPriceRange, value); }
        }

        private RestaurantCollectionViewModel _collectionViewModel;
        [IgnoreDataMember]
        public RestaurantCollectionViewModel CollectionViewModel
        {
            get { return this._collectionViewModel; }
            set { this.SetProperty<RestaurantCollectionViewModel>(ref this._collectionViewModel, value); this._collectionViewModel.PropertyChanged += CollectionViewModel_PropertyChanged; }
        }

        private Windows.UI.Xaml.Visibility _favoritesVisibility = Windows.UI.Xaml.Visibility.Collapsed;
        [IgnoreDataMember]
        public Windows.UI.Xaml.Visibility FavoritesVisibility
        {
            get { return this._favoritesVisibility; }
            set { this.SetProperty<Windows.UI.Xaml.Visibility>(ref this._favoritesVisibility, value); }
        }

        private int _viewCount = 0;
        [IgnoreDataMember]
        public int ViewCount
        {
            get { return this._viewCount; }
            set { this.SetProperty<int>(ref this._viewCount, value); }
        }

        private int _numZoomedInRows = 3;
        [IgnoreDataMember]
        public int NumZoomedInRows
        {
            get { return this._numZoomedInRows; }
            set { this.SetProperty<int>(ref this._numZoomedInRows, value); }
        }

        private int _numZoomedOutRows = 5;
        [IgnoreDataMember]
        public int NumZoomedOutRows
        {
            get { return this._numZoomedOutRows; }
            set { this.SetProperty<int>(ref this._numZoomedOutRows, value); }
        }
            
        #endregion Bindable Properties

        #region Constructors

        public RestaurantListViewModel()
        {
            this.CheckSize();
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        #endregion Constructors

        #region Private Methods

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.CheckSize();
        }

        private void CheckSize()
        {
            if (Window.Current.Bounds.Height >= 1080)
            {
                this.NumZoomedInRows = 4;
                this.NumZoomedOutRows = 6;
            }
            else
            {
                this.NumZoomedInRows = 3;
                this.NumZoomedOutRows = 4;
            }
                
        }

        private void CollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FilteredCollection":
                    this.CreateGroupings();
                    break;
            }
        }

        private async Task CreateGroupings()
        {
            List<Lib.Models.Restaurant> restauarants;
            if (CollectionViewModel.FilteredCollection != null)
                restauarants = CollectionViewModel.FilteredCollection.ToList();
            else
                restauarants = this.RestaurantList.ToList();

            this.ViewCount = restauarants.Count;


            foreach (Lib.Models.Restaurant restaurant in this.RestaurantList)
                await Lib.Services.Unity.FillFactualAsync(restaurant);

            Dictionary<Lib.Models.Cuisines, List<Lib.Models.Restaurant>> cuisineGroups =
                new Dictionary<Lib.Models.Cuisines, List<Lib.Models.Restaurant>>();
            IEnumerable<Lib.Models.Restaurant> restaurants;

            foreach (Lib.Models.Cuisines cuisine in Enum.GetValues(typeof(Lib.Models.Cuisines)).Cast<Lib.Models.Cuisines>().OrderBy(c => c.ToString()))
            {
                restaurants = restauarants.Where(r => null != r.Cuisines && r.Cuisines.Contains(cuisine));
                if(null != restaurants && 0 < restaurants.Count())
                    cuisineGroups.Add(cuisine, restaurants.ToList());
            }

            this.RestaurantsGroupedByCuisine = cuisineGroups;

            Dictionary<Lib.Models.PriceRanges, List<Lib.Models.Restaurant>> priceRangeGroups = 
                new Dictionary<Lib.Models.PriceRanges, List<Lib.Models.Restaurant>>();

            foreach (Lib.Models.PriceRanges priceRange in Enum.GetValues(typeof(Lib.Models.PriceRanges)))
            {
                restaurants = restauarants.Where(r => r.PriceRange.HasValue && r.PriceRange.Equals(priceRange));
                if (null != restaurants && 0 < restaurants.Count())
                    priceRangeGroups.Add(priceRange, restaurants.ToList());
            }

            this.RestaurantsGroupedByPriceRange = priceRangeGroups;
        }

        #endregion Private Methods
    }
}
