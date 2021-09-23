using Models.MotionDetector;
using MotionDetector.Models;
using MotionDetector.Utilities;
using MotionDetector.ViewModels;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace MotionDetector.Views
{
    public sealed partial class DashboardPage : Page
    {
        public ConfigModel ConfigurationSettings { get; set; }
        private MotionDetectorViewModel viewModel { get; set; }

        public ICommand InitializeCaptureSinkCommand
        {
            get { return (ICommand)GetValue(InitializeCaptureSinkProperty); }
            set { SetValue(InitializeCaptureSinkProperty, value); }
        }

        public ICommand SaveImageCommand
        {
            get { return (ICommand)GetValue(SaveImageCommandProperty); }
            set { SetValue(SaveImageCommandProperty, value); }
        }

        public AlertDisplayImageModel SelectedImage
        {
            get { return (AlertDisplayImageModel)GetValue(SelectedImageProperty); }
            set { SetValue(SelectedImageProperty, value); }
        }

        public static readonly DependencyProperty SelectedImageProperty =
            DependencyProperty.Register(nameof(SelectedImage), typeof(AlertDisplayImageModel), typeof(DashboardPage), new PropertyMetadata(null));

        public static readonly DependencyProperty SaveImageCommandProperty =
            DependencyProperty.Register(nameof(SaveImageCommand), typeof(ICommand), typeof(DashboardPage), new PropertyMetadata(null));

        public static readonly DependencyProperty InitializeCaptureSinkProperty =
            DependencyProperty.Register(nameof(InitializeCaptureSinkCommand), typeof(ICommand), typeof(DashboardPage), new PropertyMetadata(null));

        public DashboardPage()
        {
            this.InitializeComponent();
            App.Current.LeavingBackground += Current_LeavingBackground;
            App.Current.EnteredBackground += Current_EnteredBackground;

            this.DataContextChanged += DashboardPage_DataContextChanged;
        }

        private void DashboardPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            SetBinding(InitializeCaptureSinkProperty, new Binding() { Path = new PropertyPath("InitializeCaptureSinkCommand") });
            SetBinding(SaveImageCommandProperty, new Binding() { Path = new PropertyPath("SaveImageCommand") });
            SetBinding(SelectedImageProperty, new Binding() { Path = new PropertyPath("SelectedAlertImage"), Mode = BindingMode.TwoWay });
        }

        //**-------------**********************************************--------------**//
        // We need to reinitialize the capture sink every time someone minimizes and maxamizes the window
        // This is because UWP -- Internals --- Limitations --- etc --- #HacksAndDuctTape 
        private void Current_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            InitializeCaptureSinkCommand?.Execute(sender);
        }
        private void Current_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            InitializeCaptureSinkCommand?.Execute(sender);
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
            SaveImageCommand?.Execute(sender);
        }

        private void alertImageListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if(e.OriginalSource is Image image)
            {
                SelectedImage = image.DataContext as AlertDisplayImageModel;
            }
        }
    }
}
