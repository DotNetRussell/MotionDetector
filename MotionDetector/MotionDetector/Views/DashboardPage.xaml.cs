using Models.MotionDetector;
using MotionDetector.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
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
    public sealed partial class DashboardPage : Page
    {        
        /// <summary>
             /// This is the list of images that will be emailed to the recipient once the threshold has been met.
             /// </summary>
        private List<IRandomAccessStream> streamList { get; set; }

        public ConfigModel ConfigurationSettings { get; set; }

        private MotionDetectorViewModel viewModel { get; set; }

        public DashboardPage()
        {
            this.InitializeComponent();
        }
        private void Setup()
        {
            viewModel = new MotionDetectorViewModel(); ;
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
            Setup();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        // This is probably the dumbest thing ever....
        // Command binding isn't working currently on buttons and flyout menu items.... 
        // Not sure why but this is a quick work around.
        private void OnSaveContextMenuClicked(object sender, RoutedEventArgs e)
        {
            viewModel.SaveImageCommand.Execute(sender);
        }

    }
}
