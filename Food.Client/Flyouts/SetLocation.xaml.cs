using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.DPE.ReferenceApps.Food.Client.Flyouts
{
    public sealed partial class SetLocation : UserControl
    {
        public SetLocation()
        {
            this.InitializeComponent();
            Loaded += SetLocation_Loaded;
        }

        void SetLocation_Loaded(object sender, RoutedEventArgs e)
        {
            this.UsingMockData.Visibility = AppData.Current.UseMockData ? Visibility.Visible : Visibility.Collapsed;
            LocationListBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            CurrentLocationTextBox.Text = string.IsNullOrWhiteSpace(AppData.Current.Location) ? "UNKNOWN" : AppData.Current.Location;
            NewLocationTextBox.Text = string.Empty;
            EndAsync();
        }

        public event EventHandler LocationChanged;
        private async Task RaiseLocationChangedAsync(Lib.Services.BingMaps.SearchHelper.Resource resource)
        {
            await AppData.Current.SetLocationAsync(resource);
            // IANBE: I just set the location why do I want to refresh it?
            //await AppData.Current.LoadDataAsync(true);
            await AppData.Current.LoadDataAsync(false);
            CurrentLocationTextBox.Text = AppData.Current.Location;
            if (LocationChanged != null)
                LocationChanged(this, EventArgs.Empty);
        }

        private void StartAsync()
        {
            LocationProgressRing.Visibility = Visibility.Visible;
        }

        private void EndAsync()
        {
            LocationProgressRing.Visibility = Visibility.Collapsed;
        }

        async void LocateMeButton_Click_1(object sender, RoutedEventArgs e)
        {
            // find by current
            StartAsync();

            var _Location = await Unity.GetCurrentLocationAsync();
            if (_Location == null)
            { /* TODO: what if none */ }
            else
            {

                var _Name = await Unity.CoordinateToStringAsync(_Location.Coordinate.Latitude, _Location.Coordinate.Longitude);
                if (_Name.Count() == 0)
                { /* TODO: what if none */ }

                await RaiseLocationChangedAsync(_Name.First());
            }
            EndAsync();
        }

        async void SetLocationButton_Click_1(object sender, RoutedEventArgs e)
        {
            // find by string 
            StartAsync();

            var _String = NewLocationTextBox.Text.Trim();
            NewLocationTextBox.Text = string.Empty;
            if (!string.IsNullOrWhiteSpace(_String))
            {
                try
                {
                    var _Location = await Unity.StringToCoordinateAsync(_String);
                    if (_Location.Count() == 0)
                        throw new Exception("Not found");
                    if (_Location.Count() > 1)
                    {
                        LocationListBox.ItemsSource = _Location;
                        LocationListBox.Visibility = Visibility.Visible;
                        EndAsync();
                        return;
                    }
                    await RaiseLocationChangedAsync(_Location.First());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("SetLocationButton_Click_1:" + ex.Message);
                    CurrentLocationTextBox.Text = "Not found";
                }
            }
            EndAsync();

        }

        private void LocationListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            LocationListBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            RaiseLocationChangedAsync(LocationListBox.SelectedItem as Lib.Services.BingMaps.SearchHelper.Resource);
        }
    }
}
