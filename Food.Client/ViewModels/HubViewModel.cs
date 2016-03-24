using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Helpers;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Lib.SampleData;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp;
using Windows.UI.Xaml;
using Microsoft.DPE.ReferenceApps.Food.Lib;


namespace Microsoft.DPE.ReferenceApps.Food.Client.ViewModels
{
    public class HubViewModel : ViewModelBase
    {
        // constants
        const string NOLOCATIONSTRING = "UNKNOWN";
        const string RESTAURANTKEY = "Key";
        const int FAVORITESTAKE = 8;
        const int NEARMETAKE = 4;
        const int RECENTTAKE = 4;
        const int TRENDINGTAKE = 7;

        // lists for collection view
        public IEnumerable<Restaurant> NearMeAll;
        public IEnumerable<Restaurant> RecentAll;
        public IEnumerable<Restaurant> TrendingAll;
        public IEnumerable<Restaurant> FavoritesAll;

        // bindables
        public ObservableCollection<Restaurant> FavoritesHub { get; private set; }
        public ObservableCollection<Restaurant> NearMeHub { get; private set; }
        public ObservableCollection<Restaurant> RecentHub { get; private set; }
        public ObservableCollection<Restaurant> TrendingHub { get; private set; }
        public Uri StaticMapUri { get; set; }

        private bool _motionSupported = false;
        public bool MotionSupported
        {
            get { return this._motionSupported; }
            set { this.SetProperty<bool>(ref this._motionSupported, value); }
        }

        private Restaurant _surpriseMeRestaurant;
        public Restaurant SurpriseMeRestaurant
        {
            get { return this._surpriseMeRestaurant; }
            set { this.SetProperty<Restaurant>(ref this._surpriseMeRestaurant, value); }
        }

        // members
        private bool m_DataIsBad = false;
        private System.Threading.SynchronizationContext m_UiThread = System.Threading.SynchronizationContext.Current;
        private Random m_SurpriseMeRan = new Random();
        private int _smIndex = 0;

        // events
        public EventHandler LocationIsEmpty;
        private void RaiseLocationIsEmpty()
        {
            this.NearMeHub.Clear();
            this.RecentHub.Clear();
            this.TrendingHub.Clear();
            RaisePropertyChanged(this);
            if (LocationIsEmpty != null)
                LocationIsEmpty(this, EventArgs.Empty);

        }

        public EventHandler DataIsEmpty;
        private void RaiseDataEmpty()
        {
            //raise event
            this.NearMeHub.Clear();
            this.RecentHub.Clear();
            this.TrendingHub.Clear();
            RaisePropertyChanged(this);
            if (null != this.DataIsEmpty)
                this.DataIsEmpty(this, EventArgs.Empty);
        }

        //constructor
        public HubViewModel()
        {
            // setup hub lists (never set them again!)
            this.FavoritesHub = new ObservableCollection<Restaurant>();
            this.NearMeHub = new ObservableCollection<Restaurant>();
            this.RecentHub = new ObservableCollection<Restaurant>();
            this.TrendingHub = new ObservableCollection<Restaurant>();

            // initial load
            Task.Factory.StartNew(async () => { await AppData.Current.LoadDataAsync(true); });

            // when favorites changes, update favorites
            AppData.Current.FavoritesChanged += (s, e) => ParseFavorites(); ;

            // when data is loading, show the progressbar
            AppData.Current.DataLoading += (s, e) => m_UiThread.Post((o) => { ProgressBarVisibility = Visibility.Visible; }, null);

            // when data is loaded/reloaded, update the hub
            AppData.Current.DataLoaded += (s, e) => m_UiThread.Post(async (o) => { await this.UpdateEverythingAsync(); }, null);

            // when location is lost, bubble that up to the UI
            AppData.Current.LocationNotFound += (s, e) => m_UiThread.Post((o) => { RaiseLocationIsEmpty(); }, null);

            // when there is no data, bubble that up to the UI
            AppData.Current.DataNotFound += (s, e) => m_UiThread.Post((o) => { RaiseDataEmpty(); }, null);
        }

        // prop: friendly name for location
        public string Location
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(AppData.Current.City) &&
                    !string.IsNullOrWhiteSpace(AppData.Current.State))
                    return string.Format("{0}, {1}", AppData.Current.City, AppData.Current.State);
                return NOLOCATIONSTRING;
            }
        }

        // prop: visible when loading
        private Visibility m_ProgressBarVisibility;
        public Visibility ProgressBarVisibility
        {
            get { return m_ProgressBarVisibility; }
            set
            {
                m_ProgressBarVisibility = value;
                RaisePropertyChanged(this);
            }
        }

        //method: update everyting
        bool m_UpdateEverythingAsyncBusy = false;
        public async Task UpdateEverythingAsync(bool includeFavorites = true)
        {
            // not two at a time
            while (this.m_UpdateEverythingAsyncBusy)
                await Task.Delay(100);
            this.m_UpdateEverythingAsyncBusy = true;

            if (includeFavorites)
                this.ParseFavorites();
            this.ParseNearMe();
            this.ParseRecent();
            this.ParseTrending();
            this.RefreshSurpriseMe();

            // hide progress bar, AppData.DataLoading shows it
            this.ProgressBarVisibility = Visibility.Collapsed;
            this.m_UpdateEverythingAsyncBusy = false;
        }

        //method: update favorites
        public int HowManyMoreFavorites { get; set; }
        private void ParseFavorites()
        {
            // determine favs for hub
            this.FavoritesAll = AppData.Current.Favorites.ToList();
            var _HubList = FavoritesAll.OrderBy(x => x.Name)
                .OrderBy(x => x.Distance)
                .Take(FAVORITESTAKE)
                .Select((r, i) => r.ToClone(i)).ToArray();

            // setup for layout in variable sized wrap grid
            this.SetSpans(_HubList, 228, 189, 1);
            this.SetSpans(_HubList, 163, 133, 2);
            this.SetSpans(_HubList, 226, 113, 3);
            this.SetSpans(_HubList, 183, 133, 4);
            this.SetSpans(_HubList, 250, 113, 5);
            this.SetSpans(_HubList, 118, 189, 6);
            this.SetSpans(_HubList, 130, 129, 7);
            this.SetSpans(_HubList, 130, 193, 8);

            // merge 
            this.FavoritesHub.Clear();
            foreach (Restaurant r in _HubList)
                this.FavoritesHub.Add(r);

            // count
            this.HowManyMoreFavorites = this.FavoritesAll.Count() - _HubList.Count();

            //raise event
            this.RaisePropertyChanged(this);
        }

        public int HowManyMoreNearMe { get; set; }
        private void ParseNearMe()
        {
            // this does not retreive data, it only parses it
            // if the data is bad, there is nothing to do
            if (m_DataIsBad)
            {
                this.NearMeHub.Clear();
                return;
            }

            // all nearme (we will pass this to the collection view)
            this.NearMeAll = AppData.Current.Restaurants;

            // update hub list
            var _HubList = this.NearMeAll
                .OrderByDescending(x => x.AverageRating)
                .Take(NEARMETAKE)
                .Select((x, i) => x.ToClone(i)).ToArray();

            // merge
            this.NearMeHub.Clear();
            foreach (Restaurant r in _HubList)
                this.NearMeHub.Add(r);

            // count
            this.HowManyMoreNearMe = this.NearMeAll.Count() - _HubList.Count();

            //update map

            // get the map URL. The imgMap object is bound to this property in XAML, 
            // when this value is set the map automatically updates
            this.StaticMapUri = Unity.GetMapUri(this.NearMeHub, 600, 600);

            //raise event
            RaisePropertyChanged(this);
        }

        public int HowManyMoreRecent { get; set; }
        private void ParseRecent()
        {
            // this does not retreive data, it only parses it
            // if the data is bad, there is nothing to do
            if (m_DataIsBad)
            {
                this.RecentHub.Clear();
                return;
            }

            // all recent (we will pass this to the collection view)
            var _RecentThreshold = DateTime.Now.AddDays(-Settings.RecentlyReviewedMinimumInDays);
            if (AppData.Current.UseMockData)
                this.RecentAll = AppData.Current.Restaurants;
            else
                // filter restaurants based on criteria
                this.RecentAll = AppData.Current.Restaurants
                    .Where(x => x.NewestReviewDate >= _RecentThreshold);

            // hub clones
            var _HubList = this.RecentAll.OrderByDescending(x => x.AverageRating)
                .Take(RECENTTAKE)
                .Select((x, i) => x.ToClone(i)).ToArray();

            // merge
            this.RecentHub.Clear();
            foreach (Restaurant r in _HubList)
                this.RecentHub.Add(r);

            // count
            this.HowManyMoreRecent = RecentAll.Count() - _HubList.Count();

            //raise event
            RaisePropertyChanged(this);
        }

        public int HowManyMoreTrending { get; set; }
        private void ParseTrending()
        {
            // this does not retreive data, it only parses it
            // if the data is bad, there is nothing to do
            if (m_DataIsBad)
            {
                TrendingHub.Clear();
                return;
            }

            // all trending (we will pass this to the collection view)
            var _TrendingDayThreshold = DateTime.Now.AddDays(-Settings.TrendingMinimumReviewAgeInDays);
            if (AppData.Current.UseMockData)
                this.TrendingAll = AppData.Current.Restaurants;
            else
                // filter restaurants based on criteria
                this.TrendingAll = AppData.Current.Restaurants
                    .Where(x => x.AverageRating >= Settings.TrendingMinimumRating)
                    .Where(x => x.NewestReviewDate > _TrendingDayThreshold);

            // hub clones
            var _HubList = this.TrendingAll.Take(TRENDINGTAKE)
                .OrderByDescending(x => x.AverageRating)
                .Select((x, i) => x.ToClone(i)).ToArray();

            // setup for layout in variable sized wrap grid
            this.SetSpans(_HubList, 330, 312, 1);
            this.SetSpans(_HubList, 200, 124, 2);
            this.SetSpans(_HubList, 130, 124, 3);
            this.SetSpans(_HubList, 268, 142, 4);
            this.SetSpans(_HubList, 268, 100, 5);
            this.SetSpans(_HubList, 118, 193, 6);
            this.SetSpans(_HubList, 150, 193, 7);

            // merge
            this.TrendingHub.Clear();
            foreach (Restaurant r in _HubList)
                this.TrendingHub.Add(r);

            // count
            this.HowManyMoreTrending = this.TrendingAll.Count() - _HubList.Count();

            //raise event
            RaisePropertyChanged(this);
        }

        // helper method
        private void SetSpans(Restaurant[] list, int colSpan, int rowSpan, int index)
        {
            var _Item = list.FirstOrDefault(x => x.Index == index);
            if (_Item == null)
                return;
            _Item.ColSpan = colSpan;
            _Item.RowSpan = rowSpan;
        }

        // Rerefreshes surprise me restauarant
        public void RefreshSurpriseMe()
        {
            if (null != AppData.Current.Restaurants && 1 < AppData.Current.Restaurants.Count)
            {
                int _newIndex = _smIndex;
                while(_smIndex == _newIndex)
                    _newIndex = m_SurpriseMeRan.Next(AppData.Current.Restaurants.Count);

                _smIndex = _newIndex;

                this.SurpriseMeRestaurant = AppData.Current.Restaurants[_smIndex];
            }
            else if (0 < AppData.Current.Restaurants.Count)
            {
                this.SurpriseMeRestaurant = AppData.Current.Restaurants[0];
            }
        }
    }
}
