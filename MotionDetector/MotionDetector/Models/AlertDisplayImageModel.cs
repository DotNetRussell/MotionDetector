using BaseCodeLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace MotionDetector.Models
{
    public class AlertDisplayImageModel : ModelBase
    {
        private WriteableBitmap _alertDisplayImage; 

        public WriteableBitmap AlertDisplayImage
        {
            get { return _alertDisplayImage; }
            set { _alertDisplayImage = value; OnPropertyChanged("AlertDisplayImage"); }
        }

        private string _alertDisplayCaption;

        public string AlertDisplayCaption
        {
            get { return _alertDisplayCaption; }
            set { _alertDisplayCaption = value; OnPropertyChanged("AlertDisplayCaption"); }
        }
    }
}
