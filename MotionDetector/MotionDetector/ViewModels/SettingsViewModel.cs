using BasecodeLibrary.Utilities;
using LightBuzz.SMTP;
using Models.MotionDetector;
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

        private async void RunSMTPTestExecuted()
        {
            IsNotRunningTest = false;
            using (SmtpClient client = new SmtpClient(ConfigurationSettings.SmtpSettings.SmtpServer,
                                                      ConfigurationSettings.SmtpSettings.SmtpPort,
                                                      ConfigurationSettings.SmtpSettings.UseSSL,
                                                      ConfigurationSettings.SmtpSettings.SmtpUserName,
                                                      ConfigurationSettings.SmtpSettings.SmtpPassword))
            {
                EmailMessage emailMessage = new EmailMessage();
                emailMessage.Subject = "TEST | ALERT | MOTION DETECTED";
                emailMessage.Importance = EmailImportance.High;
                emailMessage.Sender.Address = "IoTAlertApp@donotreply.com";
                emailMessage.Sender.Name = "IoT App";
                emailMessage.To.Add(new EmailRecipient(ConfigurationSettings.SmtpSettings.Recipient));
                emailMessage.Subject = "ALERT | MOTION DETECTED";

 
                SmtpResult result = await client.SendMailAsync(emailMessage);
                IsNotRunningTest = true;
            }
        }

        private async void Setup()
        {
            RunTestsCommand = new CommandHandler(RunSMTPTestExecuted);
            ConfigurationSettings = await SaveManager.GetJsonFile<ConfigModel>("config.json");
            OnPropertyChanged("ConfigurationSettings");
            this.ConfigurationSettings.AppConfig.PropertyChanged += SettingsViewModel_PropertyChanged;
            this.ConfigurationSettings.SmtpSettings.PropertyChanged += SettingsViewModel_PropertyChanged;
        }

        private void SettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateSettingsExecuted();
                       
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

        private async void UpdateSettingsExecuted()
        {
            await SaveManager.SaveJsonFile("config.json", ConfigurationSettings);
        }
    }
}
