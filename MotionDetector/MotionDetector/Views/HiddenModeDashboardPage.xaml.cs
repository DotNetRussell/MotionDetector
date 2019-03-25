using MotionDetector.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MotionDetector.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HiddenModeDashboardPage : Page, INotifyPropertyChanged
    {
        private string _destinationAddress;

        public event PropertyChangedEventHandler PropertyChanged;

        public string DestinationAddress
        {
            get { return _destinationAddress; }
            set { _destinationAddress = value; OnPropertyChanged("DestinationAddress"); }
        }

        public HiddenModeDashboardPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            webBrowser.NavigationCompleted += WebBrowser_NavigationCompleted;
            this.AddressTextBox.KeyDown += AddressTextBox_KeyDown;

            hiddenDashboard.Navigate(typeof(DashboardPage));
            DestinationAddress = "https://www.google.com";
            NavigateForwardImp();
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
            if(hiddenDashboard.Visibility == Visibility.Visible)
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
