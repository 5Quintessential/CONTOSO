using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Helpers;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services
{
    public static class Unity
    {
        private static Factual.ISearchHelper s_FactualSearch;
        private static Factual.ICrosswalkHelper s_FactualCrosswalk;
        private static Yelp.IV1Helper s_YelpSearch;
        private static Yelp.IBusinessHelper s_YelpBusinessSearch;
        private static OpenMenu.ICrosswalkHelper s_OpenMenuCrosswalk;
        private static OpenMenu.IMenuHelper s_OpenMenuMenu;

        static Unity()
        {
            InitUnity();
        }

        public static void InitUnity(bool forceRefresh = false)
        {
            if (forceRefresh)
            {
                UseRealOnlineData();
            }
            else if (Lib.Settings.UseMockData)
            {
                UseMockSampleData();
            }
            else
            {
                UseRealOnlineData();
            }
        }

        private static void UseMockSampleData()
        {
            // use mock services
            s_YelpSearch = new SampleData.MockYelpSearch();
            s_YelpBusinessSearch = new SampleData.MockYelpBusinessSearch();
            s_FactualCrosswalk = new SampleData.MockFactualCrosswalk();
            s_FactualSearch = new SampleData.MockFactualSearch();
            s_OpenMenuCrosswalk = new SampleData.MockOpenmenuCrosswalk();
            s_OpenMenuMenu = new SampleData.MockOpenmenuSearch();
        }

        private static void UseRealOnlineData()
        {
            // use real service
            s_YelpSearch = new Yelp.V1Helper(Settings.YelpV1Key);
            s_YelpBusinessSearch = new Yelp.BusinessHelper(Settings.YelpConsumerKey, Settings.YelpConsumerSecret, Settings.YelpToken, Settings.YelpTokenSecret);
            s_FactualSearch = new Factual.SearchHelper(Settings.FactualConsumerKey, Settings.FactualConsumerSecret);
            s_FactualCrosswalk = new Factual.CrosswalkHelper(Settings.FactualConsumerKey, Settings.FactualConsumerSecret);
            s_OpenMenuCrosswalk = new OpenMenu.CrosswalkHelper(Settings.OpenMenuKey);
            s_OpenMenuMenu = new OpenMenu.MenuHelper(Settings.OpenMenuKey);
        }

        public async static Task RefreshRestaurants(IEnumerable<Restaurant> restaurants)
        {
            foreach (var item in restaurants)
                await RefreshRestaurant(item);
        }

        private async static Task RefreshRestaurant(Restaurant restaurant)
        {
            restaurant.FactualRecordLoaded = false;
            await FillFactualAsync(restaurant);

            restaurant.YelpBusinessRecordLoaded = false;
            await FillYelpBusinessAsync(restaurant);

            YelpBusinessIntoYelpV1(restaurant);

            restaurant.OpenmenuRecordLoaded = false;
            await FillOpenMenuAsync(restaurant);
        }

        public async static Task<IEnumerable<Restaurant>> SearchYelpAsync(Filter filter, bool forceRefresh = false)
        {
            try
            {
                InitUnity(forceRefresh);
                var _Yelps = await s_YelpSearch.SearchAsync(filter.Search, filter.Latitude, filter.Longitude, filter.Category, limit: filter.MaxResults, radius: filter.RadiusInMiles);
                var _Restaurants = _Yelps.Select(x => new Restaurant(x));
                return _Restaurants.ToList();
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                {
                    Debug.WriteLine("Unity.SearchYelpAsync:" + e.Message);
                    e = e.InnerException;
                }
                return null;
            }
        }

        #region Factual

        public async static void FillFactualFireAndForget(IEnumerable<Restaurant> restaurants)
        {
            await FillFactualAsync(restaurants);
        }

        public async static Task FillFactualAsync(IEnumerable<Restaurant> restaurants)
        {
            foreach (var item in restaurants.AsParallel())
                try { await FillFactualAsync(item); }
                catch (Exception ex)
                {
                    while (ex.InnerException != null)
                        ex = ex.InnerException;
                    Debug.WriteLine("FillFactualAsync:" + ex.Message);
                    continue; /* process all */
                }
        }

        public async static void FillFactualFireAndForget(Restaurant restaurant)
        {
            await FillFactualAsync(restaurant);
        }

        public async static Task FillFactualAsync(Restaurant restaurant)
        {
            try
            {
                InitUnity();
                if (restaurant.FactualRecordLoaded)
                    return; // we will not load it twice
                restaurant.FactualRecord = null; // null flags don't retry

                // crosswalk
                var _FactualId = await s_FactualCrosswalk.SearchFactualAsync(restaurant.YelpRecord.id, Factual.FactualCrosswalkNamespaces.Yelp);
                if (null != _FactualId && !string.IsNullOrWhiteSpace(_FactualId.FactualId))
                {
                    // fetch and set
                    var _Factual = await s_FactualSearch.SearchAsync(_FactualId.FactualId);
                    restaurant.FactualRecord = _Factual; // null flags don't retry
                }

                if (null == restaurant.FactualRecord)
                {
                    IEnumerable<Factual.SearchHelper.Restaurant> _factualMatches =
                        await s_FactualSearch.SearchAsync(restaurant.Name,
                            restaurant.YelpRecord.latitude,
                            restaurant.YelpRecord.longitude,
                            0d,
                            null,
                            1,
                            1);

                    if (null != _factualMatches)
                        restaurant.FactualRecord = _factualMatches.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                {
                    Debug.WriteLine("Unity.FillFactualAsync:" + e.Message);
                    e = e.InnerException;
                }
            }
        }

        #endregion

        #region Openmenu

        public async static void FillOpenMenuFireAndForget(IEnumerable<Restaurant> restaurants) { await FillOpenMenuAsync(restaurants); }

        public async static Task FillOpenMenuAsync(IEnumerable<Restaurant> restaurants)
        {
            foreach (var item in restaurants.AsParallel())
                try { await FillOpenMenuAsync(item); }
                catch (Exception ex)
                {
                    while (ex.InnerException != null)
                        ex = ex.InnerException;
                    Debug.WriteLine("FillOpenMenuAsync:" + ex.Message);
                    continue; /* process all */
                }
        }

        public async static void FillOpenMenuFireAndForget(Restaurant restaurant) { await FillOpenMenuAsync(restaurant); }

        public async static Task FillOpenMenuAsync(Restaurant restaurant)
        {
            try
            {
                InitUnity();
                if (restaurant.OpenmenuRecordLoaded)
                    return; // we will not load it twice
                restaurant.OpenmenuRecord = null; // null flags don't retry

                if (restaurant.FactualRecord == null)
                    await FillFactualAsync(restaurant); // factual is required (for crosswalk)
                if (restaurant.FactualRecord == null)
                    return; // factual not found

                // crosswalk
                var _Crosswalk = await s_OpenMenuCrosswalk.SearchAsync(restaurant.FactualRecord.factual_id);
                if (_Crosswalk == null | string.IsNullOrWhiteSpace(_Crosswalk))
                    return; // open menu not found

                // fetch and set
                var _Menu = await s_OpenMenuMenu.SearchAsync(_Crosswalk);
                restaurant.OpenmenuRecord = _Menu; // null flags don't retry
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                {
                    Debug.WriteLine("Unity.FillOpenMenuAsync:" + e.Message);
                    e = e.InnerException;
                }
            }
        }

        #endregion

        #region YelpBusiness

        public async static void FillYelpBusinessFireAndForget(IEnumerable<Restaurant> restaurants) { await FillYelpBusinessAsync(restaurants); }

        public async static Task FillYelpBusinessAsync(IEnumerable<Restaurant> restaurants)
        {
            foreach (var item in restaurants.AsParallel())
                try { await FillYelpBusinessAsync(item); }
                catch (Exception ex)
                {
                    while (ex.InnerException != null)
                        ex = ex.InnerException;
                    Debug.WriteLine("FillYelpBusinessAsync:" + ex.Message);
                    continue; /* process all */
                }
        }

        public async static void FillYelpBusinessFireAndForget(Restaurant restaurant) { await FillYelpBusinessAsync(restaurant); }

        public async static Task FillYelpBusinessAsync(Restaurant restaurant)
        {
            try
            {
                InitUnity();
                if (restaurant.YelpRecord == null)
                    return;
                if (restaurant.YelpBusinessRecordLoaded)
                    return; // we will not load it twice
                restaurant.YelpBusinessRecord = null; // null flags don't retry

                // fetch and set
                var _Business = await s_YelpBusinessSearch.SearchAsync(restaurant.YelpRecord.url);
                restaurant.YelpBusinessRecord = _Business; // null flags don't retry
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                {
                    Debug.WriteLine("Unity.FillYelpBusinessAsync:" + e.Message);
                    e = e.InnerException;
                }
            }
        }

        #endregion

        #region Map

        public static Uri GetMapUri(Models.Restaurant restaurant, int width, int height, string pinText = null, double? userLat = null, double? userLong = null, int? zoom = null, BingMaps.SearchHelper.StaticMapIconStyles? iconStyle = BingMaps.SearchHelper.StaticMapIconStyles.BlueBox)
        {
            return GetMapUri(new Models.Restaurant[] { restaurant }, width, height, pinText, userLat, userLong, zoom, iconStyle);
        }

        public static Uri GetMapUri(IEnumerable<Models.Restaurant> restaurants, int width, int height, string pinText = null, double? userLat = null, double? userLong = null, int? zoom = null, BingMaps.SearchHelper.StaticMapIconStyles? iconStyle = BingMaps.SearchHelper.StaticMapIconStyles.BlueBox)
        {
            var _Items = restaurants.Select(x => new BingMaps.SearchHelper.StaticMapPushpin
            {
                Latitude = x.YelpRecord.latitude,
                Longitude = x.YelpRecord.longitude,
                Text = (null != pinText) ? pinText : x.Index.ToString(),
                IconStyle = iconStyle.Value,
            });

            var _User = new BingMaps.SearchHelper.StaticMapPushpin
            {
                Latitude = userLat ?? 0d,
                Longitude = userLong ?? 0d,
                Text = string.Empty,
                IconStyle = BingMaps.SearchHelper.StaticMapIconStyles.Star,
            };
            if (userLat == null || userLong == null)
                _User = null;

            var _Helper = new BingMaps.SearchHelper(Settings.BingMapsKey);
            return _Helper.GetMapUrl(_Items, _User, null, new Size(width, height), zoom);
        }

        public static async Task<Windows.Devices.Geolocation.Geoposition> GetCurrentLocationAsync()
        {
            if (!IsLocationAllowed)
                throw new Exception("Location is not allowed; use Unity.IsLocationAllowed before calling.");
            Windows.Devices.Geolocation.Geoposition _Postion = null;
            try
            {
                var _Locator = new Windows.Devices.Geolocation.Geolocator();
                var _Token = new System.Threading.CancellationTokenSource().Token;
                _Postion = await _Locator.GetGeopositionAsync().AsTask(_Token);
            }
            catch { /* continue */ }
            return _Postion;
        }

        public async static Task<IEnumerable<Lib.Services.BingMaps.SearchHelper.Resource>> CoordinateToStringAsync(double latitude, double longitude)
        {
            var _Location = (await Lib.Services.BingMaps.SearchHelper.FindLocationByPointAsync(latitude, longitude))
                .Where(x => x.matchCodes.Contains("Good"));
            return _Location;
        }

        public async static Task<IEnumerable<Lib.Services.BingMaps.SearchHelper.Resource>> StringToCoordinateAsync(string address)
        {
            var _Location = (await Lib.Services.BingMaps.SearchHelper.FindLocationByQueryAsync(address))
                .Where(x => x.matchCodes.Contains("Good"));
            return _Location;
        }

        #endregion

        public static bool IsLocationAllowed
        {
            get
            {
                var _Status = new Windows.Devices.Geolocation.Geolocator().LocationStatus;
                return !(_Status == Windows.Devices.Geolocation.PositionStatus.Disabled
                    || _Status == Windows.Devices.Geolocation.PositionStatus.NotAvailable);
            }
        }

        public static bool IsNetworkAvailable
        {
            get
            {
                return Windows.Networking.Connectivity.NetworkInformation
                    .GetInternetConnectionProfile()
                    .GetNetworkConnectivityLevel() == Windows.Networking
                    .Connectivity.NetworkConnectivityLevel.InternetAccess;
            }
        }

        // helper
        private static void YelpBusinessIntoYelpV1(Restaurant restaurant)
        {
            restaurant.YelpRecord.address1 = restaurant.YelpBusinessRecord.location.address.FirstOrDefault() ?? "NONE";
            restaurant.YelpRecord.city = restaurant.YelpBusinessRecord.location.city;
            restaurant.YelpRecord.state = restaurant.YelpBusinessRecord.location.state_code;
            restaurant.YelpRecord.zip = restaurant.YelpBusinessRecord.location.postal_code;
            restaurant.YelpRecord.photo_url = restaurant.YelpBusinessRecord.image_url;
            restaurant.YelpRecord.avg_rating = restaurant.YelpBusinessRecord.rating;
            // anything else?

            if (restaurant.YelpBusinessRecord.reviews.Any())
            {
                // TODO: restaurant.YelpRecord.reviews[0].date = restaurant.YelpBusinessRecord.reviews[0].time_created;
                restaurant.YelpRecord.reviews[0].user_name = restaurant.YelpBusinessRecord.reviews[0].user.name;
                restaurant.YelpRecord.reviews[0].user_photo_url = restaurant.YelpBusinessRecord.reviews[0].user.image_url;
                restaurant.YelpRecord.reviews[0].rating = restaurant.YelpBusinessRecord.reviews[0].rating;
                restaurant.YelpRecord.reviews[0].text_excerpt = restaurant.YelpBusinessRecord.reviews[0].excerpt;

                if (restaurant.YelpBusinessRecord.reviews.Count > 1)
                {
                    // TODO: restaurant.YelpRecord.reviews[1].date = restaurant.YelpBusinessRecord.reviews[0].time_created;
                    restaurant.YelpRecord.reviews[1].user_name = restaurant.YelpBusinessRecord.reviews[0].user.name;
                    restaurant.YelpRecord.reviews[1].user_photo_url = restaurant.YelpBusinessRecord.reviews[0].user.image_url;
                    restaurant.YelpRecord.reviews[1].rating = restaurant.YelpBusinessRecord.reviews[0].rating;
                    restaurant.YelpRecord.reviews[1].text_excerpt = restaurant.YelpBusinessRecord.reviews[0].excerpt;
                }

                if (restaurant.YelpBusinessRecord.reviews.Count > 2)
                {
                    // TODO: restaurant.YelpRecord.reviews[2].date = restaurant.YelpBusinessRecord.reviews[0].time_created;
                    restaurant.YelpRecord.reviews[2].user_name = restaurant.YelpBusinessRecord.reviews[0].user.name;
                    restaurant.YelpRecord.reviews[2].user_photo_url = restaurant.YelpBusinessRecord.reviews[0].user.image_url;
                    restaurant.YelpRecord.reviews[2].rating = restaurant.YelpBusinessRecord.reviews[0].rating;
                    restaurant.YelpRecord.reviews[2].text_excerpt = restaurant.YelpBusinessRecord.reviews[0].excerpt;
                }
            }
        }

    }
}
