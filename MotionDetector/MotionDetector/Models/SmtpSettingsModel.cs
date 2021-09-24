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
        private string _preferredSmtpServer;

        public string PreferredSmtpServer
        {
            get { return _preferredSmtpServer; }
            set { _preferredSmtpServer = value; OnPropertyChanged(nameof(PreferredSmtpServer)); }
        }

        public string SmtpServer
        {
            get { return _smtpRelay; }
            set { _smtpRelay = value; OnPropertyChanged(nameof(SmtpServer)); }
        }

        public int SmtpPort
        {
            get { return _smtpPort; }
            set { _smtpPort = value; OnPropertyChanged(nameof(SmtpPort)); }
        }

        public bool UseSSL
        {
            get { return _useSSL; }
            set { _useSSL = value; OnPropertyChanged(nameof(UseSSL)); }
        }

        public string SmtpUserName
        {
            get { return _smtpUserName; }
            set { _smtpUserName = value; OnPropertyChanged(nameof(SmtpUserName)); }
        }

        public string SmtpPassword
        {
            get { return _smtpPassword; }
            set { _smtpPassword = value; OnPropertyChanged(nameof(SmtpPassword)); }
        }

        public string Recipient
        {
            get { return _recipient; }
            set { _recipient = value; OnPropertyChanged(nameof(Recipient)); }
        }
    }
}
