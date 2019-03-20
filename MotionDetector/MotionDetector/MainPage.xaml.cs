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

namespace MotionDetector
{
    public sealed partial class MainPage : BaseCodePageContainer
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
            MainDisplayFrame.CacheSize = 0;
            MainDisplayFrame.Navigate(typeof(DashboardPage));
        }

        private void NavigationPane_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            switch (args.InvokedItem)
            {
                case ("Dashboard"):
                    MainDisplayFrame.Navigate(typeof(DashboardPage));
                    break;
                case ("About"):
                    MainDisplayFrame.Navigate(typeof(AboutPage));
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
