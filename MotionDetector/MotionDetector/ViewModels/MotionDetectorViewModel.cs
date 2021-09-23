using BasecodeLibrary.Utilities;
using Models.MotionDetector;
using MotionDetector.Models;
using MotionDetector.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MotionDetector.ViewModels
{
    public class MotionDetectorViewModel : ViewModelBase
    {
        #region Bound Properties

        #region private backers
        private bool _isAlert;
        private bool _isAlertSoundRunning = false;
        private List<byte[]> baselineImages;
        private AlertDisplayImageModel selectedAlertImage;
        private ObservableCollection<WriteableBitmap> _displayImages;
        private ObservableCollection<AlertDisplayImageModel> _alertDisplayImages;
        #endregion

        public bool IsAlert
        {
            get { return _isAlert; }
            set { _isAlert = value; OnPropertyChanged(nameof(IsAlert)); }
        }

        public ConfigModel ConfigurationSettings { get; set; }

        public MediaCapture MediaCaptureElement { get; set; }

        public AlertDisplayImageModel SelectedAlertImage { get => selectedAlertImage; set { selectedAlertImage = value; OnPropertyChanged(nameof(SelectedAlertImage)); } }

        /// <summary>
        /// The baseline images are what each newly captured image will be compared to.
        /// </summary>
        public ObservableCollection<WriteableBitmap> DisplayImages
        {
            get { return _displayImages; }
            set { _displayImages = value; OnPropertyChanged(nameof(DisplayImages)); }
        }

        public ObservableCollection<AlertDisplayImageModel> AlertDisplayImages
        {
            get { return _alertDisplayImages; }
            set { _alertDisplayImages = value; OnPropertyChanged(nameof(AlertDisplayImages)); }
        }

        #region Command Bindings
        public ICommand SaveImageCommand { get; set; }
        public ICommand StopAlertSoundCommand { get; set; }
        public ICommand InitializeCaptureSinkCommand { get; set; }
        #endregion

        #endregion

        #region Unbound properties

        /// <summary>
        /// The timer that controls the image test frequency 
        /// </summary>
        private DispatcherTimer captureTimer { get; set; }

        /// <summary>
        /// The timer that controls the baseline image capture frequency 
        /// </summary>
        private DispatcherTimer baselineTimer { get; set; }

        private DisplayRequest displayRequest { get; set; }

        /// <summary>
        /// This is the list of images that will be emailed to the recipient once the threshold has been met.
        /// </summary>
        private List<IRandomAccessStream> streamList { get; set; }

        #endregion

        #region Variables

        //This is the secret sauce
        public CaptureElement caputureSink = new CaptureElement();

        #endregion

        #region Constructors

        public MotionDetectorViewModel()
        {
            baselineImages = new List<byte[]>();
            captureTimer = new DispatcherTimer();
            baselineTimer = new DispatcherTimer();
            displayRequest = new DisplayRequest();
            streamList = new List<IRandomAccessStream>();
            DisplayImages = new ObservableCollection<WriteableBitmap>();
            AlertDisplayImages = new ObservableCollection<AlertDisplayImageModel>();

            SaveImageCommand = new CommandHandler(SaveImageExecuted);
            StopAlertSoundCommand = new CommandHandler(StopAlertSoundExecuted);
            InitializeCaptureSinkCommand = new CommandHandler(InitializeCameraAndSink);
        }


        public void Destroyer()
        {
            MediaCaptureElement?.StopPreviewAsync();
            MediaCaptureElement?.Dispose();
            MediaCaptureElement = null;
            baselineImages = null;
            captureTimer?.Stop();

            if (baselineTimer != null && captureTimer != null)
            {
                //for some reason I can't use the null check operator with events? Super weird 
                baselineTimer.Tick -= OnBaselineTimerTick;
                captureTimer.Tick -= OnCaptureTimerTick;
            }

            displayRequest = null;
            streamList = null;
            DisplayImages = null;
            AlertDisplayImages = null;

            SaveImageCommand = null;
        }

        #endregion

        #region Executed functions

        private void SaveImageExecuted()
        {
            CameraServices.SaveImage(SelectedAlertImage);
        }


        #endregion

        #region Public Functions

        public async void InitializeCameraAndSink()
        {
            try
            {
                // If the element isn't null and it's just not streaming, that means we're still initializing it
                // don't reinitialize it or it'll explode. This is a side effect of the backgrounding shit
                // we need to do
                if (MediaCaptureElement != null
                    && MediaCaptureElement.CameraStreamState == CameraStreamState.NotStreaming)
                {
                    return;
                }

                MediaCaptureElement = new MediaCapture();
                DisplayRequest _displayRequest = new DisplayRequest();

                //make request to put in active state
                _displayRequest.RequestActive();

                await MediaCaptureElement.InitializeAsync();
                caputureSink.Source = MediaCaptureElement;

                await MediaCaptureElement.StartPreviewAsync();
            }
            catch { }
        }

        /// <summary>
        /// Sets up the application and initializes the camera.
        /// </summary>
        public async void Setup()
        {
            try
            {

                ConfigurationSettings = await ConfigurationServices.GetConfig();

                InitializeCameraAndSink();

                baselineTimer.Interval = new TimeSpan(0, 0, 3);
                baselineTimer.Tick += OnBaselineTimerTick;
                baselineTimer.Start();

                captureTimer.Interval = new TimeSpan(0, 0, 0, 0, ConfigurationSettings.AppConfig.CaptureDelay);
                captureTimer.Tick += OnCaptureTimerTick;
            }
            catch (Exception error)
            {
                //So for some reason on initial launch there's some race condition that happens in the above block
                //it only happens on the initial launch and it only happens once.
                //it's late, I'm sick, and I'm done trying to figure out the depths of this platform
                //recursive call fixes it 
                //<light match, drop match, walk away>
                Setup();
            }
        }


        #endregion

        #region Private Functions


        private void OnCaptureTimerTick(object sender, object e)
        {
            TakePhoto();
        }

        private void OnBaselineTimerTick(object sender, object e)
        {
            if (captureTimer == null)
            {
                return;
            }

            if (captureTimer.IsEnabled)
            {
                captureTimer.Stop();
            }

            CaptureBaseLineImage();
            captureTimer.Start();
        }

        /// <summary>
        /// Will capture a single image and store it in the baseline images list. Each image will be used to determine if we have an alert or not.
        /// </summary>
        private async void CaptureBaseLineImage()
        {
            try
            {
                SoftwareBitmap softwareBitmap = await CameraServices.CaptureImage(MediaCaptureElement);

                WriteableBitmap writeableBitmap = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
                softwareBitmap.CopyToBuffer(writeableBitmap.PixelBuffer);

                byte[] imageBytes = new byte[4 * softwareBitmap.PixelWidth * softwareBitmap.PixelHeight];
                softwareBitmap.CopyToBuffer(imageBytes.AsBuffer());

                if (baselineImages.Count > 6)
                {
                    baselineImages.Clear();
                    DisplayImages.Clear();
                }

                baselineImages.Add(imageBytes);
                DisplayImages.Add(writeableBitmap);

            }
            catch (Exception error)
            {
                // Sometimes when you serial capture we get an explosion because the hardware isn't ready...
                // Eat it and move on
            }
        }

        private void StopAlertSoundExecuted()
        {
            _isAlertSoundRunning = false;
        }

        private async void PlayAlertSound()
        {
            if (StoreServices.IsPremium && ConfigurationSettings.SoundConfig.PlaySounds && !_isAlertSoundRunning)
            {
                if (!ConfigurationSettings.SoundConfig.PlayContinuous)
                {
                    await SoundsServices.PlayAlertSound(ConfigurationSettings.SoundConfig.SoundName);
                }
                else
                {
                    _isAlertSoundRunning = true;

                    while (_isAlertSoundRunning)
                    {
                        await SoundsServices.PlayAlertSound(ConfigurationSettings.SoundConfig.SoundName);
                        await Task.Delay(new TimeSpan(0, 0, ConfigurationSettings.SoundConfig.ContinuousSecondDelay));
                    }
                }
            }
        }

        /// <summary>
        /// Captures a photo and sends it off for analysis 
        /// </summary>
        private async void TakePhoto()
        {
            try
            {
                SoftwareBitmap softwareBitmap = await CameraServices.CaptureImage(MediaCaptureElement);

                byte[] imageBytes = new byte[4 * softwareBitmap.PixelWidth * softwareBitmap.PixelHeight];
                softwareBitmap.CopyToBuffer(imageBytes.AsBuffer());

                bool isAlert = IsAlert = MotionServices.CheckForMotion(ConfigurationSettings, imageBytes, baselineImages);

                if (isAlert)
                {
                    PlayAlertSound();

                    WriteableBitmap writeableBitmap = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
                    softwareBitmap.CopyToBuffer(writeableBitmap.PixelBuffer);

                    AlertDisplayImages.Add(new AlertDisplayImageModel() { AlertDisplayImage = writeableBitmap, AlertDisplayCaption = DateTime.Now.ToString() });

                    captureTimer.Stop();
                    await Task.Delay(new TimeSpan(0, 0, ConfigurationSettings.AppConfig.AlertDelay));
                    captureTimer.Start();

                    InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();

                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    encoder.SetSoftwareBitmap(softwareBitmap);
                    await encoder.FlushAsync();
                    streamList.Add(stream);

                    if (ConfigurationSettings.AppConfig.SendEmails && streamList.Count > ConfigurationSettings.AppConfig.AlertThreshold)
                    {
                        await SMTPServices.SendAlertEmail(streamList, ConfigurationSettings);
                    }
                }
            }
            catch (Exception error)
            {
                // Getting random COM errors. Just eat it and continue. There's nothing I can do about this. 
            }
        }


        #endregion
    }
}
