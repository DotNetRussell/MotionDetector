using MotionDetector.ViewModels;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace MotionDetector.Views
{
    public sealed partial class HiddenModeDashboardPage : Page, INotifyPropertyChanged
    {
        private string _destinationAddress;

        public event PropertyChangedEventHandler PropertyChanged;

        public string DestinationAddress
        {
            get { return _destinationAddress; }
            set { _destinationAddress = value; OnPropertyChanged(nameof(DestinationAddress)); }
        }

        public HiddenModeDashboardPage()
        {
            this.InitializeComponent();          
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = this;
            webBrowser.NavigationCompleted += WebBrowser_NavigationCompleted;
            AddressTextBox.KeyDown += AddressTextBox_KeyDown;

            hiddenDashboard.Navigate(typeof(DashboardPage), false);
            DestinationAddress = "https://www.google.com";
            NavigateForwardImp();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            hiddenDashboard.Navigate(typeof(BlankPage));
            webBrowser.NavigationCompleted -= WebBrowser_NavigationCompleted;
            AddressTextBox.KeyDown -= AddressTextBox_KeyDown;
            DataContext = null;
            webBrowser = null;
            base.OnNavigatedFrom(e);
        }

        private void WebBrowser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            DestinationAddress = webBrowser.Source.AbsoluteUri;
        }

        private void AddressTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ( e.Key == Windows.System.VirtualKey.Enter)
            {
                NavigateForwardImp();
            }
        }

        private void NavigateForward(object sender, RoutedEventArgs e)
        {
            NavigateForwardImp();
        }

        private void NavigateForwardImp()
        {
            if (!string.IsNullOrEmpty(DestinationAddress))
            {
                if (DestinationAddress.Contains("http", StringComparison.InvariantCultureIgnoreCase)
                    || DestinationAddress.Contains("https", StringComparison.InvariantCultureIgnoreCase))
                {
                    webBrowser.Navigate(new Uri(DestinationAddress));
                }
                else
                {
                    DestinationAddress = "https://" + DestinationAddress;
                    webBrowser.Navigate(new Uri(DestinationAddress));
                }
            }
        }
        private void NavigateBack(object sender, RoutedEventArgs e)
        {
            if (webBrowser.CanGoBack)
            {
                webBrowser.GoBack();
            }
        }


        private void OnPropertyChanged(string prop)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(prop));
            }
        }

        private void ToggleHiddenDashboard(object sender, RoutedEventArgs e)
        {
            if (hiddenDashboard.Visibility == Visibility.Visible)
            {
                webBrowser.Visibility = Visibility.Visible;
                hiddenDashboard.Visibility = Visibility.Collapsed;
            }
            else
            {
                webBrowser.Visibility = Visibility.Collapsed;
                hiddenDashboard.Visibility = Visibility.Visible;
            }
        }
    }
}
