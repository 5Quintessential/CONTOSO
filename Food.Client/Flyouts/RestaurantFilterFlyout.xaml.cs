using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using Microsoft.DPE.ReferenceApps.Food.Lib.Common;
using Microsoft.DPE.ReferenceApps.Food.Client.ViewModels;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.DPE.ReferenceApps.Food.Client.Flyouts
{
  public sealed partial class RestaurantFilterFlyout : UserControl, INotifyPropertyChanged
  {

    public event EventHandler FilterChanged;

    private FilterFlyoutViewModel _currentData;

    public FilterFlyoutViewModel CurrentData
    {
      get
      {
        return _currentData;
      }
      set
      {
        _currentData = value;
        _currentData.Dirty = false;
        cbxCuisine.ItemsSource = CurrentData.CuisineList;
        cbxOrderBy.ItemsSource = FilterFlyoutViewModel.OrderByList;
        cbxPriceRanges.ItemsSource = FilterFlyoutViewModel.PriceRangeList;
        if (PropertyChanged != null)
          PropertyChanged(this, new PropertyChangedEventArgs("CurrentData"));
      }
    }

    public RestaurantFilterFlyout()
    {
      this.InitializeComponent(); 
    }


    public event PropertyChangedEventHandler PropertyChanged;

    private void btnApplyFilter_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (this.Parent is Popup) (this.Parent as Popup).IsOpen = false;
      return;
    }

    private void ClearFilter_Click(object sender, RoutedEventArgs e)
    {
        cbxOrderBy.SelectedIndex = 0;
        cbxCuisine.SelectedIndex = 0;
        cbxPriceRanges.SelectedIndex = 0;
        chkReservationsRequired.IsChecked = false;
        chkTakeout.IsChecked = false;
        chkDelivery.IsChecked = false;
        chkAlcohol.IsChecked = false;
        chkVegetarian.IsChecked = false;
        chkParking.IsChecked = false;
        chkKidFriendly.IsChecked = false;

        if (this.Parent is Popup) (this.Parent as Popup).IsOpen = false;
        return;
    }
  }



}
