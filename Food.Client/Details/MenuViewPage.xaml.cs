using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.DPE.ReferenceApps.Food.Client.Details
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MenuViewPage : Microsoft.DPE.ReferenceApps.Food.Client.Common.LayoutAwarePage
    {
        Client.ViewModels.MenuViewModel m_ViewModel;

        public MenuViewPage()
        {
            this.InitializeComponent();
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
                this.m_ViewModel = new Client.ViewModels.MenuViewModel();
            }

            if (null != navigationParameter && navigationParameter is Lib.Models.Restaurant)
            {
                this.m_ViewModel.Restaurant = navigationParameter as Lib.Models.Restaurant;
            }

            this.DataContext = this.m_ViewModel;
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
        /// Sets the MenusFull GridView's ItemSource to the tapped/clicked Menu Group in the zoomed out Semantic Zoom view
        /// </summary>
        /// <param name="sender">the clicked / tapped Menu Group</param>
        /// <param name="e">the ItemClickEventArgs</param>
        private void ZoomGroup_Click(object sender, ItemClickEventArgs e)
        {
            Lib.Services.OpenMenu.MenuHelper.Group group = e.ClickedItem as Lib.Services.OpenMenu.MenuHelper.Group;

            // Get the Menu that owns the clicked / tapped Menu Group
            Lib.Services.OpenMenu.MenuHelper.Menu parentMenu = 
                this.m_ViewModel.Restaurant.Menus.FirstOrDefault(r => group.Equals(r.Groups.FirstOrDefault(g => g.Id == group.Id)));

            // Create a new Menu instance to hold only the clicked / tapped Menu Group
            List<Lib.Services.OpenMenu.MenuHelper.Menu> menus = 
                new List<Lib.Services.OpenMenu.MenuHelper.Menu>() {
                    new Lib.Services.OpenMenu.MenuHelper.Menu()
                    {
                        Name = parentMenu.Name,
                        Groups = new List<Lib.Services.OpenMenu.MenuHelper.Group>()
                        {
                            group
                        }
                    }
                };

            // Zoom back in and show the new Menu
            this.ZoomedOutViewAllItem.Visibility = Visibility.Visible;
            this.MenusFull.ItemsSource = menus;
            this.MenusSnappedItems.ItemsSource = menus;
            this.MenusFull.ApplyTemplate();
            this.MenuZoom.ToggleActiveView();
        }

        /// <summary>
        /// Goes back to the default state showing all Menus with all Menu Groups
        /// </summary>
        /// <param name="sender">the View All button in the zommed out Smeantic Zoom view</param>
        /// <param name="e">the ItemClickEventArgs</param>
        private void ViewAll_Click(object sender, RoutedEventArgs e)
        {
            this.ZoomedOutViewAllItem.Visibility = Visibility.Collapsed;
            this.MenusFull.ItemsSource = this.m_ViewModel.Restaurant.Menus;
            this.MenusSnappedItems.ItemsSource = this.m_ViewModel.Restaurant.Menus;
            this.MenuZoom.ToggleActiveView();
        }
    }
}
