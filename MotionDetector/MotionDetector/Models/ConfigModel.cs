using MotionDetector.Models;

namespace Models.MotionDetector
{
    public class ConfigModel 
    {
        public SmtpSettingsModel SmtpSettings { get; set; }
        public AppConfigModel AppConfig { get; set; }
        public SoundConfigModel SoundConfig { get; set; }
    }
}
