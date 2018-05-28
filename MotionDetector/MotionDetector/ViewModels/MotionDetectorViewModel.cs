using BasecodeLibrary.Utilities;
using LightBuzz.SMTP;
using Models.MotionDetector;
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
        // Bound properties

        /// <summary>
        /// Sets the sensitivity of the image delta. If set to 10, then if any of the baseline images are greater than 10% different
        /// than the captured image, it will alert.
        /// </summary>
        private int _sensitivity;

        public int Sensitivity
        {
            get { return _sensitivity; }
            set { _sensitivity = value; ConfigurationSettings.AppConfig.ImageDelta = value; OnPropertyChanged("Sensitivity"); }
        }

        private MediaCapture MediaCaptureElement { get; set; }

        /// <summary>
        /// The baseline images are what each newly captured image will be compared to.
        /// </summary>
        private List<byte[]> baselineImages;

        private ObservableCollection<WriteableBitmap> _displayImages;

        public ObservableCollection<WriteableBitmap> DisplayImages
        {
            get { return _displayImages; }
            set { _displayImages = value; OnPropertyChanged("DisplayImages"); }
        }

        private ObservableCollection<WriteableBitmap> _alertDisplayImages;

        public ObservableCollection<WriteableBitmap> AlertDisplayImages
        {
            get { return _alertDisplayImages; }
            set { _alertDisplayImages = value; OnPropertyChanged("AlertDisplayImages"); }
        }

        public WriteableBitmap SelectedAlertImage { get; set; }

        private bool _isAlert;

        public bool IsAlert
        {
            get { return _isAlert; }
            set { _isAlert = value; OnPropertyChanged("IsAlert"); }
        }

        public ICommand UpdateSettingsCommand { get; set; }
        public ICommand RunTestsCommand { get; set; }
        public ICommand SaveImageCommand { get; set; }

        /// <summary>
        /// The timer that controls the image test frequency 
        /// </summary>
        private DispatcherTimer captureTimer { get; set; }

        private DisplayRequest displayRequest { get; set; }
        private LowLagPhotoCapture lowLagCapture { get; set; }

        /// <summary>
        /// This is the list of images that will be emailed to the recipient once the threshold has been met.
        /// </summary>
        private List<IRandomAccessStream> streamList { get; set; }

        public ConfigModel ConfigurationSettings { get; set; }

        public MotionDetectorViewModel()
        {

            MediaCaptureElement = new MediaCapture();
            baselineImages = new List<byte[]>();
            captureTimer = new DispatcherTimer();
            displayRequest = new DisplayRequest();
            streamList = new List<IRandomAccessStream>();
            DisplayImages = new ObservableCollection<WriteableBitmap>();
            AlertDisplayImages = new ObservableCollection<WriteableBitmap>();

            UpdateSettingsCommand = new CommandHandler(UpdateSettingsExecuted, true);
            RunTestsCommand = new CommandHandler(RunTestsExecuted, true);
            SaveImageCommand = new CommandHandler(SaveImageExecuted, true);
        }

        public async Task<bool> FileExists(string fileName)
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);
            return item != null;
        }

        DispatcherTimer baselineTimer = new DispatcherTimer();

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

            await MediaCaptureElement.InitializeAsync();

            captureElement.Source = MediaCaptureElement;

            displayRequest.RequestActive();

            await MediaCaptureElement.StartPreviewAsync();

            baselineTimer.Interval = new TimeSpan(0, 0, 10);
            baselineTimer.Tick += BaselineTimer_Tick;
            baselineTimer.Start();

            captureTimer.Interval = new TimeSpan(0, 0, 0, 0, ConfigurationSettings.AppConfig.CaptureDelay);
            captureTimer.Tick += CaptureTimer_Tick;
        }

        private void CaptureTimer_Tick(object sender, object e)
        {
            TakePhoto();
        }

        private void BaselineTimer_Tick(object sender, object e)
        {
            if(captureTimer == null)
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
        /// Tests the systems capture and email functionality.
        /// </summary>
        /// <param name="sender">Sending button</param>
        /// <param name="e">Button args</param>
        private async void RunTestsExecuted()
        {
            if (captureTimer.IsEnabled)
            {
                captureTimer.Stop();
            }

            var stream = new InMemoryRandomAccessStream();
            await MediaCaptureElement.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
            await Task.Delay(10);
            List<IRandomAccessStream> streamTestList = new List<IRandomAccessStream>() { stream };
            await SendAlertEmail(streamTestList, true);

            captureTimer.Start();
        }

        /// <summary>
        /// Orders the capture of a baseline image.
        /// </summary>
        /// <param name="sender">Sending button</param>
        /// <param name="e">Button args</param>
        private void BaseLineClicked(object sender, RoutedEventArgs e)
        {
            if (captureTimer.IsEnabled)
            {
                captureTimer.Stop();
            }
            CaptureBaseLineImage();
            captureTimer.Start();
        }

        DispatcherTimer delayTimer = new DispatcherTimer();
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
                    AlertDisplayImages.Add(writeableBitmap);

                    delayTimer.Interval = new TimeSpan(0, 0, ConfigurationSettings.AppConfig.AlertDelay);

                    captureTimer.Stop();
                    delayTimer.Tick += DelayTimer_Tick;
                    delayTimer.Start();

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
                        await SendAlertEmail(streamList);
                        await Task.Delay(new TimeSpan(0, 1, 0));
                    }
                }
            }
            catch (Exception)
            {
                // Getting random COM errors. Just eat it and continue. There's nothing I can do about this. 
            }
        }

        private void DelayTimer_Tick(object sender, object e)
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
            using (SmtpClient client = new SmtpClient(ConfigurationSettings.SmtpSettings.SmtpRelay,
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

        public async Task<bool> Dispose()
        {
            
            await MediaCaptureElement.StopPreviewAsync();
            MediaCaptureElement.Dispose();
            MediaCaptureElement = null;
            baselineImages = null;
            captureTimer.Stop();

            baselineTimer.Tick -= BaselineTimer_Tick;
            captureTimer.Tick -= CaptureTimer_Tick;
            delayTimer.Tick -= DelayTimer_Tick;

            displayRequest = null;
            streamList = null;
            DisplayImages = null;
            AlertDisplayImages = null;

            UpdateSettingsCommand = null;
            RunTestsCommand = null;
            SaveImageCommand = null;
            return true;
        }
    }
}
