using MotionDetector.Models;
using MotionDetector.Utilities;
using MotionDetector.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MotionDetector.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private TutorialModel _tutorialModels = null;
        public SettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = new SettingsViewModel();
            this.Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _tutorialModels = await ConfigurationServices.GetTutorialLinks();
        }

        private async void BrowseToYoutubeTutorial(object sender, RoutedEventArgs e)
        {
            Uri youtubeTutorial;
            if (_tutorialModels != null)
            {
                youtubeTutorial = new Uri(_tutorialModels.TutorialLinkTwo);
            }
            else
            {
                youtubeTutorial = new Uri(@"https://youtu.be/EpaH1thk4IA");
            }
            await Windows.System.Launcher.LaunchUriAsync(youtubeTutorial);
        }

        private async void BrowseToGmailSecurity(object sender, RoutedEventArgs e)
        {
            Uri gmailSecurityUri = new Uri(@"https://myaccount.google.com/lesssecureapps");            
            await Windows.System.Launcher.LaunchUriAsync(gmailSecurityUri);
        }
    }
}
