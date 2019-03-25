using BasecodeLibrary.Utilities;
using Models.MotionDetector;
using MotionDetector.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MotionDetector.ViewModels
{
    public class SoundSetupViewModel : ViewModelBase
    {
        private ConfigModel _ConfigurationSettings;

        public ConfigModel ConfigurationSettings
        {
            get { return _ConfigurationSettings; }
            set { _ConfigurationSettings = value; OnPropertyChanged("ConfigurationSettings"); }
        }

        public ObservableCollection<string> AvailableSounds { get; set; }
        private string _selectedSound;

        public string SelectedSound
        {
            get { return _selectedSound; }
            set { _selectedSound = value; ConfigurationSettings.SoundConfig.SoundName = value; OnPropertyChanged("SelectedSound"); }
        }

        public ICommand PlaySoundCommand { get; set; }
        public ICommand UpdateSettingsCommand { get; set; }
        public ICommand LoadCustomSoundCommand { get; set; }

        public SoundSetupViewModel()
        {
            AvailableSounds = new ObservableCollection<string>();
            PlaySoundCommand = new CommandHandler(PlaySoundExecuted);
            UpdateSettingsCommand = new CommandHandler(UpdateSettingsExecuted);
            LoadCustomSoundCommand = new CommandHandler(LoadCustomSoundExecuted);

            Setup();
        }
        
        private async void Setup()
        {
            ConfigurationSettings = await ConfigurationServices.GetConfig();

            foreach (string sound in await SoundsServices.GetAvailableSounds())
            {
                AvailableSounds.Add(sound);
            }

            SelectedSound = ConfigurationSettings.SoundConfig.SoundName;
        }
        private async void LoadCustomSoundExecuted()
        {
            await SoundsServices.ImportCustomSound();

            foreach (string sound in await SoundsServices.GetAvailableSounds())
            {
                if(!AvailableSounds.Contains(sound))
                    AvailableSounds.Add(sound);
            }
        }

        private void UpdateSettingsExecuted()
        {
            ConfigurationServices.SaveConfig(ConfigurationSettings);
        }

        private void PlaySoundExecuted()
        {
            if (SelectedSound != null)
            {
                SoundsServices.PlayAlertSound(SelectedSound);
            }
        }

    }
}
