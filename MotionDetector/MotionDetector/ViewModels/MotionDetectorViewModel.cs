using BasecodeLibrary.Utilities;
using LightBuzz.SMTP;
using Models.MotionDetector;
using MotionDetector.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Email;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace MotionDetector.ViewModels
{
    public class MotionDetectorViewModel : ViewModelBase
    {
        #region Bound Properties

        #region private backers
        private bool _isAlert;
        private int _sensitivity;
        private bool _autoSaveAlertImages;
        private List<byte[]> baselineImages;
        private bool _isNotRunningTest = true;
        private string _autoSaveImageLocation;
        private ObservableCollection<WriteableBitmap> _displayImages;
        private ObservableCollection<AlertDisplayImageModel> _alertDisplayImages;
        #endregion

        public bool IsAlert
        {
            get { return _isAlert; }
            set { _isAlert = value; OnPropertyChanged("IsAlert"); }
        }

        public bool IsNotRunningTest
        {
            get { return _isNotRunningTest; }
            set { _isNotRunningTest = value; OnPropertyChanged("IsNotRunningTest"); }
        }

        /// <summary>
        /// Sets the sensitivity of the image delta. If set to 10, then if any of the baseline images are greater than 10% different
        /// than the captured image, it will alert.
        /// </summary>
        public int Sensitivity
        {
            get { return _sensitivity; }
            set { _sensitivity = value; ConfigurationSettings.AppConfig.ImageDelta = value; OnPropertyChanged("Sensitivity"); }
        }
        
        public bool AutoSaveAlertImages
        {
            get { return _autoSaveAlertImages; }
            set { _autoSaveAlertImages = value; OnPropertyChanged("AutoSaveAlertImages"); }
        }

        public string AutoSaveImageLocation
        {
            get { return _autoSaveImageLocation; }
            set { _autoSaveImageLocation = value; OnPropertyChanged("AutoSaveImageLocation"); }
        }

        public ConfigModel ConfigurationSettings { get; set; }

        private MediaCapture MediaCaptureElement { get; set; }

        public WriteableBitmap SelectedAlertImage { get; set; }

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
        public ICommand UpdateSettingsCommand { get; set; }
        public ICommand RunTestsCommand { get; set; }
        public ICommand SaveImageCommand { get; set; }
        #endregion

        #endregion

        #region Unbound properties

        /// <summary>
        /// The timer that controls the image test frequency 
        /// </summary>
        private DispatcherTimer captureTimer { get; set; }

        private DispatcherTimer baselineTimer { get; set; }

        private DispatcherTimer delayTimer { get; set; }

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
            delayTimer = new DispatcherTimer();
            baselineImages = new List<byte[]>();
            captureTimer = new DispatcherTimer();
            baselineTimer = new DispatcherTimer();
            displayRequest = new DisplayRequest();
            MediaCaptureElement = new MediaCapture();
            streamList = new List<IRandomAccessStream>();
            DisplayImages = new ObservableCollection<WriteableBitmap>();
            AlertDisplayImages = new ObservableCollection<AlertDisplayImageModel>();

            RunTestsCommand = new CommandHandler(RunTestsExecuted, CanExecuteRunTest);
            SaveImageCommand = new CommandHandler(SaveImageExecuted, CanExecuteSaveImage);
            UpdateSettingsCommand = new CommandHandler(UpdateSettingsExecuted, CanExecuteUpdateSettings);
        }

        #endregion

        #region CanExecute functions

        public bool CanExecuteUpdateSettings()
        {
            return true;
        }

        public bool CanExecuteSaveImage()
        {
            return true;
        }
        
        public bool CanExecuteRunTest()
        {
            return true;
        }

        #endregion

        #region Executed functions

        private async void SaveImageExecuted()
        {
            if (SelectedAlertImage != null)
            {
                FileSavePicker picker = new FileSavePicker();
                picker.FileTypeChoices.Add("PNG File", new List<string> { ".png" });
                StorageFile destFile = await picker.PickSaveFileAsync();

                using (IRandomAccessStream stream = await destFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    Stream pixelStream = SelectedAlertImage.PixelBuffer.AsStream();
                    byte[] pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                (uint)SelectedAlertImage.PixelWidth, (uint)SelectedAlertImage.PixelHeight, 96.0, 96.0, pixels);
                    await encoder.FlushAsync();
                }
            }
        }

        private async void UpdateSettingsExecuted()
        {
            string settingsJson = JsonConvert.SerializeObject(ConfigurationSettings);
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await storageFolder.GetFileAsync("config.json");
            await FileIO.WriteTextAsync(sampleFile, settingsJson);
            Sensitivity = ConfigurationSettings.AppConfig.ImageDelta;
        }

        /// <summary>
        /// Tests the systems capture and email functionality.
        /// </summary>
        /// <param name="sender">Sending button</param>
        /// <param name="e">Button args</param>
        private async void RunTestsExecuted()
        {
            try
            {
                if (captureTimer.IsEnabled)
                {
                    captureTimer.Stop();
                }

                IsNotRunningTest = false;
                var stream = new InMemoryRandomAccessStream();
                await MediaCaptureElement.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
                await Task.Delay(10);
                List<IRandomAccessStream> streamTestList = new List<IRandomAccessStream>() { stream };
                await SendAlertEmail(streamTestList, true);
                IsNotRunningTest = true;
                captureTimer.Start();
            }
            catch (Exception) { /*Working with hardware has a lot of exception cases.... nom nom nom*/}

        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Sets up the application and initializes the camera.
        /// </summary>
        public async void Setup(CaptureElement captureElement)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            if (await FileExists("config.json"))
            {
                StorageFile sampleFile = await storageFolder.GetFileAsync("config.json");
                string file = await FileIO.ReadTextAsync(sampleFile);
                ConfigurationSettings = JsonConvert.DeserializeObject<ConfigModel>(file);
            }
            else
            {
                StorageFile sampleFile = await storageFolder.CreateFileAsync("config.json", CreationCollisionOption.ReplaceExisting);
                string settingsJson = "{\"smtpSettings\":{\"smtpRelay\":\"\",\"smtpPort\":465,\"useSSL\":true,\"smtpUserName\":\"\",\"smtpPassword\":\"\",\"recipient\":\"\",},\"appConfig\":{\"sendEmails\":false,\"captureDelay\":300,\"alertDelay\":2,\"alertThreshold\":1,\"pixelDelta\":.3,\"imageDelta\":3,}}";
                await FileIO.WriteTextAsync(sampleFile, settingsJson);

                ConfigurationSettings = JsonConvert.DeserializeObject<ConfigModel>(settingsJson);
            }

            Sensitivity = ConfigurationSettings.AppConfig.ImageDelta;

            ConfigurationSettings.SmtpSettings.PropertyChanged += SmtpSettings_PropertyChanged;

            await MediaCaptureElement.InitializeAsync();

            try
            {
                captureElement.Source = MediaCaptureElement;
            }
            catch (Exception)
            {

            }

            displayRequest.RequestActive();

            await MediaCaptureElement.StartPreviewAsync();

            baselineTimer.Interval = new TimeSpan(0, 0, 10);
            baselineTimer.Tick += OnBaselineTimerTick;
            baselineTimer.Start();

            captureTimer.Interval = new TimeSpan(0, 0, 0, 0, ConfigurationSettings.AppConfig.CaptureDelay);
            captureTimer.Tick += OnCaptureTimerTick;
        }

        public async Task<bool> Dispose()
        {

            await MediaCaptureElement.StopPreviewAsync();
            MediaCaptureElement.Dispose();
            MediaCaptureElement = null;
            baselineImages = null;
            captureTimer.Stop();

            baselineTimer.Tick -= OnBaselineTimerTick;
            captureTimer.Tick -= OnCaptureTimerTick;
            delayTimer.Tick -= OnDelayTimerTick;

            displayRequest = null;
            streamList = null;
            DisplayImages = null;
            AlertDisplayImages = null;

            UpdateSettingsCommand = null;
            RunTestsCommand = null;
            SaveImageCommand = null;
            return true;
        }

        public void UpdateSettings()
        {
            UpdateSettingsExecuted();
        }

        #endregion

        #region Private Functions

        private void SmtpSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "PreferredSmtpServer")
            {
                switch (ConfigurationSettings.SmtpSettings.PreferredSmtpServer)
                {
                    case ("Gmail"):
                        ConfigurationSettings.SmtpSettings.SmtpServer = "smtp.gmail.com";
                        ConfigurationSettings.SmtpSettings.SmtpPort = 465;
                        break;
                    case ("Yahoo"):
                        ConfigurationSettings.SmtpSettings.SmtpServer = "smtp.mail.yahoo.com";
                        ConfigurationSettings.SmtpSettings.SmtpPort = 465;
                        break;
                }
            }
        }

        private async Task<bool> FileExists(string fileName)
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);
            return item != null;
        }

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

                await lowLagCapture.FinishAsync();
            }
            catch (Exception)
            {
                // Sometimes when you serial click the capture button we get an explosion...
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

                bool isAlert = CheckForMotion(imageBytes);

                if (isAlert)
                {

                    WriteableBitmap writeableBitmap = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
                    softwareBitmap.CopyToBuffer(writeableBitmap.PixelBuffer);
                    AlertDisplayImages.Add(new AlertDisplayImageModel() { AlertDisplayImage = writeableBitmap, AlertDisplayCaption = DateTime.Now.ToString() });

                    delayTimer.Interval = new TimeSpan(0, 0, ConfigurationSettings.AppConfig.AlertDelay);

                    captureTimer.Stop();
                    delayTimer.Tick += OnDelayTimerTick;
                    delayTimer.Start();

                    // It seems silly that we need to capture a second image but the first image that was captured isn't in a format that can
                    // be easily emailed. This being the case, I decided it'd just be easier to grab another capture in the correct format and 
                    // email it off. The delta between the images is negligable 
                    var stream = new InMemoryRandomAccessStream();
                    await MediaCaptureElement.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
                    await Task.Delay(10);
                    streamList.Add(stream);

                    if (AutoSaveAlertImages)
                    {                    }

                    if (ConfigurationSettings.AppConfig.SendEmails && streamList.Count > ConfigurationSettings.AppConfig.AlertThreshold)
                    {
                        captureTimer.Stop();
                        await SendAlertEmail(streamList);
                        await Task.Delay(new TimeSpan(0, 1, 0));
                    }
                }
            }
            catch (Exception error)
            {
                // Getting random COM errors. Just eat it and continue. There's nothing I can do about this. 
            }
        }

        private void OnDelayTimerTick(object sender, object e)
        {
            DispatcherTimer delayTimer = sender as DispatcherTimer;
            if (delayTimer != null)
            {
                delayTimer.Stop();
                captureTimer.Start();
            }
        }

        /// <summary>
        /// Uses the parameters set in the config file to test the image passed in against all baseline images. If any of the baseline
        /// images match, then false is returned. Otherwise true is returned.
        /// </summary>
        /// <param name="testImage">The image you wish to test against all baseline images.</param>
        /// <returns>Returns true if none of the baseline images match the image passed in.</returns>
        private bool CheckForMotion(byte[] testImage)
        {
            List<bool> baseLineChecks = new List<bool>();
            foreach (byte[] baseline in baselineImages)
            {

                int changedPixels = 0;

                for (int x = 0; x < baseline.Length; x++)
                {
                    double pixelDelta = Convert.ToDouble(baseline[x]) / Convert.ToDouble(testImage[x]);
                    if (pixelDelta > (1.0 + ConfigurationSettings.AppConfig.PixelDelta) || pixelDelta < (1.0 - ConfigurationSettings.AppConfig.PixelDelta))
                    {
                        changedPixels++;
                    }
                }

                baseLineChecks.Add(Convert.ToDouble(changedPixels) / Convert.ToDouble(baseline.Length) > (Convert.ToDouble(Sensitivity) / 100d));
            }

            if (!baseLineChecks.Contains(false))
            {
                return IsAlert = true;
            }
            else
            {
                return IsAlert = false;
            }
        }

        private async Task<SmtpResult> SendAlertEmail(List<IRandomAccessStream> streams, bool isTest = false)
        {
            using (SmtpClient client = new SmtpClient(ConfigurationSettings.SmtpSettings.SmtpServer,
                                                      ConfigurationSettings.SmtpSettings.SmtpPort,
                                                      ConfigurationSettings.SmtpSettings.UseSSL,
                                                      ConfigurationSettings.SmtpSettings.SmtpUserName,
                                                      ConfigurationSettings.SmtpSettings.SmtpPassword))
            {
                EmailMessage emailMessage = new EmailMessage();

                emailMessage.Importance = EmailImportance.High;
                emailMessage.Sender.Address = "IoTAlertApp@donotreply.com";
                emailMessage.Sender.Name = "IoT App";
                emailMessage.To.Add(new EmailRecipient(ConfigurationSettings.SmtpSettings.Recipient));
                emailMessage.Subject = "ALERT | MOTION DETECTED";

                if (isTest)
                    emailMessage.Subject = "TEST | ALERT | MOTION DETECTED";

                emailMessage.Body = "Alert detected at " + DateTime.Now;

                int imageCount = 1;
                foreach (IRandomAccessStream stream in streams)
                {
                    string fileName = "image_" + imageCount + ".png";
                    RandomAccessStreamReference reference = RandomAccessStreamReference.CreateFromStream(stream);
                    emailMessage.Attachments.Add(new EmailAttachment(fileName, reference));
                    imageCount++;
                }

                streams.Clear();
                SmtpResult result = await client.SendMailAsync(emailMessage);

                if (!isTest)
                    captureTimer.Start();

                return result;
            }
        }

        #endregion
    }
}
