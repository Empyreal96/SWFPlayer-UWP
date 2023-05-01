using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using WebViewComponents;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.ViewManagement;
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


    public sealed partial class PlayerPage : Page
    {
        public static string htmlPage;
        public static WebView PlayerView;
        InjectedInputMouseInfo inputInfo = new InjectedInputMouseInfo();

        public PlayerPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            StaticHandlers.Notify += CheckNotifyData;
            StaticHandlers.XEvents += (s, e) =>
            {
                try
                {
                    var xEventData = (XEventData)e;
                    if (xEventData != null)
                    {
                        if (xEventData.type == "loaded")
                        {
                            Debug.WriteLine("Page Loaded");
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            };
            StaticHandlers.AddToConsole += (s, e) =>
            {
                var consoleOutput = (ConsoleOutput)e;
                Debug.WriteLine($"[Type] {consoleOutput.type}\n[Message] {consoleOutput.message}\n\n");


            };
            wsadToggle.Toggled += WsadToggle_Checked;
            Start();

        }

        private void WsadToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (wsadToggle.IsOn == true)
            {
                isArrows = false;

            }
            else
            {
                isArrows = true;

            }

        }

        private void Start()
        {
            if (Helpers.SelectedFile != null)
            {

                if (Helpers.SelectedFile.Contains(".swf"))
                {
                    PlayerView = new WebView(WebViewExecutionMode.SeparateThread);
                    Helpers.PlayerWebView = PlayerView;
                    PlayerView.LostFocus += PlayerView_LostFocus;
                    PlayerView.NavigationCompleted += PlayerView_NavigationCompleted;
                    PlayerView.NavigationStarting += PlayerView_NavigationStarting;
                    PlayerView.Holding += PlayerView_Holding;
                    PlayerView.RightTapped += PlayerView_RightTapped;
                    PlayerView.ContextRequested += PlayerView_ContextRequested;
                    //PlayerView.IsHoldingEnabled = false;
                    PlayerGrid.Children.Add(PlayerView);
                    Grid.SetRow(PlayerView, 0);

                    PlayerView.Navigate(new Uri(@"ms-appx-web:///swf2js/index.html"));
                    Helpers.PlayerRunningTextBox.Text = "Player is running";
                    Helpers.MainPageHeader.Text = "Player (Running)";
                }
                else
                {
                    ErrorDialog.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ErrorDialog.Visibility = Visibility.Visible;
            }
        }

        private void PlayerView_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            args.Handled = true;
        }

        private void PlayerView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void PlayerView_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void PlayerView_LostFocus(object sender, RoutedEventArgs e)
        {
            PlayerView.Focus(FocusState.Programmatic);
        }

        private void PlayerView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            //PlayerView.AddWebAllowedObject("console", new ConsoleOverride());
        }

        private async void PlayerView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {

            var imagesScript = @"function MountContextMenuToImages(){ var imgs = document.images;
                                    for (var i = 0; i < imgs.length; i++) {
                                        if (!imgs[i].hasAttribute('xevent'))
                                        {
                                            imgs[i].setAttribute('xevent', 'ready');
                                            imgs[i].oncontextmenu = function(e) 
                                                {
                                                    var elementHTML = e.target.outerHTML;
                                                    console.notify('longclick', 'image|#|' + (this.hasAttribute('src') ? this.src : '') + '|#|' + (this.hasAttribute('alt') ? this.alt : '') + '|#|' + e.clientX + '|#|' + e.clientY + '|#|' + elementHTML);
                                                    return false;
                                                }
                                         }
                                        }
                                       }
                                    MountContextMenuToImages();";

            var linksScript = @"function MountContextMenuToLinks(){ var links = document.links;
                                    for (var i = 0; i < links.length; i++) {
                                    var innerImages = links[i].getElementsByTagName('img');
                                    if (!links[i].hasAttribute('xevent') && (innerImages.length == 0 || window.location.hostname.includes('youtube.com')))
                                    {
                                        links[i].setAttribute('xevent', 'ready');
                                        links[i].oncontextmenu = function(e) {
                                             var elementHTML = e.target.outerHTML;
                                             console.notify('longclick', 'link|#|' + (this.hasAttribute('href') ? this.href : '') + '|#|' + this.innerHTML + '|#|' + e.clientX + '|#|' + e.clientY + '|#|' + elementHTML);
                                             return false;
                                    }
                                  }
                                }
                              }
                            MountContextMenuToLinks();";

            var videosScript = @"function MountContextMenuToVideos(){var videos = document.getElementsByTagName('video');
for (var i = 0; i < videos.length; i++) {
    if (!videos[i].hasAttribute('xevent'))
    {
        videos[i].setAttribute('xevent', 'ready');
        videos[i].oncontextmenu = function(e) {
            var sources = this.getElementsByTagName('source');
            var sourceLink = sources.length > 0 ? sources[0].src : (this.hasAttribute('src') ? this.src : '');
            var elementHTML = e.target.outerHTML;
            console.notify('longclick', 'video|#|' + sourceLink + '|#|' + '' + '|#|' + e.clientX + '|#|' + e.clientY + '|#|' + elementHTML);
            return false;
        }
    }
}
}
MountContextMenuToVideos();
";

            var documentScript = @"function getSelectionText() { var text = ''; if (window.getSelection) { text = window.getSelection().toString(); } else if (document.selection && document.selection.type != 'Control') {
                                   text = document.selection.createRange().text; }return text; }
                                        document.oncontextmenu = function(e) {MountContextMenuToVideos(); MountContextMenuToLinks(); MountContextMenuToImages(); var targetText = getSelectionText(); var elementHTML=e.target.outerHTML; 
                                        if(targetText.length > 0){ console.notify('longclick', 'text|#|' + '' + '|#|' + targetText + '|#|' + e.clientX + '|#|' + e.clientY + '|#|' + elementHTML); }
                                        else{ if(e.target.nodeName != 'IMG' && e.target.nodeName != 'VIDEO' && e.target.nodeName != 'A'){ 
                                        console.notify('longclick', 'element|#|' + '' + '|#|' + targetText + '|#|' + e.clientX + '|#|' + e.clientY + '|#|' + elementHTML); } } return false;}
                                       ";



            var DocumentLoadedHandler = "var xeventsPageLoaderMonitor = setInterval(function () { if (document.readyState === 'complete'){ clearInterval(xeventsPageLoaderMonitor); xevents.loaded(); } },1000);";

            var scrollHandler = "var W10MScrollPreState = false, scrollState = false; var ScrollLoaderMonitor = setInterval(function () { if(W10MScrollPreState!=scrollState){ W10MScrollPreState = scrollState; xevents.address(scrollState); } },500); window.onscroll = function(e){ MountContextMenuToVideos(); MountContextMenuToLinks(); MountContextMenuToImages();  scrollState = this.oldScroll > this.scrollY; this.oldScroll = this.scrollY; }";


            await PlayerView.InvokeScriptAsync("eval", new string[] { linksScript });
            await PlayerView.InvokeScriptAsync("eval", new string[] { imagesScript });
            await PlayerView.InvokeScriptAsync("eval", new string[] { videosScript });
            await PlayerView.InvokeScriptAsync("eval", new string[] { DocumentLoadedHandler });
            await PlayerView.InvokeScriptAsync("eval", new string[] { documentScript });
        }

        private async void CheckNotifyData(object sender, EventArgs e)
        {
            //Debug.WriteLine("Reached CheckNotifyData");
            if (e.GetType() == typeof(NotifyData))
            {

                NotifyData notifyData = (NotifyData)e;
                if (notifyData != null)
                {
                    Debug.WriteLine(notifyData.type);
                }
            }
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Helpers.MainPageHeader.Text = "Player";
            Helpers.MainNavigationListView.SelectedIndex = 1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ErrorDialog.Visibility = Visibility.Collapsed;
        }



        bool isArrows = false;

        private async void UpButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (isArrows)
            {
                holdingDebug.Text = "Tapped Up Arrow (38)";

                 string keyEvent = "simulateKey(38, \"press\");";
               // string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 38; e.keyCode = 38; $(\"input\").trigger(e);";
                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
            else
            {
                holdingDebug.Text = "Tapped W Key (87)";

                string keyEvent = "simulateKey(87, \"press\");";
               // string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 87; e.keyCode = 87; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });

            }
        }

        private async void UpButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            holdingDebug.Text = "Holding";
            if (isArrows == true)
            {
                isHolding = true;
                holdingDebug.Text = "Holding Up Arrow (38)";

                string keyEvent = "simulateKey(38, \"down\");";
                //string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 38; e.keyCode = 38; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
            else
            {
                isHolding = true;
                holdingDebug.Text = "Holding W Key (87)";

                string keyEvent = "simulateKey(87, \"down\");";
                //string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 87; e.keyCode = 87; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
        }

        private async void DownButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (isArrows)
            {
                holdingDebug.Text = "Tapped Right Arro (40)";

                string keyEvent = "simulateKey(40, \"press\");";
               // string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 40; e.keyCode = 40; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
            else
            {
                holdingDebug.Text = "Tapped A Key (83)";

                string keyEvent = "simulateKey(83, \"press\");";
               // string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 83; e.keyCode = 83; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
        }

        private async void DownButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (isArrows)
            {
                isHolding = true;
                holdingDebug.Text = "Holding Down Arrow (40)";

                string keyEvent = "simulateKey(40, \"down\");";
                //string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 40; e.keyCode = 40; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
            else
            {
                isHolding = true;
                holdingDebug.Text = "Holding S Key (83)";

                 string keyEvent = "simulateKey(83, \"down\");";

                //string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 83; e.keyCode = 83; $(\"input\").trigger(e);";
                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
        }

        private async void LeftButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (isArrows)
            {
                holdingDebug.Text = "Tapped Left Arrow (37)";

                string keyEvent = "simulateKey(37, \"press\");";
               // string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 37; e.keyCode = 37; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
            else
            {
                holdingDebug.Text = "Tapped A Key (65)";

                string keyEvent = "simulateKey(65, \"press\");";
               // string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 65; e.keyCode = 65; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
        }

        private async void LeftButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (isArrows)
            {
                isHolding = true;
                holdingDebug.Text = "Holding Left Arrow (37)";

                string keyEvent = "simulateKey(37, \"down\");";
                //string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 37; e.keyCode = 37; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
            else
            {
                isHolding = true;
                holdingDebug.Text = "Holding A Key (65)";

                string keyEvent = "simulateKey(65, \"down\");";
               // string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 65; e.keyCode = 65; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
        }

        private async void RightButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (isArrows)
            {
                holdingDebug.Text = "Tapped Arrow Right (39)";

                string keyEvent = "simulateKey(39, \"press\");";
                //string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 39; e.keyCode = 39; $(\"input\").trigger(e);";
                ///
                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
            else
            {
                holdingDebug.Text = "Tapped D Key (68)";

                string keyEvent = "simulateKey(68, \"press\");";
                //string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 68; e.keyCode = 68; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
        }

        private async void RightButton_Holding(object sender, HoldingRoutedEventArgs e)
        {

            if (isArrows)
            {
                isHolding = true;
                holdingDebug.Text = "Holding Arrow Right (39)";

                string keyEvent = "simulateKey(39, \"down\");";
                //string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 39; e.keyCode = 39; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
            else
            {
                holdingDebug.Text = "Holding D Key (68)";

                isHolding = true;
                string keyEvent = "simulateKey(68, \"down\");";
                //string keyEvent = "var e = jQuery.Event(\"keydown\"); e.which = 68; e.keyCode = 68; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
        }

        bool isHolding = false;
        private async void ButtonW()
        {
           
        }
        private async void ButtonWHolding()
        {
          
        }
        private async void ButtonS()
        {
           
        }
        private async void ButtonSHolding()
        {
            
        }
        private async void ButtonA()
        {
            
        }
        private async void ButtonAHolding()
        {
           
        }
        private async void ButtonD()
        {
          

        }
        private async void ButtonDHolding()
        {
           
        }
        private async void Action1()
        {

        }
        private async void Action1Holding()
        {
            isHolding = true;

        }
        private async void Action2()
        {

        }
        private async void Action2Holding()
        {
            isHolding = true;

        }
        private async void Action3()
        {

        }
        private async void Action3Holding()
        {
            isHolding = true;

        }
        private async void Action4()
        {

        }
        private async void Action4Holding()
        {
            isHolding = true;

        }

        private async void UpButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!isArrows)
            {
                holdingDebug.Text = "Exited";

                string keyEvent = "simulateKey(87, \"up\");";
                //string keyEvent = "var e = jQuery.Event(\"keyup\"); e.which = 87; e.keyCode = 87; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });

                isHolding = false;

            }
            else
            {
                holdingDebug.Text = "Exited";

                string keyEvent = "simulateKey(68, \"up\");";
               // string keyEvent = "var e = jQuery.Event(\"keyup\"); e.which = 38; e.keyCode = 38; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }

        }

        private async void DownButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!isArrows)
            {
                holdingDebug.Text = "Exited";

                string keyEvent = "simulateKey(83, \"up\");";
               // string keyEvent = "var e = jQuery.Event(\"keyup\"); e.which = 83; e.keyCode = 83; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });

                isHolding = false;

            }
            else
            {
                holdingDebug.Text = "Exited";

                string keyEvent = "simulateKey(68, \"up\");";
                //string keyEvent = "var e = jQuery.Event(\"keyup\"); e.which = 40; e.keyCode = 40; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
        }

        private async void LeftButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!isArrows)
            {
                holdingDebug.Text = "Exited";

                string keyEvent = "simulateKey(65, \"up\");";
                //string keyEvent = "var e = jQuery.Event(\"keyup\"); e.which = 65; e.keyCode = 65; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });

                isHolding = false;

            }
            else
            {
                holdingDebug.Text = "Exited";

                string keyEvent = "simulateKey(68, \"up\");";
                //string keyEvent = "var e = jQuery.Event(\"keyup\"); e.which = 37; e.keyCode = 37; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
        }

        private async void RightButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!isArrows)
            {
                holdingDebug.Text = "Exited";

                string keyEvent = "simulateKey(68, \"up\");";
                //string keyEvent = "var e = jQuery.Event(\"keyup\"); e.which = 68; e.keyCode = 68; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
                isHolding = false;

            } else
            {
                holdingDebug.Text = "Exited";

                string keyEvent = "simulateKey(68, \"up\");";
                //string keyEvent = "var e = jQuery.Event(\"keyup\"); e.which = 39; e.keyCode = 39; $(\"input\").trigger(e);";

                await PlayerView.InvokeScriptAsync("eval", new string[] { keyEvent });
            }
        }

        private void UpButton_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void DownButton_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void LeftButton_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void RightButton_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }
    }
}
