using BaseCodeLibrary.Utilities;

namespace MotionDetector.Models
{
    public class SoundConfigModel : ModelBase
    {
        private bool _playSounds;
        private string _soundName;
        private bool _playContinuous;
        private int _continuousSecondDelay;

        public bool PlaySounds
        {
            get { return _playSounds; }
            set { _playSounds = value; OnPropertyChanged(nameof(PlaySounds)); }
        }

        public bool PlayContinuous
        {
            get { return _playContinuous; }
            set { _playContinuous = value; OnPropertyChanged(nameof(PlayContinuous)); }
        }

        public int ContinuousSecondDelay
        {
            get { return _continuousSecondDelay; }
            set { _continuousSecondDelay = value; OnPropertyChanged(nameof(ContinuousSecondDelay)); }
        }

        public string SoundName
        {
            get { return _soundName; }
            set { _soundName = value; OnPropertyChanged(nameof(SoundName)); }
        }
    }
}
