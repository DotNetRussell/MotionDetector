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
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Models.MotionDetector;
using MotionDetector.ViewModels;
using BasecodeLibrary.Controls;
using RussLib.Pages;
using Windows.UI.Xaml.Navigation;

namespace MotionDetector
{
    public sealed partial class MainPage : BaseCodePageContainer
    {
        /// <summary>
        /// This is the list of images that will be emailed to the recipient once the threshold has been met.
        /// </summary>
        private List<IRandomAccessStream> streamList { get; set; }

        public ConfigModel ConfigurationSettings { get; set; }

        private MotionDetectorViewModel viewModel { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Setup()
        {
            viewModel = new MotionDetectorViewModel();
            this.DataContext = viewModel;

            // As much as I hate doing this, we have to inject the capture element control becase
            // the MediaCapture object can't initialize until it has a "Sink" (the capture element)
            // to dump images into. However, if you just bind to it, it attempts to initialize 
            // prior to binding. Chicken and egg problem. 
            //
            // The alternative was to create a ContentControl and bind to a CaptureElement maintained
            // in the viewmodel. This sounds worse than just injecting it into the setup function.
            viewModel.Setup(captureElementControl);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Frame.BackStack.Clear();
            Setup();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            this.DataContext = null;
            await viewModel.Dispose();
            viewModel = null;
            rateReminder = null;
            base.OnNavigatingFrom(e);
        }

        private void OnAboutButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

        // This is probably the dumbest thing ever....
        // Command binding isn't working currently on buttons and flyout menu items.... 
        // Not sure why but this is a quick work around.
        private void OnSaveContextMenuClicked(object sender, RoutedEventArgs e)
        {
            viewModel.SaveImageCommand.Execute(sender);            
        }

        private void UpdateSettingsClosed(object sender, object e)
        {
            viewModel.UpdateSettings();
        }

        private async void TutorialButtonClicked(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            var uriBing = new Uri(@"http://www.bing.com");

            // Launch the URI
            await Windows.System.Launcher.LaunchUriAsync(uriBing);
        }
    }
}
