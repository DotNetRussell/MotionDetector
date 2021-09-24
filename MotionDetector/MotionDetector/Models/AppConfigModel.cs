using BaseCodeLibrary.Utilities;

namespace MotionDetector.Models
{

    public class AppConfigModel : ModelBase
    {
        private bool _sendEmails;
        private int _captureDelay;
        private int _alertDelay;
        private int _alertThreshold;
        private double _pixelDelta;
        private int _imageDelta;
        private double _darkShiftThreshold;
        private int _configVersion;
        private string _selectedCameraId;

        public int ConfigVersion
        {
            get { return _configVersion; }
            set { _configVersion = value; }
        }

        public double DarkShiftThreshold
        {
            get { return _darkShiftThreshold; }
            set { _darkShiftThreshold = value; OnPropertyChanged(nameof(DarkShiftThreshold)); }
        }

        public bool SendEmails
        {
            get { return _sendEmails; }
            set { _sendEmails = value; OnPropertyChanged(nameof(SendEmails)); }
        }

        public int CaptureDelay
        {
            get { return _captureDelay; }
            set { _captureDelay = value; OnPropertyChanged(nameof(CaptureDelay)); }
        }

        public int AlertDelay
        {
            get { return _alertDelay; }
            set { _alertDelay = value; OnPropertyChanged(nameof(AlertDelay)); }
        }

        public int AlertThreshold
        {
            get { return _alertThreshold; }
            set { _alertThreshold = value; OnPropertyChanged(nameof(AlertThreshold)); }
        }

        public double PixelDelta
        {
            get { return _pixelDelta; }
            set { _pixelDelta = value; OnPropertyChanged(nameof(PixelDelta)); }
        }

        public int ImageDelta
        {
            get { return _imageDelta; }
            set { _imageDelta = value; OnPropertyChanged(nameof(ImageDelta)); }
        }

        public string SelectedCameraId
        {
            get { return _selectedCameraId; }
            set { _selectedCameraId = value; OnPropertyChanged(nameof(SelectedCameraId)); }
        }
    }
}
