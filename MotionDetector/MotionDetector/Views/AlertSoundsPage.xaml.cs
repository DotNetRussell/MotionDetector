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
    public sealed partial class AlertSoundsPage : Page
    {
        public AlertSoundsPage()
        {
            this.InitializeComponent();
            this.DataContext = new SoundSetupViewModel();
        }

        private async void OpenFreeSoundWebsite(object sender, RoutedEventArgs e)
        {
            Uri youtubeTutorial = new Uri(@"https://freesound.org/");
            await Windows.System.Launcher.LaunchUriAsync(youtubeTutorial);
        }
    }
}
