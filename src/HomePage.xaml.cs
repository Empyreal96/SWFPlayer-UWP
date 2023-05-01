using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SWF_Player
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            MainPageHeader.Text = "Welcome, use the controls below to open a flash file (.swf)\n\nNote: Some flash files are not supported and will not load (See \"About\" page for more info)";
            Helpers.PlayerRunningTextBox = PlayerRunningText;
        }

        private async void OpenImageRect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".swf");

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }
            if (Helpers.PlayerWebView != null)
            {
                Helpers.PlayerWebView.Navigate(new Uri("about:blank"));
            }
            //var copy = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, "loadedfile.swf", NameCollisionOption.ReplaceExisting);
            FileInfoText.Text = Path.GetFileName(file.Path);
            StorageFolder assets = await Package.Current.InstalledLocation.GetFolderAsync("swf2js");
            var copy = await file.CopyAsync(assets, "loadedfile.swf", NameCollisionOption.ReplaceExisting);
            Debug.WriteLine(copy.Path);

            Helpers.SelectedFile = copy.Path;

            PlayBtn.Visibility = Visibility.Visible;

           
        }

        private void RefreshContent_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Helpers.PlayerWebView != null)
            {
                Helpers.PlayerWebView.Navigate(new Uri("about:blank"));
                Helpers.PlayerWebView.Navigate(new Uri(@"ms-appx-web:///swf2js/index.html"));
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Helpers.MainPageHeader.Text = "Home";
            Helpers.MainNavigationListView.SelectedIndex = 0;
        }

        private void PlayBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pageName = $"SWF_Player.PlayerPage";
            var pageType = Type.GetType(pageName);

            Helpers.PlayerFrame.Navigate(pageType);
        }
    }
}
