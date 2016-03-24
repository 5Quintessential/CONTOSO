using Bing.Maps;
using Microsoft.DPE.ReferenceApps.Food.Client.Controls;
using Microsoft.DPE.ReferenceApps.Food.Client.Details;
using Microsoft.DPE.ReferenceApps.Food.Client.ViewModels;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Microsoft.DPE.ReferenceApps.Food.Client.Sections
{
    public sealed partial class MapPage : Microsoft.DPE.ReferenceApps.Food.Client.Common.LayoutAwarePage
    {
        private int m_restaurantID;
        System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();
        readonly int m_BingMapsInitialZoomLevel = Lib.Settings.BingMapsInitialZoomLevel;
        LocationCollection m_rLocations = new LocationCollection();
        private RestaurantCustomBalloon m_TappedBalloon;
        private List<Location> clusteredLocs = null;
        private RestaurantCollectionViewModel _viewModel;
        private List<MapLayer> clusteredPinAddedLayers = new List<MapLayer>();
        private ClusteredPinControl lastTappedClusteredPin = null;
        private CollectionViewSource _cvsRestaurants = null;

        public RestaurantCollectionViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                _viewModel.CurrentFilter.ShowOrderBy = false;
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        /// <summary>
        /// Handles the view model's PropertyChangedEventArgs to change the data source to the FilteredCollection if present
        /// </summary>
        /// <param name="sender">the CollectionViewModel</param>
        /// <param name="e">the PropertyChangedEventArgs</param>
        void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FilteredCollection":
                    if (_cvsRestaurants != null && _viewModel.FilteredCollection != null)
                    {
                        _cvsRestaurants.Source = ViewModel.FilteredCollection;
                    }
                    break;
                default:
                    break;
            }
        }

        public MapPage()
        {
            this.InitializeComponent();

            // hide TopAppBar 
            var _Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _Timer.Tick += (s, e) =>
            {
                _Timer.Stop();
                this.TopAppBar.IsOpen = false;
            };
            _Timer.Start();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (navigationParameter is Tuple<string, IEnumerable<Restaurant>, bool>)
            {
                Tuple<string, IEnumerable<Restaurant>, bool> navparams = navigationParameter as Tuple<string, IEnumerable<Restaurant>, bool>;

                // Title of collection
                pageTitle.Text = navparams.Item1;

                // Restuarant collection to display as pins on the map
                this.ViewModel = new RestaurantCollectionViewModel(navparams.Item2, false);
                if (null != ViewModel)
                {
                    ViewModel.ApplyOptions();
                    this.RestaurantFilterFlyoutControl.CurrentData = this.ViewModel.CurrentFilter;
                    await FillMapWithDataAsync();
                }
            }
        }

        /// <summary>
        /// Entry point for creating map pins for the current Restauarant collection
        /// </summary>
        /// <returns>a waitable Task</returns>
        private async Task FillMapWithDataAsync()
        {
            Location _location = new Location();

            mapControl.Credentials = Lib.Settings.BingMapsKey;

            try
            {
                _location.Latitude = AppData.Current.Latitude ?? 0d;
                _location.Longitude = AppData.Current.Longitude ?? 0d;
                AddUserLocationToMap(_location);
                AddRestaurantsToMap();
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                new MessageDialog(ex.Message, "Error").ShowAsync();
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Adds map pins for each Restauarnt in the current Restauarant collection
        /// </summary>
        private void AddRestaurantsToMap()
        {
            MapLayer _layer;

            if (0 != ViewModel.FilteredCollection.Count())
                m_rLocations.Clear();

            m_restaurantID = ViewModel.FilteredCollection.Count();

            if (ViewModel.FilteredCollection != null)
            {

                // Foreach restaurant in the current collection, creates instance of RatauarntBalloon and adds it to 
                // a new MapLayer which in turn added to the map.
                foreach (Restaurant _restaurant in ViewModel.FilteredCollection.Reverse<Restaurant>())
                {
                    if (_restaurant.YelpRecordLoaded)
                    {
                        _layer = addRestaurantBalloonLayer(_restaurant);
                        mapControl.Children.Add(_layer);
                        m_restaurantID--;
                    }
                }

                // No Restauarants in the list.  Perhaps the filter is too restrictive?
                if (0 == ViewModel.FilteredCollection.Count)
                {
                    MessageDialog md = new MessageDialog("No restaurants to display based on current filter.", "None found");
                    md.ShowAsync();
                }
            }

            // Collapse all balloons (needed if the map is already displaying balloons)
            CollapseAllBalloonsAndSetOpacity(0.3d);
            FloatFirstToTop();
            ZoomAroundLocationsAsync();

        }
        
        /// <summary>
        /// Zooms the map around the current map pins
        /// </summary>
        private async void ZoomAroundLocationsAsync()
        {
            Location nw = new Location(
                m_rLocations.Max((l) => l.Latitude),
                m_rLocations.Min((l) => l.Longitude));

            Location se = new Location(
                m_rLocations.Min((l) => l.Latitude),
                m_rLocations.Max((l) => l.Longitude));

            LocationRect bounds = new LocationRect(nw, se);

            mapControl.SetView(bounds, new TimeSpan(0, 0, 0, 0, 900));
            await Task.Delay(1100);
            mapControl.SetZoomLevel(mapControl.ZoomLevel + 0.25);
            await Task.Delay(100);
            this.ClusterPinsAsync();
        }

        /// <summary>
        /// Inner IEqualityComparer used to compare Locations 
        /// </summary>
        private class ClusteredLocationEqualityComparer : IEqualityComparer<Location>
        {
            public bool Equals(Location x, Location y)
            {
                return x.Latitude.Equals(y.Latitude) &&
                    x.Longitude.Equals(y.Longitude);
            }

            public int GetHashCode(Location obj)
            {
                return obj.Latitude.GetHashCode() + obj.Longitude.GetHashCode();
            }
        }

        /// <summary>
        /// Looks for map pins within the radius defined by Lib.Settings.PinClusterPixelRadius and then groups them into a "cluster".
        /// If a "cluster" can contain a maximum of five pins.
        /// </summary>
        private async void ClusterPinsAsync()
        {
            if (Lib.Settings.MaxPinsPerCluster <= mapControl.Children.Count)
            {
                // Dictionary to hold pin clusters
                Dictionary<Location, List<Restaurant>> pinClusters =
                    new Dictionary<Location, List<Restaurant>>();

                clusteredLocs = new List<Location>();

                Location nw, se;
                Point nwPoint, sePoint, locPoint;
                LocationRect bounds;

                // Get a list of Location objects from the view model's FilteredCollection
                IEnumerable<Location> locations =
                    ViewModel.FilteredCollection.Select<Restaurant, Location>(r => new Location { Latitude = r.Latitude, Longitude = r.Longitude });

                // Clear out the pins from the map
                mapControl.Children.Clear();
                IEnumerable<Location> otherLocs;
                IEnumerable<Restaurant> nearby;
                ClusteredLocationEqualityComparer comparer = new ClusteredLocationEqualityComparer();

                List<Restaurant> clusteredRestaurants;

                foreach (Location rLoc in locations)
                {
                    // If the current Location has not already been clustered
                    if (!clusteredLocs.Contains(rLoc, comparer))
                    {
                        // Get the pixel location for the Location
                        if (mapControl.TryLocationToPixel(rLoc, out locPoint))
                        {
                            nwPoint = locPoint;
                            sePoint = locPoint;

                            // Create the point a logical rectangle around the location using Lib.Settings.PinClusterPixelRadius
                            nwPoint.X -= Lib.Settings.PinClusterPixelRadius;
                            nwPoint.Y -= Lib.Settings.PinClusterPixelRadius;
                            sePoint.X += Lib.Settings.PinClusterPixelRadius;
                            sePoint.Y += Lib.Settings.PinClusterPixelRadius;

                            // Convert the pixel locations to actual Locations
                            if (mapControl.TryPixelToLocation(nwPoint, out nw) &&
                                    mapControl.TryPixelToLocation(sePoint, out se))
                            {
                                // Create the LocationRect using the NW and SE corners
                                bounds = new LocationRect(nw, se);

                                // Find all of the Locations in the LocationRect and are not already clustered
                                otherLocs = locations.Where(l => !comparer.Equals(l, rLoc) &&
                                    !clusteredLocs.Contains(l, comparer) &&
                                    bounds.Contains(l)).Take(Lib.Settings.MaxPinsPerCluster - 1);

                                nearby =
                                    ViewModel.FilteredCollection.Where(r =>
                                        otherLocs.Contains(new Location { Latitude = r.Latitude, Longitude = r.Longitude }, comparer));

                                // Add the newly clustered Locations to the dictionary
                                clusteredRestaurants = new List<Restaurant>();
                                clusteredRestaurants.Add(
                                    ViewModel.FilteredCollection.FirstOrDefault(r => r.Latitude == rLoc.Latitude && r.Longitude == rLoc.Longitude));
                                clusteredLocs.Add(rLoc);

                                if (null != nearby && 0 < nearby.Count())
                                {
                                    clusteredRestaurants.AddRange(nearby);
                                    clusteredLocs.AddRange(otherLocs);
                                }

                                pinClusters.Add(rLoc, clusteredRestaurants); ;
                            }
                        }
                    }
                }

                ClusteredPinControl clusteredPin;
                RestaurantCustomBalloon balloon;
                MapLayer newLayer;

                foreach (Location keyLoc in pinClusters.Keys)
                {
                    newLayer = new MapLayer();

                    if (1 < pinClusters[keyLoc].Count)
                    {
                        // Create a new ClusteredPinControl to repesent the cluster
                        clusteredPin = new ClusteredPinControl();
                        clusteredPin.Restaurants = pinClusters[keyLoc];
                        clusteredPin.Tapped += ClusteredPin_Tapped;

                        // Seets the ClusteredPinControl Location to the center of the custered Locations
                        clusteredPin.ZoomToLocationAsync(mapControl, TimeSpan.MinValue);

                        clusteredLocs.Add(clusteredPin.Location);

                        // Add the ClusteredPinControl to the map
                        newLayer.Children.Add(clusteredPin);
                    }
                    else
                    {
                        // If a Cluster only has one Location, convert it back to a RestaurantCustomBalloon
                        balloon = new RestaurantCustomBalloon(Colors.White);

                        balloon.Tapped += Balloon_Tapped;

                        balloon.Restaurant = pinClusters[keyLoc][0];
                        balloon.AverageRating = pinClusters[keyLoc][0].AverageRating ?? 0d;

                        Location rLocation = new Location(pinClusters[keyLoc][0].Latitude, pinClusters[keyLoc][0].Longitude);
                        this.m_rLocations.Add(rLocation);
                        MapLayer.SetPosition(balloon, rLocation);
                        newLayer.Children.Add(balloon);
                    }

                    mapControl.Children.Add(newLayer);
                }

                if (1 < pinClusters.Count)
                {
                    // Sort the clustered locations by Latitude and get the "median" value
                    var sortedLocs = pinClusters.Keys.OrderBy(l => l.Longitude).OrderBy(l => l.Latitude);
                    Location median = sortedLocs.ElementAt(sortedLocs.Count() / 2);

                    int medianIndex = pinClusters.Keys.ToList().IndexOf(median);

                    // Bubble the "median" Cluster to the top of the map
                    MapLayer ml = mapControl.Children[medianIndex] as MapLayer;
                    ClusteredPinControl highlightedClusteredPin = ml.Children[0] as ClusteredPinControl;
                    mapControl.Children.Remove(ml);
                    mapControl.Children.Add(ml);
                }

                await ZoomToClusteredLocsAsync();
            }
            else
            {
                mapControl.SetZoomLevel(mapControl.ZoomLevel - 0.6);
            }
        }

        /// <summary>
        /// Zooms the map around the ClusteredPins
        /// </summary>
        /// <returns>an awaitable Task</returns>
        private async Task ZoomToClusteredLocsAsync()
        {
            // Resets the opacity on all of the non clustered RestaurantCustomBalloons
            this.ResetBalloonOpacity();

            if (0 < clusteredLocs.Count)
            {
                Location nw, se;

                //  Get the furthest NW location in the cluster
                nw = new Location(
                    clusteredLocs.Max((l) => l.Latitude),
                    clusteredLocs.Min((l) => l.Longitude));
                
                //  Get the furthest SE location in the cluster
                se = new Location(
                    clusteredLocs.Min((l) => l.Latitude),
                    clusteredLocs.Max((l) => l.Longitude));

                // Draw a LocationRect using the NEW and SE points
                LocationRect bounds = new LocationRect(nw, se);

                // Zoom using the LocationRect
                mapControl.SetView(bounds, new TimeSpan(0, 0, 0, 0, 800));
                await Task.Delay(900);

                // Zoom out a bit more to account for size of map objects
                mapControl.SetZoomLevel(mapControl.ZoomLevel - 0.6);
            }
        }

        /// <summary>
        /// Zooms to the center of a pin cluster and shows RestaurantCustomBalloons for each clustered Restaurant
        /// </summary>
        /// <param name="sender">the clicked / tapped ClusteredPinControl</param>
        /// <param name="e">the TappedRoutedEventArgs</param>
        private async void ClusteredPin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Clear added layers from previous ClusteredPinControl activation
            foreach (MapLayer addedLayer in clusteredPinAddedLayers)
                mapControl.Children.Remove(addedLayer);

            clusteredPinAddedLayers.Clear();

            ClusteredPinControl clusteredPin = sender as ClusteredPinControl;

            clusteredPin.Opacity = 1.0;

            // If this ClusteredPinControl was previously activated, "Collapse" it
            if (null != lastTappedClusteredPin &&
                lastTappedClusteredPin == clusteredPin)
            {
                lastTappedClusteredPin = null;
                await ZoomToClusteredLocsAsync();
            }
            else
            {
                lastTappedClusteredPin = clusteredPin;
                List<Restaurant> restaurants = clusteredPin.Restaurants;

                MapLayer layer;

                // Foreach Restauaarnt in the cluster, create a RestaurantCustomBalloon and add it to the map
                foreach (Restaurant restaurant in clusteredPin.Restaurants)
                {
                    layer = addRestaurantBalloonLayer(restaurant);
                    clusteredPinAddedLayers.Add(layer);
                    mapControl.Children.Add(layer);
                }

                MapLayer clusteredPinLayer = clusteredPin.Parent as MapLayer;
                mapControl.Children.Remove(clusteredPinLayer);
                mapControl.Children.Add(clusteredPinLayer);

                // Zoom to the ceneter of the cluster
                clusteredPin.ZoomToLocationAsync(mapControl, TimeSpan.FromMilliseconds(500d), true);
            }
        }

        /// <summary>
        /// Adds the User's current location to the map
        /// </summary>
        /// <param name="location">the user's current location</param>
        private void AddUserLocationToMap(Location location)
        {
            Bing.Maps.Pushpin _pushpin = new Bing.Maps.Pushpin();
            MapLayer.SetPosition(_pushpin, location);
            MapLayer _layer = new MapLayer();
            _layer.Children.Add(_pushpin);
            mapControl.Children.Add(_layer);
        }

        /// <summary>
        ///  Adds a RestaurantCustomBalloon representing the passed Restaurant to a new MapLayer
        /// </summary>
        /// <param name="restaurant">the Restauarant to add to the map</param>
        /// <returns>a MapLayer containing a RestaurantCustomBalloon representing the Restaurant</returns>
        private MapLayer addRestaurantBalloonLayer(Restaurant restaurant)
        {
            RestaurantCustomBalloon _balloon = new RestaurantCustomBalloon(Colors.White);

            _balloon.Tapped += Balloon_Tapped;

            _balloon.Restaurant = restaurant;
            _balloon.RestaurantID = m_restaurantID;
            _balloon.AverageRating = restaurant.AverageRating ?? 0d;

            Location rLocation = new Location(restaurant.YelpRecord.latitude, restaurant.YelpRecord.longitude);
            m_rLocations.Add(rLocation);
            MapLayer.SetPosition(_balloon, rLocation);
            MapLayer layer = new MapLayer();
            layer.Children.Add(_balloon);
            return layer;
        }

        /// <summary>
        /// If there are less than five RestauarantCustomBallons on the map or if the tapped / clicked RestaurantCustomBalloon 
        /// was the last tapped / clicked this will navigate to  the RestaurantDetails page passing the associated Restaurant.
        /// If there are more than five RestauarntCustomBallons on the map and the tapped / clicked RestauarantCustomBallon was 
        /// not the last tapped / clicked, the tapped / clicked RestauarantCustomBallon will be "floated" to the top and the other 
        /// RestauarantCustomBallons will have their opacity diminished.
        /// </summary>
        /// <param name="sender">the tapped / clicked RestaurantCustomBalloon</param>
        /// <param name="e">the TappedRoutedEventArgs</param>
        private void Balloon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RestaurantCustomBalloon _balloon = sender as RestaurantCustomBalloon;
            if (this.m_TappedBalloon == _balloon) // If this balloon was the last balloon tapped
            {
                this.Frame.Navigate(typeof(RestaurantDetailsPage), _balloon.Restaurant);

            }
            else if (this.lastTappedClusteredPin != null && 
                this.lastTappedClusteredPin.Restaurants.Contains(_balloon.Restaurant, new RestauarantEqualityComparer())) // Make sure there is a clusteredPin and the clusteredPin contains this restauarant
            {
                this.ChangeUntappedBalloonIntoTappedBalloon(_balloon);
            }
            else // There is no clusteredPin count (it's null)
            {
                this.Frame.Navigate(typeof(RestaurantDetailsPage), _balloon.Restaurant);
            }
        }

        /// <summary>
        /// Changes the context tapped / clicked RestaurantCustomBalloon so that the next time it is tapped 
        /// we will navigate to  the RestaurantDetails page passing the associated Restaurant.
        /// </summary>
        /// <param name="balloon">the RestaurantCustomBalloon to change the context for</param>
        private void ChangeUntappedBalloonIntoTappedBalloon(RestaurantCustomBalloon balloon)
        {
            m_TappedBalloon = balloon;

            MapLayer balloonLayer = null;
            bool found = false;

            // Find the RestaurantCustomBalloon in the control tree
            foreach (var item in mapControl.Children)
            {
                if (item.GetType() == typeof(MapLayer))
                {
                    balloonLayer = (MapLayer)item;
                    foreach (var obj in balloonLayer.Children)
                    {
                        if (obj.GetType() == typeof(RestaurantCustomBalloon))
                        {
                            var b = obj as RestaurantCustomBalloon;
                            if (balloon == b)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (found)
                        break;
                }
            }

            // Brings this to top of stacked balloons
            if (found && null != balloonLayer)
            {
                mapControl.Children.Remove(balloonLayer);
                mapControl.Children.Add(balloonLayer);
            }

            // Diminish the opacity on all of the other balloons 
            CollapseAllBalloonsAndSetOpacity(0.3d);

            // Set the opacity on this ballon to full
            balloon.Opacity = 1.0d;
        }

        /// <summary>
        /// Removes the first added RestaurantCustomBalloon and readds it so that it will be 
        /// on top of all of the others
        /// </summary>
        private void FloatFirstToTop()
        {
            if (mapControl.Children.Count > 0)
            {
                var item = mapControl.Children[mapControl.Children.Count - 1];

                if (item.GetType() == typeof(MapLayer))
                {
                    var layer = (MapLayer)item;
                    foreach (var obj in layer.Children)
                    {
                        if (obj.GetType() == typeof(RestaurantCustomBalloon))
                        {
                            m_TappedBalloon = obj as RestaurantCustomBalloon;
                            m_TappedBalloon.Opacity = 1d;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the opacity to all RestaurantCustomBalloons to the passed opacity value
        /// </summary>
        /// <param name="opacity">the opacity to set the baloons to</param>
        private void CollapseAllBalloonsAndSetOpacity(double opacity)
        {
            foreach (var item in mapControl.Children)
            {
                if (item.GetType() == typeof(MapLayer))
                {
                    var layer = (MapLayer)item;
                    foreach (var obj in layer.Children)
                    {
                        if (obj.GetType() == typeof(RestaurantCustomBalloon))
                        {
                            var baloon = obj as RestaurantCustomBalloon;
                            baloon.Opacity = opacity;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resests all RestaurantCustomBalloon to full opacity
        /// </summary>
        private void ResetBalloonOpacity()
        {
            foreach (var item in mapControl.Children)
            {
                if (item.GetType() == typeof(MapLayer))
                {
                    var layer = (MapLayer)item;
                    foreach (var obj in layer.Children)
                    {
                        if (obj.GetType() == typeof(RestaurantCustomBalloon))
                        {
                            var baloon = obj as RestaurantCustomBalloon;
                            baloon.Opacity = 1d;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the filter flyout
        /// </summary>
        /// <param name="sender">the Filter button</param>
        /// <param name="e">the TappedRoutedEventArgs</param>
        private void appbarbtnFilter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            new Lib.Helpers.FlyoutHelper().Show(this.RestaurantFilterFlyout, sender as Button, 50);

            this.RestaurantFilterFlyout.Closed += FlyoutFilterClosed;
            this.RestaurantFilterFlyout.IsOpen = true;
        }

        /// <summary>
        /// When the filter flyout is closed this handler will filter the current collection based on 
        /// the selections of the user
        /// </summary>
        /// <param name="sender">the Filter flyout</param>
        /// <param name="e">ignored</param>
        async void FlyoutFilterClosed(object sender, object e)
        {
            if (ViewModel.CurrentFilter.Dirty)
            {
                ViewModel.ApplyOptions();
                mapControl.Children.Clear();
                await FillMapWithDataAsync();
            }
        }
    }
}
