using BaseCodeLibrary.Utilities;
using Windows.UI.Xaml.Media.Imaging;

namespace MotionDetector.Models
{
    public class AlertDisplayImageModel : ModelBase
    {
        private string _alertDisplayCaption;
        private WriteableBitmap _alertDisplayImage; 

        public WriteableBitmap AlertDisplayImage
        {
            get { return _alertDisplayImage; }
            set { _alertDisplayImage = value; OnPropertyChanged(nameof(AlertDisplayImage)); }
        }

        public string AlertDisplayCaption
        {
            get { return _alertDisplayCaption; }
            set { _alertDisplayCaption = value; OnPropertyChanged(nameof(AlertDisplayCaption)); }
        }
    }
}
