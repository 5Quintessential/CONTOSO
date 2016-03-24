using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Helpers;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services;
using Windows.ApplicationModel;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Models
{
    [DataContract]
    public class AppData : BaseModel
    {
        #region Bindable Properties

        private ObservableCollection<Restaurant> m_Favorites = new ObservableCollection<Restaurant>();
        [DataMember]
        public ObservableCollection<Restaurant> Favorites
        {
            get { return this.m_Favorites; }
            set { this.SetProperty(ref this.m_Favorites, value, "Favorites"); }
        }

        private bool m_UseMockData = Lib.Settings.UseMockData;
        [IgnoreDataMember]
        public bool UseMockData
        {
            get { return this.m_UseMockData; }
            set
            {
                this.Latitude = null;
                this.Longitude = null;
                Settings.UseMockData = value;
                this.SetProperty<bool>(ref this.m_UseMockData, value);
                this.LoadDataAsync(!value);
                this.LoadFavoritesAsync();
            }
        }

        [DataMember]
        public string Street { get; private set; }
        [DataMember]
        public string City { get; private set; }
        [DataMember]
        public string State { get; private set; }
        [DataMember]
        public string Zip { get; private set; }
        [DataMember]
        public double? Latitude { get; private set; }
        [DataMember]
        public double? Longitude { get; private set; }

        private int m_nearMeMaxDistance;
        [DataMember]
        public int NearMeMaxDistance
        {
            get { return this.m_nearMeMaxDistance; }
            set { this.SetProperty<int>(ref this.m_nearMeMaxDistance, value); this.SaveNearMeMaxDistance(); }
        }
        [IgnoreDataMember]
        public int NearMeMaxDistanceThreshold
        {
            get { return Settings.NearMeMaxDistanceThresholdInMiles; }
        }

        #endregion Bindable Properties

        #region Properties

        private readonly string MockDataFile = @"Food.Lib\SampleData\AppData.json";
        private readonly string RestaurantCacheKey = "Restaurants_Cache_Key";
        private readonly string LocationCacheKey = "Location_Cache_Key";
        private bool m_ThereIsNoLocation = false;
        private bool m_CurrentDataIsBad = false;

        private static AppData s_Current;
        public static AppData Current
        {
            get
            {
                // singleton
                if (null == s_Current)
                    s_Current = new AppData(); return s_Current;
            }
        }

        public string Location { get { return string.Format("{0}, {1}", City, State); } }

        private bool HasLocation { get { return Latitude.HasValue && Longitude.HasValue; } }
        public ObservableCollection<Restaurant> Restaurants { get; private set; }
        public bool IsDataLoaded { get; private set; }

        #endregion Properties

        #region Events

        public event EventHandler LocationNotFound;
        private void RaiseLocationNotFound()
        {
            this.m_ThereIsNoLocation = true;
            if (LocationNotFound != null)
                LocationNotFound(this, EventArgs.Empty);
        }

        public event EventHandler DataNotFound;
        private void RaiseDataNotFound()
        {
            IsDataLoaded = false;
            if (DataNotFound != null)
                DataNotFound(this, EventArgs.Empty);
        }

        public event EventHandler DataLoading;
        private void RaiseDataLoading()
        {
            IsDataLoaded = false;
            if (DataLoading != null)
                DataLoading(this, EventArgs.Empty);
        }

        public event EventHandler DataLoaded;
        private void RaiseDataLoaded()
        {
            this.IsDataLoaded = true;
            if (DataLoaded != null)
                DataLoaded(this, EventArgs.Empty);
        }

        public event EventHandler FavoritesChanged;
        private void RaiseFavoritesChanged()
        {
            if (FavoritesChanged != null)
                FavoritesChanged(this, EventArgs.Empty);
        }

        #endregion Events

        #region Constructors

        private AppData() { /* do nothing */ }

        #endregion Constructors

        #region Private Methods

        AppData m_MockDataAsync = null;
        async private Task<AppData> MockDataAsync()
        {
            // retrieve mock data
            // IANBE: Bug when reenabing mock data
            //if (m_MockDataAsync != null)
            //    return m_MockDataAsync;
            try
            {
                var _File = await Package.Current.InstalledLocation.GetFileAsync(MockDataFile);
                var _Json = await Windows.Storage.FileIO.ReadTextAsync(_File);
                m_MockDataAsync = AppData.FromJSON(_Json);
                return m_MockDataAsync;
            }
            catch { /* should never fail */ throw; }
        }

        async private Task LoadFavoritesAsync()
        {
            // mock 
            if (UseMockData)
            {
                this.Favorites = (await MockDataAsync()).Favorites;
                return;
            }
            else
            {
                // from isolated storage
                try
                {
                    var _Favorites =
                        await StorageHelper.ReadFileAsync<ObservableCollection<Restaurant>>(
                            Lib.Settings.FavoritesCacheKey, StorageHelper.StorageStrategies.Roaming);
                    this.Favorites.Clear();
                    foreach (var item in _Favorites)
                        this.Favorites.Add(item);
                    RaiseFavoritesChanged();
                }
                catch (Exception) { /* swallow */ }
            }
        }

        async private Task<bool> LoadLocationAsync(bool refreshLocation)
        {
            // mock
            if (UseMockData)
            {
                var _AppData = await MockDataAsync();
                Latitude = _AppData.Latitude;
                Longitude = _AppData.Longitude;
                Street = _AppData.Street;
                City = _AppData.City;
                State = _AppData.State;
                Zip = _AppData.Zip;
                return true;
            }
            else
            {

                // use isolated storage
                // IANBE: We should not be going back to storage to retrieve this value each time
                if (!refreshLocation)
                {
                    // IANBE: We should not be going back to storage to retrieve this value
                    //var _Location = await StorageHelper
                    //    .ReadFileAsync<Microsoft.DPE.ReferenceApps.Food.Lib.Services.BingMaps.SearchHelper.Resource>(
                    //    LocationCacheKey, StorageHelper.StorageStrategies.Local);
                    //await SetLocationAsync(_Location);
                    return true;
                }
                else
                {
                    // use the web
                    try
                    {
                        // get geolocation (coordinates)
                        var _Location = await Unity.GetCurrentLocationAsync();

                        // get geocode (friendly name)
                        var _Name = await Unity.CoordinateToStringAsync(
                            _Location.Coordinate.Latitude,
                            _Location.Coordinate.Longitude);
                        await this.SetLocationAsync(_Name.FirstOrDefault());
                        return true;
                    }
                    catch (Exception)
                    {
                        RaiseLocationNotFound();
                        return false;
                    }
                }
            }
        }

        async private Task<bool> LoadRestaurantsAsync(bool useCache)
        {
            if (UseMockData)
            {
                this.Restaurants = new ObservableCollection<Restaurant>(await SampleData.Restaurants.LoadAsync());
                return true;
            }

            // use isolated storage
            //if (useCache)
            //{
            //    try
            //    {
            //        this.Restaurants = await StorageHelper.ReadFileAsync<ObservableCollection<Restaurant>>(
            //            this.RestaurantCacheKey, StorageHelper.StorageStrategies.Roaming);
            //    }
            //    catch
            //    {
            //        this.RaiseDataNotFound();
            //        return false;
            //    }
            //}

            // use the web
            try
            {
                // call service 
                Filter _Filter = new Models.Filter
                {
                    Search = "Restaurant",
                    Latitude = this.Latitude ?? 0d,
                    Longitude = this.Longitude ?? 0d,
                    RadiusInMiles = this.NearMeMaxDistance,
                    MaxResults = 20
                };
                var _Results = (await Unity.SearchYelpAsync(_Filter));
                if (!_Results.Any())
                    this.RaiseDataNotFound();
                this.Restaurants = new ObservableCollection<Restaurant>(_Results);
            }
            catch (Exception)
            {
                this.RaiseDataNotFound();
                return false;
            }
            return true;
        }

        private bool LoadMaxDistance()
        {
            if (this.UseMockData)
            {
                this.NearMeMaxDistance = 6;
                return true;
            }
            else
            {
                // Pull from isolated storage or use default in Lib.Settings
                try
                {
                    this.NearMeMaxDistance = StorageHelper.GetSetting<int>(
                        Lib.Settings.NearMeMaxDistanceCacheKey,
                            Settings.NearMeMaxDistanceThresholdInMiles,
                            StorageHelper.StorageStrategies.Roaming);
                    return true;
                }
                catch (Exception) { /* swallow */ }
            }

            return false;
        }

        private async Task<bool> SaveFavorites()
        {
            bool success = true;

            if (!this.UseMockData)
            {
                success = await StorageHelper.WriteFileAsync(Lib.Settings.FavoritesCacheKey,
                    this.Favorites,
                    StorageHelper.StorageStrategies.Roaming);
            }
            this.RaiseFavoritesChanged();
            return success;
        }

        private void SaveNearMeMaxDistance()
        {
            StorageHelper.SetSetting<int>(Lib.Settings.NearMeMaxDistanceCacheKey,
                this.NearMeMaxDistance,
                StorageHelper.StorageStrategies.Roaming);
        }

        /// <summary>
        /// Returns true if you have an internet connection and false otherwise
        /// </summary>
        private static bool IsConnected()
        {
            var profile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            if (profile != null)
            {
                return (profile.GetNetworkConnectivityLevel() != Windows.Networking.Connectivity.NetworkConnectivityLevel.None);
            }
            else
            {
                return false;
            }
        }

        #endregion Private Methods

        #region Public Methods

        bool m_LoadDataAsyncBusy = false;
        async public Task LoadDataAsync(bool refreshLocation)
        {
            // start
            while (this.m_LoadDataAsyncBusy)
                await Task.Delay(100);
            this.m_LoadDataAsyncBusy = true;
            this.RaiseDataLoading();

            // load data
            LoadMaxDistance();
            await LoadFavoritesAsync();
            var _HasLocation = await LoadLocationAsync(refreshLocation);
            await LoadRestaurantsAsync(_HasLocation);

            // end
            this.RaiseDataLoaded();
            this.m_LoadDataAsyncBusy = false;
        }

        public async Task SetLocationAsync(Lib.Services.BingMaps.SearchHelper.Resource resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");
            var _Coordinates = resource.geocodePoints.FirstOrDefault(x => x.calculationMethod == "Rooftop");
            if (_Coordinates == null)
                _Coordinates = resource.geocodePoints.FirstOrDefault();
            Latitude = _Coordinates.coordinates[0];
            Longitude = _Coordinates.coordinates[1];
            Street = resource.address.addressLine;
            City = resource.address.locality;
            State = resource.address.adminDistrict;
            Zip = resource.address.postalCode;
            RaisePropertyChanged(this);

            // persist location to cache, last known location
            await StorageHelper.WriteFileAsync(LocationCacheKey, resource, 
                StorageHelper.StorageStrategies.Local);

            // now, if you wnt to reload, call loaddata()
        }

        public async void ClearFavorites()
        {
            this.Favorites = new ObservableCollection<Restaurant>();

            await this.SaveFavorites();
        }

        public async Task AddFavoritesAsync(IEnumerable<Restaurant> restaurants)
        {
            foreach (var item in restaurants)
                await this.AddFavoriteAsync(item, false);
            await this.SaveFavorites();
        }

        public async Task AddFavoriteAsync(Restaurant restaurant, bool save = true)
        {
            await this.RemoveFavoriteAsync(restaurant, false);
            this.Favorites.Add(restaurant);
            if (save)
                await this.SaveFavorites();
        }

        public async Task RemoveFavoritesAsync(IEnumerable<Restaurant> restaurants)
        {
            if (null != restaurants)
            {
                foreach (var item in restaurants)
                    await this.RemoveFavoriteAsync(item, false);
                await this.SaveFavorites();
            }
        }

        public async Task RemoveFavoriteAsync(Restaurant restaurant, bool save = true)
        {
            if (this.IsInFavorites(restaurant))
            {
                Restaurant restaurantToRemove =
                    this.Favorites.FirstOrDefault(r => r.Key == restaurant.Key && r.Name == restaurant.Name);
                if (null != restaurantToRemove)
                    this.Favorites.Remove(restaurantToRemove);
            }
            if (save)
                await this.SaveFavorites();
        }

        public bool IsInFavorites(Restaurant restaurant)
        {
            return Favorites.Any(x => x.Key == restaurant.Key);
        }

        public static AppData FromJSON(string json)
        {
            return Lib.Helpers.SerializationHelper.JSON.Deserialize<AppData>(json);
        }

        /// <summary>
        /// Attempts to get new Yelp data from online about the restaurant that was passed in (so it will refresh the restaurant reviews, etc)
        /// </summary>
        /// <param name="restaurant">The restaurant to refresh</param>
        /// <returns>The refreshed restaurant if we could get the data successfully; otherwise the same restaurant that was passed in is returned</returns>
        public static async Task<Restaurant> RefreshRestaurant(Restaurant restaurant)
        {
            try
            {
                if (IsConnected())
                {
                    // Now we will grab the latest version of restaurant with updated data and reviews from online.  
                    var results = await Unity.SearchYelpAsync(new Filter()
                    {
                        MaxResults = 1,
                        Latitude = restaurant.Latitude,
                        Longitude = restaurant.Longitude,
                        Search = restaurant.Name,
                        Category = "restaurants"
                    }, true /* force refresh of data */);

                    if (results != null && results.Count() > 0)
                    {
                        restaurant = results.FirstOrDefault();
                    }
                }
            }
            catch { /* swallow any errors; it will just use the original version of the restaurant that was passed in */ }
            return restaurant;
        }

        public static async Task<Restaurant> RestaurantForKeyAsync(string restaurantKey)
        {
            if (!string.IsNullOrWhiteSpace(restaurantKey))
            {
                if (!Lib.Models.AppData.Current.IsDataLoaded)
                    await Lib.Models.AppData.Current.LoadDataAsync(true);

                if (null != Lib.Models.AppData.Current.Restaurants &&
                    0 < Lib.Models.AppData.Current.Restaurants.Count)
                {
                    return Lib.Models.AppData.Current.Restaurants.FirstOrDefault(r =>
                            r.Key.Equals(restaurantKey));
                }
            }

            return null;
        }

        public static async Task<Restaurant> RestaurantForLatLonNameAsync(double lat, double lon, string name)
        {
            Restaurant restaurant = null;

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.ToLowerInvariant();

                if (!Lib.Models.AppData.Current.IsDataLoaded)
                    await Lib.Models.AppData.Current.LoadDataAsync(true);

                if (null != Lib.Models.AppData.Current.Restaurants &&
                    0 < Lib.Models.AppData.Current.Restaurants.Count)
                {
                    restaurant =
                        Lib.Models.AppData.Current.Restaurants.FirstOrDefault(r =>
                            r.YelpRecordLoaded &&
                            r.YelpRecord.latitude == lat &&
                            r.YelpRecord.longitude == lon &&
                            r.Name.ToLowerInvariant() == name);
                }

                if (null == restaurant)
                {
                    var results =
                        await Unity.SearchYelpAsync(new Filter()
                        {
                            MaxResults = 1,
                            Latitude = lat,
                            Longitude = lon,
                            Search = name,
                            Category = "restaurants"
                        });

                    if (null != results &&
                        0 < results.Count())
                    {
                        restaurant = results.FirstOrDefault();
                        if (null != restaurant)
                            Lib.Models.AppData.Current.Restaurants.Add(restaurant);
                    }
                }

                if (name != restaurant.Name.ToLowerInvariant())
                    restaurant = null;
            }

            return restaurant;
        }

        #endregion Public Methods
    }
}
