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
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MotionDetector.ViewModels
{
    public class MotionDetectorViewModel : ViewModelBase
    {
        #region Bound Properties

        #region private backers
        private int configVersion = 2;
        private bool _isAlert;
        private List<byte[]> baselineImages;
        private ObservableCollection<WriteableBitmap> _displayImages;
        private ObservableCollection<AlertDisplayImageModel> _alertDisplayImages;
        #endregion

        public bool IsAlert
        {
            get { return _isAlert; }
            set { _isAlert = value; OnPropertyChanged("IsAlert"); }
        }

        public ConfigModel ConfigurationSettings { get; set; }

        public MediaCapture MediaCaptureElement { get; set; }

        public AlertDisplayImageModel SelectedAlertImage { get; set; }

        /// <summary>
        /// The baseline images are what each newly captured image will be compared to.
        /// </summary>
        public ObservableCollection<WriteableBitmap> DisplayImages
        {
            get { return _displayImages; }
            set { _displayImages = value; OnPropertyChanged("DisplayImages"); }
        }

        public ObservableCollection<AlertDisplayImageModel> AlertDisplayImages
        {
            get { return _alertDisplayImages; }
            set { _alertDisplayImages = value; OnPropertyChanged("AlertDisplayImages"); }
        }

        #region Command Bindings
        public ICommand SaveImageCommand { get; set; }
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
        
        private LowLagPhotoCapture lowLagCapture { get; set; }


        /// <summary>
        /// This is the list of images that will be emailed to the recipient once the threshold has been met.
        /// </summary>
        private List<IRandomAccessStream> streamList { get; set; }

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
        }


        public void Destroyer()
        {
            MediaCaptureElement.StopPreviewAsync();
            MediaCaptureElement.Dispose();
            MediaCaptureElement = null;
            baselineImages = null;
            captureTimer.Stop();

            baselineTimer.Tick -= OnBaselineTimerTick;
            captureTimer.Tick -= OnCaptureTimerTick;

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

        /// <summary>
        /// Sets up the application and initializes the camera.
        /// </summary>
        public async void Setup(CaptureElement captureElement)
        {
            // If the element isn't null and it's just not streaming, that means we're still initializing it
            // don't reinitialize it or it'll explode. This is a side effect of the backgrounding shit
            // we need to do
            if(MediaCaptureElement != null 
                && MediaCaptureElement.CameraStreamState == CameraStreamState.NotStreaming)
            {
                return;
            }

            MediaCaptureElement = new MediaCapture();
            DisplayRequest _displayRequest = new DisplayRequest();

            //make request to put in active state
            _displayRequest.RequestActive();

            ConfigModel config = new ConfigModel()
            {
                SmtpSettings = new SmtpSettingsModel()
                {
                    SmtpPort = 465,
                    UseSSL = true,
                },
                AppConfig = new AppConfigModel()
                {
                    SendEmails = false,
                    ImageDelta = 7,
                    PixelDelta = .3,
                    AlertThreshold = 2,
                    AlertDelay = 2,
                    CaptureDelay = 500,
                    DarkShiftThreshold = 70,
                    ConfigVersion = this.configVersion
                }
            };
            try
            {
                if (!await SaveManager.FileExists("config.json"))
                {
                    await SaveManager.SaveJsonFile("config.json", config);
                }

                // Apparently there's an edge case where if you create a config file and then attempt to read it right away,
                // it won't exist yet. Doing this little wait and see check fixes it. This also only happens on first run.
                while (!await SaveManager.FileExists("config.json")) { continue; }

                ConfigurationSettings = await SaveManager.GetJsonFile<ConfigModel>("config.json");

                if (ConfigurationSettings.AppConfig.ConfigVersion < this.configVersion)
                {
                    await SaveManager.SaveJsonFile("config.json", config);
                }


                await MediaCaptureElement.InitializeAsync();
                captureElement.Source = MediaCaptureElement;
                displayRequest.RequestActive();

                await MediaCaptureElement.StartPreviewAsync();

                baselineTimer.Interval = new TimeSpan(0, 0, 10);
                baselineTimer.Tick += OnBaselineTimerTick;
                baselineTimer.Start();

                captureTimer.Interval = new TimeSpan(0, 0, 0, 0, ConfigurationSettings.AppConfig.CaptureDelay);
                captureTimer.Tick += OnCaptureTimerTick;
            }
            catch
            {
                //So for some reason on initial launch there's some race condition that happens in the above block
                //it only happens on the initial launch and it only happens once.
                //it's late, I'm sick, and I'm done trying to figure out the depths of this platform
                //recursive call fixes it 
                //<light match, drop match, walk away>
                Setup(captureElement);
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
                lowLagCapture =
                    await MediaCaptureElement.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));

                CapturedPhoto capturedPhoto = await lowLagCapture.CaptureAsync();
                SoftwareBitmap softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;

                await lowLagCapture.FinishAsync();

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

        /// <summary>
        /// Captures a photo and sends it off for analysis 
        /// </summary>
        private async void TakePhoto()
        {
            try
            {
                lowLagCapture =
                    await MediaCaptureElement.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));

                CapturedPhoto capturedPhoto = await lowLagCapture.CaptureAsync();
                SoftwareBitmap softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;
                await lowLagCapture.FinishAsync();


                byte[] imageBytes = new byte[4 * softwareBitmap.PixelWidth * softwareBitmap.PixelHeight];
                softwareBitmap.CopyToBuffer(imageBytes.AsBuffer());

                bool isAlert = IsAlert = MotionServices.CheckForMotion(ConfigurationSettings, imageBytes, baselineImages);


                if (isAlert)
                {
                    WriteableBitmap writeableBitmap = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
                    softwareBitmap.CopyToBuffer(writeableBitmap.PixelBuffer);
                    AlertDisplayImages.Add(new AlertDisplayImageModel() { AlertDisplayImage = writeableBitmap, AlertDisplayCaption = DateTime.Now.ToString() });

                    captureTimer.Stop();
                    await Task.Delay(new TimeSpan(0, 0, ConfigurationSettings.AppConfig.AlertDelay));
                    captureTimer.Start();


                    // It seems silly that we need to capture a second image but the first image that was captured isn't in a format that can
                    // be easily emailed. This being the case, I decided it'd just be easier to grab another capture in the correct format and 
                    // email it off. The delta between the images is negligable 
                    var stream = new InMemoryRandomAccessStream();
                    await MediaCaptureElement.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
                    await Task.Delay(10);
                    streamList.Add(stream);
                    
                    if (ConfigurationSettings.AppConfig.SendEmails && streamList.Count > ConfigurationSettings.AppConfig.AlertThreshold)
                    {
                        captureTimer.Stop();
                        await SMTPServices.SendAlertEmail(streamList, ConfigurationSettings);
                        await Task.Delay(new TimeSpan(0, 1, 0));
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
