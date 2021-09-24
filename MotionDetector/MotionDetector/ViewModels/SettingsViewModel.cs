using BasecodeLibrary.Utilities;
using Models.MotionDetector;
using MotionDetector.Models;
using MotionDetector.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Enumeration;

namespace MotionDetector.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private bool _isNotRunningTest = true;
        private VideoDeviceModel selectedCameraModel;

        public ICommand RunTestsCommand { get; set; }
        public ICommand UpdateSettingsCommand { get; set; }
        public List<string> ServerOptions { get { return new List<string>() { "Gmail", "Yahoo", "Custom" }; } }
        public ObservableCollection<VideoDeviceModel> AvailableCameras { get; set; }
        public ConfigModel ConfigurationSettings { get; set; }

        public VideoDeviceModel SelectedCameraModel
        {
            get => selectedCameraModel;
            set
            {
                selectedCameraModel = value;
                OnPropertyChanged(nameof(SelectedCameraModel));
                ConfigurationSettings.AppConfig.SelectedCameraId = SelectedCameraModel.DeviceId;
            }
        }

        public bool IsNotRunningTest
        {
            get { return _isNotRunningTest; }
            set { _isNotRunningTest = value; OnPropertyChanged(nameof(IsNotRunningTest)); }
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
            AvailableCameras = new ObservableCollection<VideoDeviceModel>();
            RunTestsCommand = new CommandHandler(RunSMTPTestExecuted);
            UpdateSettingsCommand = new CommandHandler(UpdateSettingsExecuted);

            ConfigurationSettings = await ConfigurationServices.GetConfig();
            OnPropertyChanged(nameof(ConfigurationSettings));
            this.ConfigurationSettings.AppConfig.PropertyChanged += SettingsViewModel_PropertyChanged;
            this.ConfigurationSettings.SmtpSettings.PropertyChanged += SettingsViewModel_PropertyChanged;
            await LoadCameras();
        }

        private void RunSMTPTestExecuted()
        {
            IsNotRunningTest = false;
            SMTPServices.RunSMTPTest(ConfigurationSettings, () => { IsNotRunningTest = true; });
        }

        private async Task LoadCameras()
        {
            foreach (var device in await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture))
            {
                AvailableCameras.Add(new VideoDeviceModel() { DeviceId = device.Id, DeviceName = device.Name });
            }

            if (AvailableCameras.Count == 1)
            {
                SelectedCameraModel = AvailableCameras.FirstOrDefault();
            }
        }

        private void SettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PreferredSmtpServer")
            {
                switch (ConfigurationSettings.SmtpSettings.PreferredSmtpServer)
                {
                    case "Gmail":
                        ConfigurationSettings.SmtpSettings.SmtpServer = "smtp.gmail.com";
                        ConfigurationSettings.SmtpSettings.SmtpPort = 465;
                        break;
                    case "Yahoo":
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
