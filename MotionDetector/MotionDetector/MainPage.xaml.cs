using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.ComponentModel;
using RussLib.Pages;
using MotionDetector.Views;
using MotionDetector.Utilities;
using Windows.UI.Popups;
using MotionDetector.Models;
using Windows.ApplicationModel.Store;

namespace MotionDetector
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private bool _premiumFeatures;
        private TutorialModel _tutorialModels = null;

        private Visibility _AdVisibility;
        private Visibility _PremiumVersionVisibility;

        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility AdVisibility
        {
            get { return _AdVisibility; }
            set { _AdVisibility = value; OnPropertyChanged(nameof(AdVisibility)); }
        }

        public Visibility PremiumVersionVisibility
        {
            get { return _PremiumVersionVisibility; }
            set { _PremiumVersionVisibility = value; OnPropertyChanged(nameof(PremiumVersionVisibility)); }
        }

        public bool PremiumFeatures
        {
            get { return _premiumFeatures; }
            set
            {
                _premiumFeatures = value;
                PremiumVersionVisibility = _premiumFeatures ? Visibility.Collapsed : Visibility.Visible;
                OnPropertyChanged(nameof(PremiumFeatures));
            }
        }

        public MainPage()
        {
            AdVisibility = Visibility.Visible;

            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            MainDisplayFrame.Navigate(typeof(DashboardPage));

            try
            {
                await StoreServices.SetupStoreServices();

                LicenseInformation LicenseInformation = CurrentApp.LicenseInformation;
                StoreServices.CheckFreemiumStatus();
                StoreServices.CheckForPremiumStatus();

                AdVisibility = StoreServices.RemoveAds || StoreServices.IsPremium ? Visibility.Collapsed : Visibility.Visible;
                PremiumFeatures = StoreServices.IsPremium;
   
                _tutorialModels = ConfigurationServices.GetTutorialLinks();
            }
            catch
            {
                //if this fails we want to eat it silently. 
                //we have a backup for tutorials null and 
                //the user doesn't care about telemetry
            }

            this.DataContext = this;
        }

        private async void ShowPayWall()
        {
            MessageDialog dialog = new MessageDialog(String.Empty);
            dialog.Commands.Add(new UICommand("Go Premium") { Id = 0 });
            dialog.Commands.Add(new UICommand("Stay Free but Weeeaaaakkk") { Id = 1 });
            dialog.Content = "In order to use this feature you must have the Premium Version.";
            dialog.Title = "Woah, hold on partner!";
            var result = await dialog.ShowAsync();
            if(result != null && (int)result.Id == 0 )
            {
                PremiumFeatures = await StoreServices.OpenStorePurchasePremium();
                AdVisibility = StoreServices.RemoveAds || StoreServices.IsPremium ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private async void NavigationPane_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            switch (args.InvokedItem)
            {
                case "Unlock Premium Version $1.99":
                    PremiumFeatures = await StoreServices.OpenStorePurchasePremium();
                    AdVisibility = StoreServices.RemoveAds || StoreServices.IsPremium ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case "Go into Hidden Mode":
                    if (!PremiumFeatures)
                    {
                        ShowPayWall();
                    }
                    else
                    {
                        MainDisplayFrame.Navigate(typeof(HiddenModeDashboardPage));
                    }
                    break;
                case "Select Alert Sounds":
                    if (!PremiumFeatures)
                    {
                        ShowPayWall();
                    }
                    else
                    {
                        MainDisplayFrame.Navigate(typeof(AlertSoundsPage));
                    }
                    break;
                case "Schedule Active Hours":
                    if (!PremiumFeatures)
                    {
                        ShowPayWall();
                    }
                    break;
                case "Define Custom Alert Area":
                    if (!PremiumFeatures)
                    {
                        ShowPayWall();
                    }
                    break;
                case "Dashboard":
                    if (MainDisplayFrame.CurrentSourcePageType != typeof(DashboardPage) )
                    {
                        MainDisplayFrame.Navigate(typeof(DashboardPage));
                    }
                    break;
                case "About":
                    MainDisplayFrame.Navigate(typeof(AboutPage));
                    break;
                case "Remove Ads $.99":
                    AdVisibility = await StoreServices.OpenStoreRemoveAds() ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case "Tutorial":
                    Uri youtubeTutorial;
                    if (_tutorialModels != null)
                    {
                        youtubeTutorial = new Uri(_tutorialModels.TutorialLinkOne);
                    }
                    else
                    {
                        youtubeTutorial = new Uri(@"https://youtu.be/EpaH1thk4IA");
                    }
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
                    else if (MainDisplayFrame.CurrentSourcePageType != typeof(DashboardPage))
                    {
                        MainDisplayFrame.Navigate(typeof(DashboardPage));
                    }
                    break;
            }
        }

        public void OnPropertyChanged(String prop)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
