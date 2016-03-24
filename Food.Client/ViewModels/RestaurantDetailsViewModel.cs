using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Common;
using Microsoft.DPE.ReferenceApps.Food.Lib.Helpers;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.BingMaps;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.BingSearch;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.OpenMenu;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;

namespace Microsoft.DPE.ReferenceApps.Food.Client.ViewModels
{
    public class RestaurantDetailsViewModel : BindableBase
    {
        #region Properties

        Lib.Services.BingMaps.SearchHelper m_StaticImageryHelper;
        ImageSearchHelper m_ImageSearchHelper;
        MenuHelper m_MenuHelper;
        CrosswalkHelper m_MenuCrosswalkHelper;
        int numImages = 12;
        int numMenuGroups = 3;
        int numMenuItems = 3;

        Restaurant _restaurant;
        public Restaurant Restaurant
        {
            get { return this._restaurant; }
            set
            {
                this.SetProperty<Restaurant>(ref this._restaurant, value);
                LoadRestaurantDetails();
            }
        }

        Lib.Services.Yelp.BusinessHelper.Business _business;
        public Lib.Services.Yelp.BusinessHelper.Business Business
        {
            get { return this._business; }
            set
            {
                this.SetProperty<Lib.Services.Yelp.BusinessHelper.Business>(ref this._business, value);
            }
        }

        private Uri _mapSnapshotSource;
        public Uri MapSnapshotSource
        {
            get { return this._mapSnapshotSource; }
            set { this.SetProperty<Uri>(ref this._mapSnapshotSource, value); }
        }

        private int? _ratingPercentage;
        public int? RatingPercentage
        {
            get { return _ratingPercentage; }
            set { this.SetProperty<int?>(ref this._ratingPercentage, value); }
        }

        private string _neighborhoods;
        public string Neighborhoods
        {
            get { return _neighborhoods; }
            set { this.SetProperty<string>(ref this._neighborhoods, value); }
        }

        private IEnumerable<Lib.Services.OpenMenu.MenuHelper.Group> _selectedMenuGroups;
        public IEnumerable<Lib.Services.OpenMenu.MenuHelper.Group> SelectedMenuGroups
        {
            get { return this._selectedMenuGroups; }
            set { this.SetProperty<IEnumerable<Lib.Services.OpenMenu.MenuHelper.Group>>(ref this._selectedMenuGroups, value); }
        }

        private IEnumerable<Lib.Services.BingSearch.ImageSearchHelper.Result> _imageResults;
        public IEnumerable<Lib.Services.BingSearch.ImageSearchHelper.Result> ImageResults
        {
            get { return _imageResults; }
            set { this.SetProperty<IEnumerable<Lib.Services.BingSearch.ImageSearchHelper.Result>>(ref this._imageResults, value); }
        }

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility
        {
            get { return _progressBarVisibility; }
            set { this.SetProperty<Visibility>(ref this._progressBarVisibility, value); }
        }

        private Visibility _dealsVisibility = Visibility.Collapsed;
        public Visibility DealsVisibility
        {
            get { return _dealsVisibility; }
            set { this.SetProperty<Visibility>(ref this._dealsVisibility, value); }
        }

        private Visibility _aggregateRatingPercentageVisibility = Visibility.Collapsed;
        public Visibility AggregateRatingPercentageVisibility
        {
            get { return _aggregateRatingPercentageVisibility; }
            set { this.SetProperty<Visibility>(ref this._aggregateRatingPercentageVisibility, value); }
        }

        private Visibility _yelpRatingVisibility = Visibility.Collapsed;
        public Visibility YelpRatingVisibility
        {
            get { return _yelpRatingVisibility; }
            set { this.SetProperty<Visibility>(ref this._yelpRatingVisibility, value); }
        }

        private Visibility _factualRatingVisibility = Visibility.Collapsed;
        public Visibility FactualRatingVisibility
        {
            get { return _factualRatingVisibility; }
            set { this.SetProperty<Visibility>(ref this._factualRatingVisibility, value); }
        }

        private Visibility _factualDetailsVisibility = Visibility.Collapsed;
        public Visibility FactualDetailsVisibility
        {
            get { return _factualDetailsVisibility; }
            set { this.SetProperty<Visibility>(ref this._factualDetailsVisibility, value); }
        }

        private Visibility _menuVisibility = Visibility.Collapsed;
        public Visibility MenuVisibility
        {
            get { return _menuVisibility; }
            set { this.SetProperty<Visibility>(ref this._menuVisibility, value); }
        }

        private Visibility _addToFavoritesButtonVisibility = Visibility.Collapsed;
        public Visibility AddToFavoritesButtonVisibility
        {
            get { return _addToFavoritesButtonVisibility; }
            set { this.SetProperty<Visibility>(ref this._addToFavoritesButtonVisibility, value); }
        }

        private Visibility _removeFromFavoritesButtonVisibility = Visibility.Collapsed;
        public Visibility RemoveFromFavoritesButtonVisibility
        {
            get { return _removeFromFavoritesButtonVisibility; }
            set { this.SetProperty<Visibility>(ref this._removeFromFavoritesButtonVisibility, value); }
        }

        private Visibility _pinSecondaryTileButtonVisibility = Visibility.Collapsed;
        public Visibility PinSecondaryTileButtonVisibility
        {
            get { return _pinSecondaryTileButtonVisibility; }
            set { this.SetProperty<Visibility>(ref this._pinSecondaryTileButtonVisibility, value); }
        }

        private Visibility _unpinSecondaryTileButtonVisibility = Visibility.Collapsed;
        public Visibility UnpinSecondaryTileButtonVisibility
        {
            get { return _unpinSecondaryTileButtonVisibility; }
            set { this.SetProperty<Visibility>(ref this._unpinSecondaryTileButtonVisibility, value); }
        }

        public Visibility JSONPanelVisibility
        {
            get { return Lib.Settings.ShowJSONPanel? Visibility.Visible : Visibility.Collapsed; }
        }

        private double _maxHeight = 820d;
        public double MaxHeight
        {
            get { return this._maxHeight; }
            set { this.SetProperty<double>(ref this._maxHeight, value); }
        }

        #endregion Properties

        #region Constructors

        public RestaurantDetailsViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                //this.Restaurant = await SampleData.Restaurants.LoadAsync()[0];
                return;
            }
            else
            {
                InitVM();
            }
        }

        public RestaurantDetailsViewModel(Restaurant restaurant)
        {
            this.Restaurant = restaurant;
            InitVM();
        }

        #endregion Constructors

        #region Private Methods

        private void InitVM()
        {
            this.m_ImageSearchHelper =
                new ImageSearchHelper(Lib.Settings.BingSearchCustomerID, Lib.Settings.BingSearchKey);

            this.m_MenuCrosswalkHelper =
                new CrosswalkHelper(Lib.Settings.OpenMenuKey);

            this.m_MenuHelper =
                new MenuHelper(Lib.Settings.OpenMenuKey);

            AppData.Current.Favorites.CollectionChanged += (s, e) => CheckFavs();
            CheckSize();

            Window.Current.SizeChanged += Current_SizeChanged;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            CheckSize();
            LoadRestaurantDetails();
        }

        private void CheckSize()
        {
            if (Window.Current.Bounds.Height >= 1080)
            {
                numMenuGroups = 3;
                numMenuItems = 3;
                numImages = 18;
                this.MaxHeight = 820d;
            }
            else
            {
                numMenuGroups = 2;
                numMenuItems = 3;
                numImages = 12;
                this.MaxHeight = 508d;
            }
                
        }

        private void CheckFavs()
        {
            this.AddToFavoritesButtonVisibility =
                AppData.Current.IsInFavorites(this.Restaurant) ? Visibility.Collapsed : Visibility.Visible;
            this.RemoveFromFavoritesButtonVisibility =
                AppData.Current.IsInFavorites(this.Restaurant) ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void LoadRestaurantDetails()
        {
            ProgressBarVisibility = Visibility.Visible;

            if (null != this.Restaurant)
            {
                this.AddToFavoritesButtonVisibility =
                    AppData.Current.IsInFavorites(this.Restaurant) ? Visibility.Collapsed : Visibility.Visible;
                this.RemoveFromFavoritesButtonVisibility =
                    AppData.Current.IsInFavorites(this.Restaurant) ? Visibility.Visible : Visibility.Collapsed;

                // Set visibility for pin/unpin secondary tile
                this.PinSecondaryTileButtonVisibility = SecondaryTile.Exists(this.Restaurant.Key) ? Visibility.Collapsed : Visibility.Visible;
                this.UnpinSecondaryTileButtonVisibility = SecondaryTile.Exists(this.Restaurant.Key) ? Visibility.Visible : Visibility.Collapsed;

                if (!this.Restaurant.FactualRecordLoaded)
                {
                    await Unity.FillFactualAsync(this.Restaurant);
                }

                if (this.Restaurant.FactualRecordLoaded && null != this.Restaurant.FactualRecord)
                {
                    this.FactualDetailsVisibility = Visibility.Visible;
                    this.FactualRatingVisibility = Visibility.Visible;

                    if (!this.Restaurant.OpenmenuRecordLoaded)
                    {
                        await Unity.FillOpenMenuAsync(this.Restaurant);
                    }

                    this.MenuVisibility = (null != this.Restaurant.OpenmenuRecord &&
                        null != this.Restaurant.OpenmenuRecord.Menus &&
                        this.Restaurant.OpenmenuRecord.Menus.Count > 0) ? Visibility.Visible : Visibility.Collapsed;

                    if (this.Restaurant.OpenmenuRecordLoaded &&
                        null != this.Restaurant.OpenmenuRecord &&
                        null != this.Restaurant.OpenmenuRecord.Menus &&
                        this.Restaurant.OpenmenuRecord.Menus.Count > 0)
                    {
                        IEnumerable<Lib.Services.OpenMenu.MenuHelper.Group> groups =
                            this._restaurant.OpenmenuRecord.Menus[0].Groups.Take(numMenuGroups);
                        foreach (Lib.Services.OpenMenu.MenuHelper.Group group in groups)
                        {
                            group.Items = group.Items.Take(numMenuItems).ToList();
                        }

                        this.SelectedMenuGroups = groups;
                    }
                }
                else
                {
                    this.FactualDetailsVisibility = Visibility.Collapsed;
                    this.FactualRatingVisibility = Visibility.Collapsed;
                }

                if (this.Restaurant.YelpRecordLoaded && !this.Restaurant.YelpBusinessRecordLoaded)
                {
                    var bizHelper = new Lib.Services.Yelp.BusinessHelper(
                        Lib.Settings.YelpConsumerKey,
                        Lib.Settings.YelpConsumerSecret,
                        Lib.Settings.YelpToken,
                        Lib.Settings.YelpTokenSecret);

                    try
                    {
                        this.Restaurant.YelpBusinessRecord = await bizHelper.SearchAsync(this.Restaurant.YelpRecord.id);
                    }
                    catch (Lib.Services.Exceptions.FoodLibServiceException e)
                    {
                    }
                }

                if (null != this.Restaurant.Deals &&
                    this.Restaurant.Deals.Count > 0)
                    this.DealsVisibility = Visibility.Visible;
                else
                    this.DealsVisibility = Visibility.Collapsed;


                if (null == this.MapSnapshotSource)
                {
                    if (null != this.Restaurant && 
                        this.Restaurant.YelpRecordLoaded && 
                        null != this.Restaurant.YelpRecord)
                    {
                        this.MapSnapshotSource = 
                            Unity.GetMapUri(Restaurant, 
                                300,
                                300,
                                string.Empty, 
                                null, 
                                null, 
                                null,
                                Lib.Services.BingMaps.SearchHelper.StaticMapIconStyles.OrangeBoxWithArrow);

                        if (null != Restaurant.YelpRecord.neighborhoods &&
                            Restaurant.YelpRecord.neighborhoods.Count > 0)
                        {
                            IEnumerable<Lib.Services.Yelp.V1Helper.Neighborhood> neighborhoods =
                                Restaurant.YelpRecord.neighborhoods.Where(n => !string.IsNullOrWhiteSpace(n.name));

                            this.Neighborhoods = neighborhoods.Count() > 0 ?
                                neighborhoods.Select(n => n.name).Aggregate((a, b) => a + ", " + b) :
                                string.Empty;
                        }
                    }

                    if (null == this.ImageResults)
                    {
                        if (null != this.Restaurant)
                        {
                            // BUG: exception thrown here when offline
                            var results = await this.m_ImageSearchHelper.GetImages(this.Restaurant, 40);
                            if (null != results && results.Count() > 0)
                                results = results.Where(r => r.Width >= 250 && r.Height >= 200);
                            if (null != results && results.Count() > 0)
                                this.ImageResults = results.Take(numImages);
                        }
                    }
                }

                if (!this.RatingPercentage.HasValue)
                {
                    if (null != Restaurant && null != Restaurant.YelpRecord)
                    {
                        double avgRating = double.NaN;
                        if (Restaurant.YelpRecord.avg_rating.HasValue &&
                            null != Restaurant.FactualRecord &&
                            Restaurant.FactualRecord.rating.HasValue)
                        {
                            avgRating =
                                (Restaurant.YelpRecord.avg_rating.Value +
                                Restaurant.FactualRecord.rating.Value) / 2;
                        }
                        else if (Restaurant.YelpRecord.avg_rating.HasValue)
                        {
                            avgRating = Restaurant.YelpRecord.avg_rating.Value;
                        }
                        else if (null != Restaurant.FactualRecord &&
                           Restaurant.FactualRecord.rating.HasValue)
                        {
                            avgRating = Restaurant.FactualRecord.rating.Value;
                        }

                        if (null == Restaurant.YelpRecord ||
                            !Restaurant.YelpRecord.avg_rating.HasValue)
                        {
                            this.YelpRatingVisibility = Visibility.Collapsed;
                        }
                        else
                        {
                            this.YelpRatingVisibility = Visibility.Visible;
                        }

                        if (double.NaN != avgRating)
                        {
                            this.RatingPercentage = (int)Math.Round((1 - ((5 - avgRating) / 5)) * 100, 0);
                            this.AggregateRatingPercentageVisibility = Visibility.Visible;
                        }
                        else
                        {
                            this.AggregateRatingPercentageVisibility = Visibility.Collapsed;
                        }
                    }
                }
            }

            ProgressBarVisibility = Visibility.Collapsed;
        }

        #endregion Private Methods
    }
}
