using BaseCodeLibrary.Utilities;

namespace MotionDetector.Models
{
    public class SmtpSettingsModel : ModelBase
    {
        private string _smtpRelay;
        private int _smtpPort;
        private bool _useSSL;
        private string _smtpUserName;
        private string _smtpPassword;
        private string _recipient;

        public string SmtpRelay
        {
            get { return _smtpRelay; }
            set { _smtpRelay = value; OnPropertyChanged("SmtpRelay"); }
        }

        public int SmtpPort
        {
            get { return _smtpPort; }
            set { _smtpPort = value; OnPropertyChanged("SmtpPort"); }
        }

        public bool UseSSL
        {
            get { return _useSSL; }
            set { _useSSL = value; OnPropertyChanged("UseSSL"); }
        }

        public string SmtpUserName
        {
            get { return _smtpUserName; }
            set { _smtpUserName = value; OnPropertyChanged("SmtpUserName"); }
        }

        public string SmtpPassword
        {
            get { return _smtpPassword; }
            set { _smtpPassword = value; OnPropertyChanged("SmtpPassword"); }
        }

        public string Recipient
        {
            get { return _recipient; }
            set { _recipient = value; OnPropertyChanged("Recipient"); }
        }

    }
}
