using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Microsoft.DPE.ReferenceApps.Food.Client.Details
{

    public sealed partial class GalleryViewPage : Microsoft.DPE.ReferenceApps.Food.Client.Common.LayoutAwarePage
    {
        string m_Title;
        List<Lib.Services.BingSearch.ImageSearchHelper.Result> m_Images;
        int m_SelectedIndex;
        Lib.Services.BingSearch.ImageSearchHelper.Result m_SelectedItem;
        int tabStop = 0;

        public GalleryViewPage()
        {
            this.InitializeComponent();
            this.KeyDown += GalleryViewPage_KeyDown;
        }

        /// <summary>
        /// Handles the KeyDown event to handle up, down, left, right, tab and back keys for scrolling
        /// </summary>
        /// <param name="sender">this page</param>
        /// <param name="e">the KeyRoutedEventArgs</param>
        void GalleryViewPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {

            int scrollOffset = 30;
            if (ApplicationViewState.Snapped == ApplicationView.Value)
            {
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.Up:
                        this.MainScroller.ScrollToVerticalOffset(this.MainScroller.VerticalOffset - scrollOffset);
                        break;
                    case Windows.System.VirtualKey.Down:
                        this.MainScroller.ScrollToVerticalOffset(this.MainScroller.VerticalOffset + scrollOffset);
                        break;
                    case Windows.System.VirtualKey.Tab:
                        this.MainScroller.ScrollToVerticalOffset(this.MainScroller.VerticalOffset + 100);
                        break;
                    case Windows.System.VirtualKey.Back:
                        this.MainScroller.ScrollToVerticalOffset(this.MainScroller.VerticalOffset - 100);
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.Left:
                        this.MainScroller.ScrollToHorizontalOffset(this.MainScroller.HorizontalOffset - scrollOffset);
                        break;
                    case Windows.System.VirtualKey.Right:
                        this.MainScroller.ScrollToHorizontalOffset(this.MainScroller.HorizontalOffset + scrollOffset);
                        break;
                    case Windows.System.VirtualKey.Tab:
                        this.MainScroller.ScrollToHorizontalOffset(this.MainScroller.HorizontalOffset + 450);
                        break;
                    case Windows.System.VirtualKey.Back:
                        this.MainScroller.ScrollToHorizontalOffset(this.MainScroller.HorizontalOffset - 450);
                        break;
                }
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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (null != navigationParameter && navigationParameter is
                Tuple<string, List<Lib.Services.BingSearch.ImageSearchHelper.Result>, int>)
            {

                Tuple<string, List<Lib.Services.BingSearch.ImageSearchHelper.Result>, int>
                    data = navigationParameter as
                        Tuple<string, List<Lib.Services.BingSearch.ImageSearchHelper.Result>, int>;

                // The title for the gallery
                this.m_Title = data.Item1;
                
                // The gallery images
                this.m_Images = data.Item2;

                // The index for the image to scroll to if any
                this.m_SelectedIndex = data.Item3;
                
                this.DefaultViewModel["Data"] = data;

                // The image represented by the passed index
                this.m_SelectedItem = this.m_Images[this.m_SelectedIndex];

                int index = 0;

                // Build out the tree of image controls to show
                foreach (Lib.Services.BingSearch.ImageSearchHelper.Result image in this.m_Images)
                {
                    BitmapImage bmp = new BitmapImage(new Uri(image.MediaUrl));
                    Image img = new Image();
                    Image snappedImg = new Image();

                    img.Source = bmp;
                    snappedImg.Source = bmp;

                    if (0 == index)
                    {
                        img.Margin = new Thickness(15d);
                        snappedImg.Margin = new Thickness(10d);
                    }
                    else
                    {
                        img.Margin = new Thickness(0d, 15d, 15d, 15d);
                        snappedImg.Margin = new Thickness(10d, 0d, 10d, 10d);
                    }

                    if (this.m_Images.Count - 1 == index)
                        img.SizeChanged += Img_SizeChanged;

                    this.ImagesStripFull.Children.Add(img);
                    this.ImagesStripSnapped.Children.Add(snappedImg);

                    index++;
                }

                this.UpdateLayout();
                Window.Current.SizeChanged += Current_SizeChanged;
            }
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            UpdateSelected(false);
        }

        private void Img_SizeChanged(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Loaded -= Img_SizeChanged;
            UpdateSelected(false);
        }

        /// <summary>
        /// Scrolls to the selected image
        /// </summary>
        /// <param name="isTab">is this update called due to the tab key being pressed?</param>
        private async void UpdateSelected(bool isTab)
        {
            if (0 < this.m_SelectedIndex && this.m_SelectedIndex < this.ImagesStripFull.Children.Count)
            {
                // If not a tab press, scroll to zero first
                if (!isTab)
                {
                    this.MainScroller.ScrollToVerticalOffset(0);
                    this.MainScroller.ScrollToHorizontalOffset(0);
                    this.UpdateLayout();
                    await Task.Delay(200);
                }

                Image img;
                double scrollOffset = 0d;

                // Get a handle on the selected image in the appropriate view
                if (ApplicationViewState.Snapped == ApplicationView.Value)
                    img = this.ImagesStripSnapped.Children[this.m_SelectedIndex] as Image;
                else
                    img = this.ImagesStripFull.Children[this.m_SelectedIndex] as Image;

                // Get the screen position of the image
                GeneralTransform visual = img.TransformToVisual(this);
                Point point = visual.TransformPoint(new Point(0, 0));

                // Scroll to the image taking into account the margins the other image widths / heights, etc.
                if (ApplicationViewState.Snapped == ApplicationView.Value)
                {
                    scrollOffset = point.Y;

                    if (0 < scrollOffset)
                    {
                        scrollOffset -= ((this.MainScroller.ViewportHeight / 2d) - (img.RenderSize.Height / 2));
                        this.MainScroller.ScrollToVerticalOffset(scrollOffset);
                    }
                }
                else
                {
                    scrollOffset = point.X;

                    if (0 < scrollOffset)
                    {
                        scrollOffset -= ((this.MainScroller.ViewportWidth / 2d) - (img.RenderSize.Width / 2));
                        this.MainScroller.ScrollToHorizontalOffset(scrollOffset);
                    }
                }
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
    }
}
