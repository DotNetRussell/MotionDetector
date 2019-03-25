using Models.MotionDetector;
using MotionDetector.Utilities;
using MotionDetector.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MotionDetector.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {        

        public ConfigModel ConfigurationSettings { get; set; }

        private MotionDetectorViewModel viewModel { get; set; }

        public DashboardPage()
        {
            this.InitializeComponent();
            App.Current.LeavingBackground += Current_LeavingBackground;
            App.Current.EnteredBackground += Current_EnteredBackground;
        }

        //**-------------**********************************************--------------**//
        // We need to reinitialize the capture sink every time someone minimizes and maxamizes the window
        // This is because UWP -- Internals --- Limitations --- etc --- #HacksAndDuctTape 
        private void Current_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            viewModel.InitializeCameraAndSink();
        }
        private void Current_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            viewModel.InitializeCameraAndSink();
        }
        //*****************************************************************************//

        private void Setup()
        {
            viewModel = new MotionDetectorViewModel(); 
            this.DataContext = viewModel;
           
            viewModel.Setup();
            this.captureElementControl.Content = viewModel.caputureSink;

            if (StoreServices.IsPremium)
            {
                AlertButton.Visibility = Visibility.Visible;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Setup();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            (this.DataContext as MotionDetectorViewModel).Destroyer();
            App.Current.LeavingBackground -= Current_LeavingBackground;
            App.Current.EnteredBackground -= Current_EnteredBackground;
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
