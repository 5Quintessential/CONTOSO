using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.DPE.ReferenceApps.Food.Lib.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Client.Sections;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using Windows.UI.Core;
using System.Collections.ObjectModel;
using Windows.Devices.Sensors;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Microsoft.DPE.ReferenceApps.Food.Client
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class AppHub : Food.Client.Common.LayoutAwarePage
    {
        private Client.ViewModels.HubViewModel m_ViewModel;
        private MessageDialog m_msgDialog;
        private UICommand m_okCommand;
        private Accelerometer m_Accelerometer = null;


        public AppHub()
        {
            this.InitializeComponent();
            this.m_okCommand = new UICommand("Ok", new UICommandInvokedHandler(OkCommandHandler));
            this.m_Accelerometer = Accelerometer.GetDefault();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Client.ViewModels.FilterFlyoutViewModel.ResetCurrent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if(null != this.m_Accelerometer)
                this.m_Accelerometer.Shaken -= Accelerometer_Shaken;
        }

        private async void ViewModelDataEmpty(object sender, EventArgs args)
        {
            if (null == m_msgDialog)
            {
                m_msgDialog = new MessageDialog("No Results were returned...", "No Data");
                m_msgDialog.Commands.Add(m_okCommand);
                await m_msgDialog.ShowAsync();
            }
        }

        async void ViewModelNoLocation(object sender, EventArgs args)
        {
            if (null == m_msgDialog)
            {
                m_msgDialog = new MessageDialog("Error retrieving location. Please set location manually via the location button in the ottom app bar.", "No Location");
                m_msgDialog.Commands.Add(m_okCommand);
                await m_msgDialog.ShowAsync();
            }
        }

        void GotoMap()
        {
            this.Frame.Navigate(typeof(Sections.MapPage),
              new Tuple<string, IEnumerable<Restaurant>, bool>("Near Me", m_ViewModel.NearMeAll, false));
        }

        void GotoFavorites()
        {
            this.Frame.Navigate(typeof(RestaurantList),
                new ViewModels.RestaurantListViewModel()
                {
                    ListType = ViewModels.RestaurantListViewModel.RestaurantListTypes.Favorites,
                    ListName = "My Favorites",
                    RestaurantList = AppData.Current.Favorites
                });
        }

        void GotoNearMe()
        {
            this.Frame.Navigate(typeof(RestaurantList),
                new ViewModels.RestaurantListViewModel() {
                    ListType = ViewModels.RestaurantListViewModel.RestaurantListTypes.Collection,
                    ListName = "Top Near Me",
                    RestaurantList = new ObservableCollection<Lib.Models.Restaurant>(m_ViewModel.NearMeAll)
                });
        }

        void GotoRecentlyReviewed()
        {
            this.Frame.Navigate(typeof(RestaurantList),
                new ViewModels.RestaurantListViewModel()
                {
                    ListType = ViewModels.RestaurantListViewModel.RestaurantListTypes.Collection,
                    ListName = "Recently Reviewed",
                    RestaurantList = new ObservableCollection<Lib.Models.Restaurant>(m_ViewModel.RecentAll)
                });
        }

        void GotoTrending()
        {
            this.Frame.Navigate(typeof(RestaurantList),
                new ViewModels.RestaurantListViewModel()
                {
                    ListType = ViewModels.RestaurantListViewModel.RestaurantListTypes.Collection,
                    ListName = "Trending",
                    RestaurantList = new ObservableCollection<Lib.Models.Restaurant>(m_ViewModel.TrendingAll)
                });
        }

        void GotoDetail(Microsoft.DPE.ReferenceApps.Food.Lib.Models.Restaurant restaurant)
        {
            this.Frame.Navigate(typeof(Details.RestaurantDetailsPage),
                restaurant);
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
            if (null != pageState)
            {
                if (pageState.ContainsKey("HubViewModel"))
                {
                    if (null != pageState["HubViewModel"])
                    {
                        this.m_ViewModel = pageState["HubViewModel"] as Client.ViewModels.HubViewModel;
                    }
                }
            }

            if (null == this.m_ViewModel)
            {
                this.m_ViewModel = new Client.ViewModels.HubViewModel();
                this.m_ViewModel.MotionSupported = (null != this.m_Accelerometer);
                if (this.m_ViewModel.MotionSupported)
                    this.m_Accelerometer.Shaken += Accelerometer_Shaken;
            }

            if (null != this.m_ViewModel)
            {
                this.DataContext = this.m_ViewModel;
                this.m_ViewModel.DataIsEmpty +=
                    new EventHandler(ViewModelDataEmpty);
                this.m_ViewModel.LocationIsEmpty +=
                    new EventHandler(ViewModelNoLocation);
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
            this.m_ViewModel.DataIsEmpty -=
                new EventHandler(ViewModelDataEmpty);
            this.m_ViewModel.LocationIsEmpty -=
                new EventHandler(ViewModelNoLocation);
            pageState.Add("HubViewModel", this.m_ViewModel);
        }

        /// <summary>
        /// Navigate to Yelp, as per http://www.yelp.com/developers/getting_started/display_requirements
        /// </summary>
        async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Launch the URI
            var uri = new Uri(@"http://www.yelp.com");
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private void LocationButton_Click(object sender, RoutedEventArgs e)
        {
            new Lib.Helpers.FlyoutHelper().Show(SetLocationFlyout, sender as Button, 50);
        }

        private void SurpriseMeButton_Click(object sender, RoutedEventArgs e)
        {
            m_ViewModel.RefreshSurpriseMe();
        }

        private void Restaurant_ItemClick(object sender, ItemClickEventArgs e)
        {
            GotoDetail(e.ClickedItem as Lib.Models.Restaurant);
        }

        private void btnMoreTrending_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            GotoTrending();
        }

        private void btnMoreRecent_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            GotoRecentlyReviewed();
        }

        private void btnMoreNearMe_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            GotoNearMe();
        }

        private void btnMoreFavorites_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            GotoFavorites();
        }

        async void RefreshButton_Click_1(object sender, RoutedEventArgs e)
        {
            // cause data to reload
            await AppData.Current.LoadDataAsync(false);
        }

        private void MapImage_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            GotoMap();
        }

        private void OkCommandHandler(IUICommand command)
        {
            m_msgDialog = null;
        }

        // Called when accelerometer triggers a Shaken event
        private void Accelerometer_Shaken(Accelerometer sender, AccelerometerShakenEventArgs args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.m_ViewModel.RefreshSurpriseMe();
            });
        }

        private void SurpriseMeRestaurant_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Details.RestaurantDetailsPage),
                this.m_ViewModel.SurpriseMeRestaurant);
        }
    }
}
