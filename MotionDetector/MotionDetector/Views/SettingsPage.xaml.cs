using MotionDetector.Models;
using MotionDetector.Utilities;
using MotionDetector.ViewModels;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

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

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
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
    }
}
