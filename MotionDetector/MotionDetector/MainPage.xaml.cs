using LightBuzz.SMTP;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Data.Json;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Collections.ObjectModel;
using MotionDetector.ViewModels;
using BasecodeLibrary.Controls;
using RussLib.Pages;
using MotionDetector.Views;
using MotionDetector.Utilities;

namespace MotionDetector
{
    public sealed partial class MainPage : BaseCodePageContainer, INotifyPropertyChanged
    {

        private Visibility _AdVisibility;

        public Visibility AdVisibility
        {
            get { return _AdVisibility; }
            set { _AdVisibility = value; OnPropertyChanged("AdVisibility"); }
        }


        public MainPage()
        {
            AdVisibility = Visibility.Visible;

            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }
        
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
            MainDisplayFrame.CacheSize = 0;
            MainDisplayFrame.Navigate(typeof(DashboardPage));

//For DEMO only
#if DEBUG
            await StoreServices.SetupDemoStore();
#endif 
            StoreServices.CheckFreemiumStatus();
            AdVisibility = StoreServices.RemoveAds ? Visibility.Collapsed : Visibility.Visible;

            this.DataContext = this;
        }

        private async void NavigationPane_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            switch (args.InvokedItem)
            {
                case ("Dashboard"):
                    MainDisplayFrame.Navigate(typeof(DashboardPage));
                    break;
                case ("About"):
                    MainDisplayFrame.Navigate(typeof(AboutPage));
                    break;
                case ("Remove Ads"):
                    AdVisibility = await StoreServices.OpenStoreRemoveAds() ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case ("Tutorial"):
                    Uri youtubeTutorial = new Uri(@"https://youtu.be/EpaH1thk4IA");
                    await Windows.System.Launcher.LaunchUriAsync(youtubeTutorial);
                    break;
                default:
                    if(args.InvokedItem is NavigationViewItem)
                    {
                        if((args.InvokedItem as NavigationViewItem).Content as String == "Settings")
                        {
                            MainDisplayFrame.Navigate(typeof(SettingsPage));
                            break;
                        }
                    }
                    MainDisplayFrame.Navigate(typeof(DashboardPage));
                    break;
            }
        }
    }
}
