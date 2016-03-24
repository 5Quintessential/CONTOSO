using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class Settings : UserControl
    {
        public Settings()
        {
            this.DataContext = Lib.Models.AppData.Current;
            this.InitializeComponent();
            // Will not bind if not using Mode=TwoWay which we do not want
            this.MaxDistance.Value = Lib.Models.AppData.Current.NearMeMaxDistance;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Windows.UI.ApplicationSettings.SettingsPane.Show();
        }

        private void SetNearMeMaxDistanceBtn_Click(object sender, RoutedEventArgs e)
        {
            Lib.Models.AppData.Current.NearMeMaxDistance = (int)this.MaxDistance.Value;
            Lib.Models.AppData.Current.LoadDataAsync(false);
        }
    }
}
