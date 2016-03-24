using Microsoft.DPE.ReferenceApps.Food.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed partial class About : UserControl
    {
        private TeamViewModel m_viewModel;
        private Uri _SupportUri = new Uri("mailto:dpesamples@microsoft.com?subject=Win8Food%20Sample%20App");
        private Uri _PrivacyUri = new Uri("http://Win8Food.azurewebsites.net/Privacy.aspx");
        private Uri _CodeUri = new Uri("http://sourceforge.net/projects/win8fooddinings/");

        public About()
        {
            this.m_viewModel = new TeamViewModel();
            this.DataContext = this.m_viewModel;
            this.InitializeComponent();
        }

        private void AboutBack_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.ApplicationSettings.SettingsPane.Show();

        }

        private void YelpLogo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri(@"http://www.yelp.com"));       
        }

        private void OpenMenuLogo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri(@"http://www.openmenu.com"));  
        }

        private void UICentricLogo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri(@"http://www.uicentric.net")); 
        }

        private void FactualLogo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri(@"http://www.factual.com")); 
        }

        private void DPELogo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri(@"http://aka.ms/DPEWin8"));
        }

        private void TeamList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != e.AddedItems && 0 < e.AddedItems.Count)
                    this.m_viewModel.CurrentMember = e.AddedItems[0] as TeamMemberViewModel;
            else
                this.TeamMemberImage.Visibility = Visibility.Collapsed;
        }

        private async void PrivacyClick(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(this._PrivacyUri);
        }

        private async void SupportClick(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(this._SupportUri);
        }

        private async void CodeClick(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(this._CodeUri);            
        }
    }
}
