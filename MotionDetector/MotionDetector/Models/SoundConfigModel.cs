using BaseCodeLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionDetector.Models
{
    public class SoundConfigModel : ModelBase
    {
        private bool _playSounds;

        public bool PlaySounds
        {
            get { return _playSounds; }
            set { _playSounds = value; OnPropertyChanged("PlaySounds"); }
        }

        private bool _playContinuous;   

        public bool PlayContinuous
        {
            get { return _playContinuous; }
            set { _playContinuous = value; OnPropertyChanged("PlayContinuous"); }
        }

        private int _continuousSecondDelay;

        public int ContinuousSecondDelay
        {
            get { return _continuousSecondDelay; }
            set { _continuousSecondDelay = value; OnPropertyChanged("ContinuousSecondDelay"); }
        }

        private string _soundName;

        public string SoundName
        {
            get { return _soundName; }
            set { _soundName = value; OnPropertyChanged("SoundName"); }
        }
    }
}
