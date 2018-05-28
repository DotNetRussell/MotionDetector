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

        public bool SendEmails
        {
            get { return _sendEmails; }
            set { _sendEmails = value; OnPropertyChanged("SendEmails"); }
        }

        public int CaptureDelay
        {
            get { return _captureDelay; }
            set { _captureDelay = value; OnPropertyChanged("CaptureDelay"); }
        }

        public int AlertDelay
        {
            get { return _alertDelay; }
            set { _alertDelay = value; OnPropertyChanged("AlertDelay"); }
        }

        public int AlertThreshold
        {
            get { return _alertThreshold; }
            set { _alertThreshold = value; OnPropertyChanged("AlertThreshold"); }
        }

        public double PixelDelta
        {
            get { return _pixelDelta; }
            set { _pixelDelta = value; OnPropertyChanged("PixelDelta"); }
        }

        public int ImageDelta
        {
            get { return _imageDelta; }
            set { _imageDelta = value; OnPropertyChanged("ImageDelta"); }
        }
    }
}
