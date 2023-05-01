using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SWF_Player
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Helpers.PlayerFrame = AppFrame;
            Helpers.MainPageHeader = MainHeaderText;
            Helpers.MainNavigationListView = NavigationListView;

            NavigationCacheMode = NavigationCacheMode.Enabled;
            var pageName = $"SWF_Player.HomePage";
            var pageType = Type.GetType(pageName);

            AppFrame.Navigate(pageType);
        }

        private void SplitView_PaneClosed(SplitView sender, object args)
        {
           // containerOverlay.Visibility = Visibility.Collapsed;
        }

        private void menuToggleButton_Click(object sender, RoutedEventArgs e)
        {
           // mainContainer.IsPaneOpen = !mainContainer.IsPaneOpen;
            if (mainContainer.IsPaneOpen)
            {
                mainContainer.IsPaneOpen = false;
            } else
            {
                mainContainer.IsPaneOpen = true;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainContainer.IsPaneOpen = false;
        }

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationCacheMode = NavigationCacheMode.Enabled;
            var pageName = $"SWF_Player.HomePage";
            var pageType = Type.GetType(pageName);
            AppFrame.Navigate(pageType);
        }

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            NavigationCacheMode = NavigationCacheMode.Enabled;
            var pageName = $"SWF_Player.PlayerPage";
            var pageType = Type.GetType(pageName);
            AppFrame.Navigate(pageType);
        }

        private void ListViewItem_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            NavigationCacheMode = NavigationCacheMode.Enabled;
            var pageName = $"SWF_Player.SettingsPage";
            var pageType = Type.GetType(pageName);
            AppFrame.Navigate(pageType);
        }

        private void AboutPageItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationCacheMode = NavigationCacheMode.Enabled;
            var pageName = $"SWF_Player.AboutPage";
            var pageType = Type.GetType(pageName);
            AppFrame.Navigate(pageType);
        }
    }
}
