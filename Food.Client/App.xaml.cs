using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Client.Details;
using Microsoft.DPE.ReferenceApps.Food.Client.Sections;
using Microsoft.DPE.ReferenceApps.Food.Lib.Helpers;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Microsoft.DPE.ReferenceApps.Food.Client
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Do not repeat app initialization when already running, just ensure that
            // the window is active
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                if (!String.IsNullOrEmpty(args.Arguments))
                {
                    // Secondary tile - Deserialize the JSON in the argument into a Restaurant object and refresh
                    var restaurant = await GetRestaurantFromSecondaryTile(args.Arguments);
                    ((Frame)Window.Current.Content).Navigate(typeof(RestaurantDetailsPage), restaurant);

                    //var restaurant = SerializationHelper.JSON.Deserialize<Restaurant>(args.Arguments);
                    //((Frame)Window.Current.Content).Navigate(typeof(RestaurantDetailsPage), restaurant);
                    //Window.Current.Activate();

                    //if (restaurant != null)
                    //{
                    //    // Now we will grab the latest version of restaurant with updated data and reviews from online.  
                    //    ((RestaurantDetailsPage)((Frame)Window.Current.Content).Content).DataContext = await AppData.RefreshRestaurant(restaurant);
                    //}

                }
                Window.Current.Activate();
                return;
            }

            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                //TODO: Load state from previously suspended application
            }

            // Create a Frame to act as the navigation context
            var rootFrame = new Frame();

            // IANBE: Navigate to the first page regardless to place it in the backstack 
            // and wireup Restaurants;
            if (!rootFrame.Navigate(typeof(AppHub)))
                throw new Exception("Failed to create initial page");

            // If the app was activated from a secondary tile, open to the correct restaurant
            if (!String.IsNullOrEmpty(args.Arguments))
            {
                var restaurant = await GetRestaurantFromSecondaryTile(args.Arguments);
                rootFrame.Navigate(typeof(RestaurantDetailsPage), restaurant);

                // TODO: figure out why the below code isn't working - probably the DataContext isn't correct (should be passing in a viewModel and not the restaurant?).
                // This implementation below should give a perf boost and support an offline scenario too.  

                //var restaurant = SerializationHelper.JSON.Deserialize<Restaurant>(args.Arguments);
                //rootFrame.Navigate(typeof(RestaurantDetailsPage), restaurant);

                //this.WireupPanes();

                //// Place the frame in the current Window and ensure that it is active
                //Window.Current.Content = rootFrame;
                //Window.Current.Activate();

                //if (restaurant != null)
                //{
                //    // Now we will grab the latest version of restaurant with updated data and reviews from online.  
                //    ((RestaurantDetailsPage)rootFrame.Content).DataContext = await AppData.RefreshRestaurant(restaurant);
                //}
                
                //return;
            }

            this.WireupPanes();

            // Place the frame in the current Window and ensure that it is active
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }

        /// <summary>
        /// Gets the Restaurant object from the activation string passed by a secondary tile.  The activation string is serialized JSON
        /// of the Restaurant object, created when the user first pins a secondary tile, so some data (like reviews) will be stale.  
        /// We are recreating the Restaurant from the serialized JSON and then updating the info with the latest data from online if 
        /// there is internet connectivity.  
        /// Side effects: Since we are going online to check for latest data, this will turn off the "Use Sample Data" setting.  
        /// </summary>
        /// <param name="activationString">Activation string passed by the secondary tile, which we are expecting to be serialized JSON of the Restaurant object</param>
        /// <returns>The rehydrated Restaurant object</returns>
        private static async Task<Restaurant> GetRestaurantFromSecondaryTile(string activationString)
        {
            // TODO: We should be navigating to this cached version of the Restaurant from the JSON immediately, and then we will 
            // update the info with the latest from online.  This will give us a performance boost.  
            // TODO: This approach should support an offline scenario, but there are bugs elsewhere in the app preventing me from 
            // testing this thoroughly.  Need to test the whole offline scenario better (with and without secondary tiles).  

            // NOTE: using AppData.RestaurantForKeyAsync() doesn't reach out to Yelp; it only uses cached data.  

            // Deserialize the JSON in the argument into a Restaurant object
            var restaurant = SerializationHelper.JSON.Deserialize<Restaurant>(activationString);

            if (restaurant != null)
            {
                // Now we will grab the latest version of restaurant with updated data and reviews from online.  
                restaurant = await AppData.RefreshRestaurant(restaurant);
            }

            return restaurant;
        }

        private void WireupPanes()
        {
            // Enable About
            // IANBE: "Settings" should only be on AppHub
            var _Helper = new Lib.Helpers.SettingsHelper();
            _Helper.AddCommand<Flyouts.About>("About", "App_About");
            _Helper.AddCommand<Flyouts.Settings>("Settings", "Hub_Settings");
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when the application is activated to display search results.
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected async override void OnSearchActivated(Windows.ApplicationModel.Activation.SearchActivatedEventArgs args)
        {
            if (!Lib.Models.AppData.Current.IsDataLoaded)
                await Lib.Models.AppData.Current.LoadDataAsync(true);

            if (null == Window.Current.Content)
            {
                var rootFrame = new Frame();
                Window.Current.Content = rootFrame;
                Window.Current.Activate();
                rootFrame.Navigate(typeof(AppHub), null);

                this.WireupPanes();
            }

            (Window.Current.Content as Frame).Navigate(typeof(RestaurantList),
                new ViewModels.RestaurantListViewModel()
                {
                    ListType = ViewModels.RestaurantListViewModel.RestaurantListTypes.SearchResults,
                    SearchText = args.QueryText
                });
        }

        private System.Text.RegularExpressions.Regex _restaurantNameAndGeoPattern =
            new System.Text.RegularExpressions.Regex(@"contosofood\://restaurant/(?<Lat>\-?\d+\.\d+),(?<Lon>\-?\d+\.\d+)/(?<Name>[a-zA-Z0-9\s]+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private System.Text.RegularExpressions.Regex _restaurantUrlPattern =
            new System.Text.RegularExpressions.Regex(@"contosofood\://restaurant/(?<Id>[a-zA-Z0-9]+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        /// <summary>
        /// Invoked when the application is activated for things like search or
        /// protocol activation
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected async override void OnActivated(IActivatedEventArgs args)
        {
            if (ActivationKind.Protocol == args.Kind)
            {
                Restaurant restaurant = null;
                ProtocolActivatedEventArgs protocolArgs = args as ProtocolActivatedEventArgs;
                Uri launchUri = protocolArgs.Uri;
                string path = Uri.UnescapeDataString(launchUri.AbsoluteUri);
                if (_restaurantNameAndGeoPattern.IsMatch(path))
                {
                    System.Text.RegularExpressions.Match _restaurantNameAndGeoPatternMatch =
                        _restaurantNameAndGeoPattern.Match(path);

                    if (null != _restaurantNameAndGeoPatternMatch &&
                        null != _restaurantNameAndGeoPatternMatch.Groups &&
                        4 == _restaurantNameAndGeoPatternMatch.Groups.Count)
                    {
                        double lat = double.NaN, lon = double.NaN;
                        string name = _restaurantNameAndGeoPatternMatch.Groups["Name"].Value;

                        if (!string.IsNullOrWhiteSpace(name) &&
                            double.TryParse(
                                _restaurantNameAndGeoPatternMatch.Groups["Lat"].Value,
                                out lat) &&
                            double.TryParse(
                                _restaurantNameAndGeoPatternMatch.Groups["Lon"].Value,
                                out lon))
                            restaurant =
                                await Lib.Models.AppData.RestaurantForLatLonNameAsync(lat, lon, name);
                    }
                }
                else if (_restaurantUrlPattern.IsMatch(path))
                {
                    System.Text.RegularExpressions.Match _restaurantUrlPatternMatch =
                        _restaurantUrlPattern.Match(path);

                    if (null != _restaurantUrlPatternMatch &&
                        null != _restaurantUrlPatternMatch.Groups &&
                        2 == _restaurantUrlPatternMatch.Groups.Count)
                    {
                        string restaurantKey = _restaurantUrlPatternMatch.Groups["Id"].Value;
                        if (!string.IsNullOrWhiteSpace(restaurantKey))
                            restaurant =
                                await Lib.Models.AppData.RestaurantForKeyAsync(restaurantKey);
                    }
                }

                if (null == Window.Current.Content)
                {
                    var rootFrame = new Frame();
                    Window.Current.Content = rootFrame;
                    Window.Current.Activate();
                    rootFrame.Navigate(typeof(AppHub), null);

                    this.WireupPanes();
                }

                if (null != restaurant)
                    (Window.Current.Content as Frame).Navigate(typeof(RestaurantDetailsPage), restaurant);

            }

            base.OnActivated(args);
        }

    }
}
