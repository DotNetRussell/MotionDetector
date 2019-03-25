using BasecodeLibrary.Utilities;
using LightBuzz.SMTP;
using Models.MotionDetector;
using MotionDetector.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Email;

namespace MotionDetector.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private bool _isNotRunningTest = true;
        public ICommand RunTestsCommand { get; set; }
        public ICommand UpdateSettingsCommand { get; set; }
        public List<string> ServerOptions { get { return new List<string>() { "Gmail", "Yahoo", "Custom" }; } }
        public ConfigModel ConfigurationSettings { get; set; }
        public bool IsNotRunningTest
        {
            get { return _isNotRunningTest; }
            set { _isNotRunningTest = value; OnPropertyChanged("IsNotRunningTest"); }
        }

        public SettingsViewModel()
        {
            Setup();
        }

        ~SettingsViewModel()
        {
            this.ConfigurationSettings.AppConfig.PropertyChanged -= SettingsViewModel_PropertyChanged;
            this.ConfigurationSettings.SmtpSettings.PropertyChanged -= SettingsViewModel_PropertyChanged;
        }

       
        private async void Setup()
        {
            RunTestsCommand = new CommandHandler(RunSMTPTestExecuted);
            UpdateSettingsCommand = new CommandHandler(UpdateSettingsExecuted);

            ConfigurationSettings = await ConfigurationServices.GetConfig();
            OnPropertyChanged("ConfigurationSettings");
            this.ConfigurationSettings.AppConfig.PropertyChanged += SettingsViewModel_PropertyChanged;
            this.ConfigurationSettings.SmtpSettings.PropertyChanged += SettingsViewModel_PropertyChanged;
        }
        
        private void RunSMTPTestExecuted()
        {
            IsNotRunningTest = false;
            SMTPServices.RunSMTPTest(ConfigurationSettings, () => { IsNotRunningTest = true; });
        }

        private void SettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {                       
            if (e.PropertyName == "PreferredSmtpServer")
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

        private void UpdateSettingsExecuted()
        {
            ConfigurationServices.SaveConfig(ConfigurationSettings);
        }
    }
}
