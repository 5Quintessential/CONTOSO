using Microsoft.DPE.ReferenceApps.Food.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Microsoft.DPE.ReferenceApps.Food.Client.Details
{
    public sealed partial class RestaurantDetailsPage : Food.Client.Common.LayoutAwarePage
    {
        Client.ViewModels.RestaurantDetailsViewModel m_ViewModel;
        DataTransferManager m_dataTransferManager;
        AppBar _bottomAppBar = null;


        public RestaurantDetailsPage()
        {
            this.InitializeComponent();

            // Get a handle on the DataTransferManager and handle the DataRequested event to share the restaurant details
            this.m_dataTransferManager = DataTransferManager.GetForCurrentView();
            if(null != this.m_dataTransferManager)
                this.m_dataTransferManager.DataRequested += DataTransferManager_DataRequested;

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
            if (ApplicationViewState.Snapped == ApplicationView.Value)
            {
                this._bottomAppBar = this.BottomAppBar;
                this.BottomAppBar = null;
            }
            else
            {
                if (null == this.BottomAppBar && null != this._bottomAppBar)
                    this.BottomAppBar = this._bottomAppBar;
            }

            this.CheckSize();
        }

        /// <summary>
        /// Overriden to remove DataTransferManager.DataRequested event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.m_dataTransferManager.DataRequested -= DataTransferManager_DataRequested;
            base.OnNavigatedFrom(e);
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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (null == this.m_ViewModel)
            {
                this.m_ViewModel = new Client.ViewModels.RestaurantDetailsViewModel();
            }

            if (null != pageState)
            {
                if (pageState.ContainsKey("RDViewModel"))
                {
                    if (null != pageState["RDViewModel"])
                    {
                        this.m_ViewModel = pageState["RDViewModel"] as Client.ViewModels.RestaurantDetailsViewModel;
                    }
                }
            }
            else if (null != navigationParameter && navigationParameter is Lib.Models.Restaurant)
            {
                this.m_ViewModel.Restaurant = navigationParameter as Lib.Models.Restaurant;
            }

            this.DataContext = this.m_ViewModel;
        }

        /// <summary>
        /// Checks the current Window height to appropriately deal with x768 and x1080 resolutions
        /// </summary>
        private void CheckSize()
        {
            // If the vertical resolution is >= 1080 we flow the overview details vertically in one column
            // otherwise we flow them into two columns
            if (Window.Current.Bounds.Height >= 1080)
            {
                this.FactualDetailsPanel.Orientation = Orientation.Vertical;
                this.FactualDetailsColumnTwoPanel.Margin = new Thickness(0d);
            }
            else
            {
                this.FactualDetailsPanel.Orientation = Orientation.Horizontal;
                this.FactualDetailsColumnTwoPanel.Margin = new Thickness(10d, 0d, 0d, 0d);
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
            // Preserve the RestaurantDetailsViewModel into page state
            pageState.Add("RDViewModel", this.m_ViewModel);
        }

        /// <summary>
        /// Launches the uri associated with a Yelp review.  By default this will launch a browser window
        /// and navigate to the Uri.
        /// </summary>
        /// <param name="sender">the clicked / tapped review item</param>
        /// <param name="e">the ItemClickEventArgs</param>
        private void ReviewItemClick(object sender, ItemClickEventArgs e)
        {
            var uri = new Uri((e.ClickedItem as Lib.Models.Review).YelpRecord.url);
            Windows.System.Launcher.LaunchUriAsync(uri);
        }

        /// <summary>
        /// Navigates to the MenuViewPage passing the Restauarant object to display the menu for.
        /// </summary>
        /// <param name="sender">the Menu header</param>
        /// <param name="e">the unused RoutedEventArgs</param>
        private void ViewMenuButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MenuViewPage), this.m_ViewModel.Restaurant);
        }

        /// <summary>
        /// Navigates to the GalleryViewPage passing the list of gallery images as well as the index for 
        /// the clicked / tapped image.  The GalleryViewPage will scroll to the image using the index.
        /// </summary>
        /// <param name="sender">the individual gallery image that was clicked / tapped</param>
        /// <param name="e">the ItemClickEventArgs</param>
        private void GalleryItemClick(object sender, ItemClickEventArgs e)
        {
            List<Lib.Services.BingSearch.ImageSearchHelper.Result> images = this.m_ViewModel.ImageResults.ToList();
            int index = images.IndexOf(e.ClickedItem as Lib.Services.BingSearch.ImageSearchHelper.Result);

            this.Frame.Navigate(typeof(GalleryViewPage),
                new Tuple<string,
                        List<Lib.Services.BingSearch.ImageSearchHelper.Result>, int>(
                            this.m_ViewModel.Restaurant.Name,
                            images,
                            index));
        }

        /// <summary>
        /// Navigates to the GalleryViewPage passing the list of images
        /// </summary>
        /// <param name="sender">the Gallery header</param>
        /// <param name="e">the unused RoutedEventArgs</param>
        private void ViewGalleryButton_Click(object sender, RoutedEventArgs e)
        {
            List<Lib.Services.BingSearch.ImageSearchHelper.Result> images = this.m_ViewModel.ImageResults.ToList();

            this.Frame.Navigate(typeof(GalleryViewPage),
                new Tuple<string,
                        List<Lib.Services.BingSearch.ImageSearchHelper.Result>, int>(
                            this.m_ViewModel.Restaurant.Name,
                            images,
                            0));
        }

        /// <summary>
        /// Adds the current Restaurant object to the user's list of favorites
        /// </summary>
        /// <param name="sender">the Add to Favorites button</param>
        /// <param name="e">the unused RoutedEventArgs</param>
        private async void AddToFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            await Lib.Models.AppData.Current.AddFavoriteAsync(this.m_ViewModel.Restaurant);
            this.m_ViewModel.AddToFavoritesButtonVisibility = Visibility.Collapsed;
            this.m_ViewModel.RemoveFromFavoritesButtonVisibility = Visibility.Visible;
        }

        /// <summary>
        /// Removes the current Restaurant object to the user's list of favorites
        /// </summary>
        /// <param name="sender">the Remove from Favorites button</param>
        /// <param name="e">the unused RoutedEventArgs</param>
        private async void RemoveFromFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            await Lib.Models.AppData.Current.RemoveFavoriteAsync(this.m_ViewModel.Restaurant);
            this.m_ViewModel.AddToFavoritesButtonVisibility = Visibility.Visible;
            this.m_ViewModel.RemoveFromFavoritesButtonVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Pins a tile on the user's start screen representing the current Restauarant
        /// </summary>
        /// <param name="sender">the Pin button</param>
        /// <param name="e">the unused RoutedEventArgs</param>
        private async void PinSecondaryTileButton_Click(object sender, RoutedEventArgs e)
        {
            // Keep AppBar open until done.
            this.BottomAppBar.IsSticky = true;
            this.BottomAppBar.IsOpen = true;

            // We are serializing the restaurant object and restoring it when the secondary tile is clicked.  The pros of this 
            // approach are that it could be a performance optimization (we already have the data rather than going over the 
            // network for it) and it enables an offline scenario where you can still see the address etc even if you can't see 
            // the very latest reviews.  On launch of the app from the secondary tile, we will refresh the data from Yelp.  

            var restaurant = this.m_ViewModel.Restaurant;

            // The restaurant logo that we will use for the secondary tile image is coming from Yelp, and it is not supported to 
            // pass an http image to the SecondaryTile constructor.  So we have to copy it locally first.  
            var logoUri = await GetLocalImageAsync(restaurant.ImagePath, restaurant.Key);

            // Serialize the Restaurant.  
            string serializedRest = SerializationHelper.JSON.Serialize(this.m_ViewModel.Restaurant);

            // Create secondary tile object.  
            var tile = new SecondaryTile(
                    restaurant.Key,                 // Tile ID
                    restaurant.Name,                // Tile short name
                    restaurant.Name,                // Tile display name
                    serializedRest,                 // Activation argument
                    TileOptions.ShowNameOnLogo,     // Tile options
                    logoUri                         // Tile logo URI
                );

            tile.ForegroundText = ForegroundText.Light;

            // Show secondary tile popup and create asynchronously.  
            bool isPinned = await tile.RequestCreateForSelectionAsync(GetElementRect((FrameworkElement)sender), Windows.UI.Popups.Placement.Above);

            if (isPinned)
            {
                Debug.WriteLine("Secondary tile successfully pinned");

                // Change button in AppBar from "Pin" to "Unpin"
                this.m_ViewModel.PinSecondaryTileButtonVisibility = Visibility.Collapsed;
                this.m_ViewModel.UnpinSecondaryTileButtonVisibility = Visibility.Visible;
            }
            else
            {
                Debug.WriteLine("Secondary tile creation FAIL");
            }

            // Close AppBar
            this.BottomAppBar.IsOpen = false;
            this.BottomAppBar.IsSticky = false;
        }

        /// <summary>
        /// Unpins the tile on the user's start screen representing the current Restauarant
        /// </summary>
        /// <param name="sender">the Unpin button</param>
        /// <param name="e">the unused RoutedEventArgs</param>
        private async void UnpinSecondaryTileButton_Click(object sender, RoutedEventArgs e)
        {
            // Keep AppBar open until done.
            this.BottomAppBar.IsSticky = true;
            this.BottomAppBar.IsOpen = true;

            // Check to see if this restaurant exists as a secondary tile and then unpin it
            string restaurantKey = this.m_ViewModel.Restaurant.Key;
            Button button = sender as Button;
            if (button != null)
            {
                if (Windows.UI.StartScreen.SecondaryTile.Exists(restaurantKey))
                {
                    SecondaryTile secondaryTile = new SecondaryTile(restaurantKey);
                    bool isUnpinned = await secondaryTile.RequestDeleteForSelectionAsync(GetElementRect((FrameworkElement)sender), Windows.UI.Popups.Placement.Above);

                    if (isUnpinned)
                    {
                        Debug.WriteLine("Secondary tile successfully unpinned.");

                        // Change button in AppBar from "Unpin" to "Pin"
                        this.m_ViewModel.PinSecondaryTileButtonVisibility = Visibility.Visible;
                        this.m_ViewModel.UnpinSecondaryTileButtonVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Debug.WriteLine("Secondary tile not unpinned.");
                    }
                }
                else
                {
                    Debug.WriteLine(restaurantKey + " is not currently pinned.");

                    // If we ever get to this point, something went very wrong and the pin/unpin functionality is mixed up, 
                    // so we will correct it now.  The secondary tile doesn't exist so "Pin" should show.  
                    this.m_ViewModel.PinSecondaryTileButtonVisibility = Visibility.Visible;
                    this.m_ViewModel.UnpinSecondaryTileButtonVisibility = Visibility.Collapsed;
                }
            }

            // Close AppBar
            this.BottomAppBar.IsOpen = false;
            this.BottomAppBar.IsSticky = false;
        }

        /// <summary>
        /// Copies an image from the internet (http protocol) locally to the AppData LocalFolder.  This is used by some methods 
        /// (like the SecondaryTile constructor) that do not support referencing images over http but can reference them using 
        /// the ms-appdata protocol.  
        /// </summary>
        /// <param name="internetUri">The path (URI) to the image on the internet</param>
        /// <param name="uniqueName">A unique name for the local file</param>
        /// <returns>Path to the image that has been copied locally</returns>
        private async Task<Uri> GetLocalImageAsync(string internetUri, string uniqueName)
        {
            if (string.IsNullOrEmpty(internetUri))
            {
                return null;
            }

            using (var response = await HttpWebRequest.CreateHttp(internetUri).GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    var desiredName = string.Format("{0}.jpg", uniqueName);
                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(desiredName, CreationCollisionOption.ReplaceExisting);

                    using (var filestream = await file.OpenStreamForWriteAsync())
                    {
                        await stream.CopyToAsync(filestream);
                        return new Uri(string.Format("ms-appdata:///local/{0}.jpg", uniqueName), UriKind.Absolute);
                    }
                }
            }
        }

        /// <summary>
        ///  Gets the rectangle of the element for proper placement of a popup
        /// </summary>
        private Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            //IANBE: To give proper spacing above app bar
            point.Y -= 5d;
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        /// <summary>
        /// Event handler for DataTransferManager.DataRequested used to share the details of the current Restaurant
        /// </summary>
        /// <param name="sender">the DataTransferManager</param>
        /// <param name="args">the DataRequestedEventArgs used to share the details of the current Restaurant</param>
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            // Make sure our data is intact
            if (null != this.m_ViewModel.Restaurant)
            {
                // Get a handle on the DataRequest
                DataRequest request = args.Request;

                // Get a deferral so we can grab the image, etc.
                var deferral = request.GetDeferral();

                // Formatted address
                string addr = string.Format("{0} {1},{2} {3}",
                        this.m_ViewModel.Restaurant.Address,
                        this.m_ViewModel.Restaurant.City,
                        this.m_ViewModel.Restaurant.State,
                        this.m_ViewModel.Restaurant.Zip);

                // Set the title of the shared data to the Restaurant name and the description to the formatted address
                request.Data.Properties.Title = this.m_ViewModel.Restaurant.Name;
                request.Data.Properties.Description = addr;

                // Build a string that contains the name, address, cuisines and avg. Yelp rating
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(this.m_ViewModel.Restaurant.Name);
                sb.AppendLine(addr);
                sb.AppendLine();
                sb.AppendFormat("Cuisines: {0}",
                    (this.m_ViewModel.Restaurant.FactualRecordLoaded && null != this.m_ViewModel.Restaurant.FactualRecord) ? this.m_ViewModel.Restaurant.FactualRecord.cuisine : string.Empty);
                sb.AppendLine();
                sb.AppendFormat("Avg. Yelp Rating: {0}",
                    this.m_ViewModel.Restaurant.YelpRecord.avg_rating);

                // Set the text to the detailed string.  This would be used by apps that cannot use the Url
                request.Data.SetText(sb.ToString());

                // If the Restaurant has an image, add it
                if (null != this.m_ViewModel.Restaurant.ImagePath)
                {
                    RandomAccessStreamReference imageStreamRef =
                            RandomAccessStreamReference.CreateFromUri(new Uri(this.m_ViewModel.Restaurant.ImagePath));

                    request.Data.Properties.Thumbnail = imageStreamRef;
                    request.Data.SetBitmap(imageStreamRef);
                }

                // If the Rstauarant has a website Uri, add it.  There are a number Share Target apps that favor the Uri over other
                // info like Text for example, Mail and People
                if (this.m_ViewModel.Restaurant.FactualRecordLoaded &&
                    null != this.m_ViewModel.Restaurant.FactualRecord &&
                    null != this.m_ViewModel.Restaurant.FactualRecord.website)
                    request.Data.SetUri(new Uri(this.m_ViewModel.Restaurant.FactualRecord.website));

                // We are done setting up the shared info so, release the deferral.
                deferral.Complete();
            }
        }

        /// <summary>
        /// Generates JSON for all of the loaded Restaurants and User settings
        /// </summary>
        /// <param name="sender">the Create JSON button</param>
        /// <param name="e">the unused RoutedEventArgs</param>
        private async void CreateJSONBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Lib.Models.Restaurant> restaurants = Lib.Models.AppData.Current.Restaurants.ToList();

            // Ensure that the menu data is loaded
            foreach (Lib.Models.Restaurant restaurant in restaurants)
                if (!restaurant.OpenmenuRecordLoaded)
                    await Lib.Services.Unity.FillOpenMenuAsync(restaurant);

            // Populate the JSONBoxRestaurants TextBox with the Restauarants list JSON
            this.JSONBoxRestaurants.Document.SetText(Windows.UI.Text.TextSetOptions.None,
                Lib.Helpers.SerializationHelper.JSON.Serialize(Lib.Models.AppData.Current.Restaurants));


            // Populate the JSONBoxUserSettings TextBox with the User settings JSON
            this.JSONBoxUserSettings.Document.SetText(Windows.UI.Text.TextSetOptions.None,
                Lib.Helpers.SerializationHelper.JSON.Serialize(Lib.Models.AppData.Current));
        }

        /// <summary>
        /// Refreshes all of the Restauarant details
        /// </summary>
        /// <param name="sender">the Resfresh button</param>
        /// <param name="e">the unused RoutedEventArgs</param>
        private async void RefreshRestButton_Click(object sender, RoutedEventArgs e)
        {
            // Will attempt to go online and refresh restaurant with latest data
            this.m_ViewModel.Restaurant = await Lib.Models.AppData.RefreshRestaurant(this.m_ViewModel.Restaurant);

            // Close app bar
            this.BottomAppBar.IsOpen = false;
        }
    }
}
