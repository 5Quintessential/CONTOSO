using Microsoft.DPE.ReferenceApps.Food.Client.Common;
using Microsoft.DPE.ReferenceApps.Food.Client.ViewModels;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Microsoft.DPE.ReferenceApps.Food.Client.Sections
{
    public sealed partial class RestaurantList : LayoutAwarePage
    {
        RestaurantListViewModel m_RestaurantListViewModel;

        public RestaurantList()
        {
            this.InitializeComponent();

            // Handle the SizeChanged event on the Window to appropriately deal with x768 and x1080 resolutions
            this.CheckSize();
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        /// <summary>
        /// Handles a SizeChanged event for the purpose of appropriately dealing with x768 and x1080 resolutions 
        /// and removing the bottom app bar when snapped.
        /// </summary>
        /// <param name="sender">The object firing SizeChanged</param>
        /// <param name="e">the WindowSizeChangeEventArgs</param>
        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.CheckSize();
        }

        /// <summary>
        /// Checks the current Window height to appropriately deal with x768 and x1080 resolutions
        /// </summary>
        private void CheckSize()
        {
            if (Window.Current.Bounds.Height >= 1080)
            {
                this.RestaurantsSemanticZoom.Height = 940;
                this.ResultsVerticalScroller.Height = 940;
            }
            else
            {
                this.RestaurantsSemanticZoom.Height = 628;
                this.ResultsVerticalScroller.Height = 628;
            }
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
            this.NoResultsTextBlock.Visibility = Visibility.Collapsed;
            if (null != navigationParameter && navigationParameter is RestaurantListViewModel)
            {
                // Communicate results through the view model
                this.DataContext = this.m_RestaurantListViewModel = navigationParameter as RestaurantListViewModel;

                if (RestaurantListViewModel.RestaurantListTypes.SearchResults == this.m_RestaurantListViewModel.ListType)
                {
                    this.m_RestaurantListViewModel.ListName = "Search Results for: ";
                    this.NoResultsTextBlock.Visibility = Visibility.Collapsed;
                    this.SearchRing.Visibility = Visibility.Visible;

                    if (null != AppData.Current.Restaurants &&
                        AppData.Current.Restaurants.Count > 0)
                    {
                        await LoadRestaurants();

                        IEnumerable<Restaurant> restaurants =
                            this.ExecSearch(this.m_RestaurantListViewModel.SearchText);

                        if (null != restaurants && restaurants.Count() > 0)
                        {
                            this.m_RestaurantListViewModel.RestaurantList =
                                new System.Collections.ObjectModel.ObservableCollection<Restaurant>(restaurants);

                            this.m_RestaurantListViewModel.CollectionViewModel =
                                new RestaurantCollectionViewModel(this.m_RestaurantListViewModel.RestaurantList);

                            VisualStateManager.GoToState(this, "ResultsFound", true);
                        }
                        else
                        {
                            VisualStateManager.GoToState(this, "NoResultsFound", true);
                        }
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "NoResultsFound", true);
                    }

                    this.m_RestaurantListViewModel.SubTitle = string.Format("\"{0}\"",
                        this.m_RestaurantListViewModel.SearchText.ToUpperInvariant());

                    this.SearchRing.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.m_RestaurantListViewModel.CollectionViewModel =
                        new RestaurantCollectionViewModel(this.m_RestaurantListViewModel.RestaurantList);

                    if (RestaurantListViewModel.RestaurantListTypes.Favorites == this.m_RestaurantListViewModel.ListType)
                    {
                        this.ResultsHorizontal.SelectionMode = ListViewSelectionMode.Multiple;
                        AppData.Current.FavoritesChanged += AppData_FavoritesChanged;
                    }
                }

                if (null != this.m_RestaurantListViewModel.CollectionViewModel)
                {
                    this.m_RestaurantListViewModel.CollectionViewModel.CurrentFilter.ShowOrderBy = true;

                    this.m_RestaurantListViewModel.CollectionViewModel.PropertyChanged += CollectionViewModel_PropertyChanged;
                    this.m_RestaurantListViewModel.CollectionViewModel.ApplyOptions();

                    this.RestaurantFilterFlyoutControl.CurrentData = this.m_RestaurantListViewModel.CollectionViewModel.CurrentFilter;
                }

            }
        }

        /// <summary>
        /// Overridden to clear out the ReasonFound text
        /// </summary>
        /// <param name="e">ignored</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            foreach (Restaurant r in this.m_RestaurantListViewModel.RestaurantList)
                r.ReasonFound = null;

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Handler to handle the favorites changed event.  When the user favorites change, if the user favorites 
        /// is the list being viewed, we'll need to update the list.
        /// </summary>
        /// <param name="sender">AppData.Current</param>
        /// <param name="e">ignored</param>
        private void AppData_FavoritesChanged(object sender, EventArgs e)
        {
            if (RestaurantListViewModel.RestaurantListTypes.Favorites == this.m_RestaurantListViewModel.ListType)
                this.RefreshFavorites();
        }

        /// <summary>
        /// Ensure that each Restaurant has the Menu loaded
        /// </summary>
        /// <returns>a waitable Task</returns>
        private async Task LoadRestaurants()
        {
            if (!this.m_RestaurantListViewModel.RestauarantsLoaded)
            {
                foreach (Restaurant restaurant in AppData.Current.Restaurants)
                {
                    if (!restaurant.OpenmenuRecordLoaded)
                        await Unity.FillOpenMenuAsync(restaurant);
                }

                this.m_RestaurantListViewModel.RestauarantsLoaded = true;
            }
        }

        /// <summary>
        /// Search for Restaurants in the following manner: first by name, then by cuisine then by menu item
        /// </summary>
        /// <param name="queryText">the text to search for</param>
        /// <returns>an IEnumerable of Restaurant objects matching the queryText</returns>
        private IEnumerable<Restaurant> ExecSearch(string queryText)
        {
            queryText = queryText.ToLowerInvariant();

            if (null != AppData.Current.Restaurants &&
                AppData.Current.Restaurants.Count > 0)
            {
                return AppData.Current.Restaurants.Where(r =>
                        NameMatches(r, queryText) ||
                        CuisineContainsText(r, queryText) ||
                        MenuContainsText(r, queryText));
            }

            return null;
        }

        /// <summary>
        /// Compares the Restaurant's name against the queryText and if it matches returns true 
        /// otherwise returns false
        /// </summary>
        /// <param name="restaurant">the Restauarant to match</param>
        /// <param name="queryText">the text to search for</param>
        /// <returns>bool representing whether the Restaurant's name matches the text</returns>
        private bool NameMatches(Restaurant restaurant, string queryText)
        {
            bool textInName = restaurant.Name.ToLowerInvariant().Contains(queryText);
            if (textInName)
                restaurant.ReasonFound = "MATCHES NAME";

            return textInName;
        }

        /// <summary>
        /// Compares the Restaurant's list of cuisines against the queryText and if a match is found returns true 
        /// otherwise returns false
        /// </summary>
        /// <param name="restaurant">the Restauarant to match</param>
        /// <param name="queryText">the text to search for</param>
        /// <returns>bool representing whether the Restaurant's list of cuisines contains the text</returns>
        private bool CuisineContainsText(Restaurant restaurant, string queryText)
        {
            bool textInCusine =
                (null != restaurant.FactualRecord &&
                null != restaurant.FactualRecord.cuisine &&
                    restaurant.FactualRecord.cuisine.ToLowerInvariant().Contains(queryText));

            if (textInCusine)
                restaurant.ReasonFound = "MATCHES CUISINE";

            return textInCusine;
        }

        /// <summary>
        /// Compares the Restaurant's menu against the queryText and if a match is found returns true 
        /// otherwise returns false
        /// </summary>
        /// <param name="restaurant">the Restauarant to match</param>
        /// <param name="queryText">the text to search for</param>
        /// <returns>bool representing whether the Restaurant's menu contains the text</returns>
        private bool MenuContainsText(Restaurant restaurant, string queryText)
        {
            bool textInMenu =
                (null != restaurant.OpenmenuRecord &&
                null != restaurant.OpenmenuRecord.Menus.FirstOrDefault(m => 
                    m.Name.ToLowerInvariant().Contains(queryText) ||
                    null != m.Groups.FirstOrDefault(g =>
                        null != g.Items.FirstOrDefault(i =>
                            i.Name.ToLowerInvariant().Contains(queryText) ||
                            i.Description.ToLowerInvariant().Contains(queryText)))));

            if (textInMenu)
                restaurant.ReasonFound = "MATCHES MENU";

            return textInMenu;
        }

        /// <summary>
        /// Navigates to the RestaurantDetails page passing the Restuarant
        /// </summary>
        /// <param name="sender">the clicked / tapped Restaurant</param>
        /// <param name="e">the ItemClickEventArgs</param>
        private void Restaurant_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(Details.RestaurantDetailsPage), e.ClickedItem as Lib.Models.Restaurant);
        }

        /// <summary>
        /// Results the results when ViewAll is clicked / tapped in the Semantic Zoom's zoomed out view
        /// </summary>
        /// <param name="sender">the clicked / tapped Restaurant Group</param>
        /// <param name="e">ignored </param>
        private void RestaurantGrouping_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag;
            if(tag is string && "VIEWALL" == tag.ToString())
            {
                this.ZoomedOutViewAllItem.Visibility = Visibility.Collapsed;
                this.m_RestaurantListViewModel.GroupedRestaurantList = null;
                this.m_RestaurantListViewModel.GroupingEnabled = false;
                this.ResultsHorizontal.ItemsSource = this.m_RestaurantListViewModel.CollectionViewModel.FilteredCollection;
                this.ResultsVertical.ItemsSource = this.m_RestaurantListViewModel.CollectionViewModel.FilteredCollection;

                this.RestaurantsSemanticZoom.ToggleActiveView();
            }

        }


        /// <summary>
        /// Sets the results to the clicked / tapped Restaurant Group from the Semantic Zoom's zoomed out view
        /// </summary>
        /// <param name="sender">the clicked / tapped Restaurant Group</param>
        /// <param name="e">ignored</param>
        private void RestaurantGrouping_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is KeyValuePair<Lib.Models.Cuisines, List<Lib.Models.Restaurant>>)
            {
                KeyValuePair<Lib.Models.Cuisines, List<Lib.Models.Restaurant>> group = 
                    (KeyValuePair<Lib.Models.Cuisines, List<Lib.Models.Restaurant>>)e.ClickedItem;
                this.m_RestaurantListViewModel.GroupedRestaurantList =
                    new System.Collections.ObjectModel.ObservableCollection<Restaurant>(group.Value);
            }
            else if (e.ClickedItem is KeyValuePair<Lib.Models.PriceRanges, List<Lib.Models.Restaurant>>)
            {
                KeyValuePair<Lib.Models.PriceRanges, List<Lib.Models.Restaurant>> group = 
                    (KeyValuePair<Lib.Models.PriceRanges, List<Lib.Models.Restaurant>>)e.ClickedItem;
                this.m_RestaurantListViewModel.GroupedRestaurantList =
                    new System.Collections.ObjectModel.ObservableCollection<Restaurant>(group.Value);
            }

            this.m_RestaurantListViewModel.GroupingEnabled = true;
            this.ResultsHorizontal.ItemsSource = this.m_RestaurantListViewModel.GroupedRestaurantList;
            this.ResultsVertical.ItemsSource = this.m_RestaurantListViewModel.GroupedRestaurantList;
            this.RestaurantsSemanticZoom.CanChangeViews = true;
            this.ZoomedOutViewAllItem.Visibility = Visibility.Visible;

            this.RestaurantsSemanticZoom.ToggleActiveView();
        }

        /// <summary>
        /// Shows the Filter flyout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">ignored </param>
        private void FilterBtn_Click(object sender, RoutedEventArgs e)
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
        void FlyoutFilterClosed(object sender, object e)
        {
            if (this.m_RestaurantListViewModel.CollectionViewModel.CurrentFilter.Dirty)
                this.m_RestaurantListViewModel.CollectionViewModel.ApplyOptions();
        }

        /// <summary>
        /// Handles the view model's PropertyChangedEventArgs to change the data source to the FilteredCollection if present
        /// </summary>
        /// <param name="sender">the CollectionViewModel</param>
        /// <param name="e">the PropertyChangedEventArgs</param>
        private void CollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FilteredCollection":
                    if (this.m_RestaurantListViewModel.CollectionViewModel.FilteredCollection != null)
                    {
                        this.ResultsHorizontal.ItemsSource = this.m_RestaurantListViewModel.CollectionViewModel.FilteredCollection;
                        this.ResultsVertical.ItemsSource = this.m_RestaurantListViewModel.CollectionViewModel.FilteredCollection;
                    }
                    break;
                case "IsFiltered":
                    if (this.m_RestaurantListViewModel.CollectionViewModel.IsFiltered)
                    {
                        StringBuilder subTitle = new StringBuilder();

                        if (this.m_RestaurantListViewModel.SubTitle.Length > 0)
                            subTitle.Append(this.m_RestaurantListViewModel.SubTitle);


                        subTitle.Append(": FILTERED");
                        this.m_RestaurantListViewModel.SubTitle = subTitle.ToString();
                    }
                    else
                    {
                        if (null != this.m_RestaurantListViewModel.SubTitle)
                            this.m_RestaurantListViewModel.SubTitle = 
                                this.m_RestaurantListViewModel.SubTitle.Replace(": FILTERED", "");
                    }
                    break;
            }
        }

        /// <summary>
        /// Navigates to the MapPage passing the cuurrent collection of Restaurants
        /// </summary>
        /// <param name="sender">the View on Map button</param>
        /// <param name="e">ignored</param>
        private void MapView_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MapPage),
              new Tuple<string, IEnumerable<Restaurant>, bool>(
                  this.m_RestaurantListViewModel.ListName,
                  this.m_RestaurantListViewModel.CollectionViewModel.FilteredCollection,
                  false));
        }

        /// <summary>
        /// Sets the list of Restaurants to the User's current favorites
        /// </summary>
        private void RefreshFavorites()
        {
            this.m_RestaurantListViewModel.RestaurantList = AppData.Current.Favorites;
            this.m_RestaurantListViewModel.CollectionViewModel =
                new RestaurantCollectionViewModel(this.m_RestaurantListViewModel.RestaurantList);
            this.LoadState(this.m_RestaurantListViewModel, null);
        }

        /// <summary>
        /// Shows a dialog to confirm the removal of the selected Rstaurants from the user's favorites
        /// </summary>
        /// <param name="sender">the Remove from Favorites button</param>
        /// <param name="e">ignored</param>
        private async void RemoveFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (null != this.ResultsHorizontal.SelectedItems && 0 < this.ResultsHorizontal.SelectedItems.Count)
            {
                MessageDialog messageDialog = 
                    new MessageDialog("Are you sure you want to remove these favorites?", "Confirm Favorites Removal");
                messageDialog.Commands.Clear();
                messageDialog.Commands.Add(new UICommand(
                    "Confirm",
                    new UICommandInvokedHandler(this.RemoveFavoritesCommandHandler),
                    0));
                messageDialog.Commands.Add(new UICommand(
                    "Cancel",
                    new UICommandInvokedHandler(this.RemoveFavoritesCommandHandler),
                    1));

                messageDialog.DefaultCommandIndex = 0;
                messageDialog.CancelCommandIndex = 1;

                await messageDialog.ShowAsync();
            }
        }

        /// <summary>
        /// If the UICommand is 0, removes the selected Restaurants from the User's favorites
        /// </summary>
        /// <param name="command">the IUICommand from the confirmation dialog</param>
        private async void RemoveFavoritesCommandHandler(IUICommand command)
        {
            if(0 == (int)command.Id) 
            {
                await AppData.Current.RemoveFavoritesAsync(this.ResultsHorizontal.SelectedItems.Cast<Restaurant>());
                this.RefreshFavorites();
            }
        }

    }
}
