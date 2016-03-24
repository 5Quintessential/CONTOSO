using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Lib.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Microsoft.DPE.ReferenceApps.Food.Client.Details
{
  /// <summary>
  /// A basic page that provides characteristics common to most applications.
  /// </summary>
  public sealed partial class RestaurantCollectionViewPage : Microsoft.DPE.ReferenceApps.Food.Client.Common.LayoutAwarePage
  {
    public RestaurantCollectionViewPage()
    {
      this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);

      if (e.Parameter != null)
      {
        if (e.Parameter is Tuple<string, Task<IEnumerable<Microsoft.DPE.ReferenceApps.Food.Lib.Models.Restaurant>>>)
        {
          Tuple<string, Task<IEnumerable<Microsoft.DPE.ReferenceApps.Food.Lib.Models.Restaurant>>> navparams = e.Parameter as Tuple<string, Task<IEnumerable<Microsoft.DPE.ReferenceApps.Food.Lib.Models.Restaurant>>>;
          ucCollectionView.ViewModel = new RestaurantCollectionViewModel(navparams.Item2);
          pageSubtitle.Text = navparams.Item1;
        }
        //else if (e.Parameter is Tuple<string, Filter>)
        //{
        //  Tuple<string, Filter> navparams = e.Parameter as Tuple<string, Filter>;
        //  ucCollectionView.ViewModel = new RestaurantCollectionViewModel(navparams.Item2);
        //  pageSubtitle.Text = navparams.Item1;
        //}
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
