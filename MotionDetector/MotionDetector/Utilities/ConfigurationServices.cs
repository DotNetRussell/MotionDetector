using BasecodeLibrary.Utilities;
using Models.MotionDetector;
using MotionDetector.Models;
using System.Threading.Tasks;

namespace MotionDetector.Utilities
{

    public static class ConfigurationServices
    {
        private static bool IsInitialized = false;
        private static ConfigModel ConfigurationSettings;
        private static int configVersion = 3;
        
        public static async Task<TutorialModel> GetTutorialLinks()
        {
            // muh webserver died and now it's broke :cry-emoji:
            //using (WebClient client = new WebClient())
            //{
            //    string result = await client.DownloadStringTaskAsync(new Uri("https://DotNetRussell.com/umde.json"));
            //    return JsonConvert.DeserializeObject<TutorialModel>(result);
            //}

            return new TutorialModel()
            {
                TutorialLinkOne = "https://www.youtube.com/watch?v=-xRucA-vsbA",
                TutorialLinkTwo = "https://www.youtube.com/watch?v=E1hWwEhwlCY"
            };
        }

        public static async Task<ConfigModel> GetConfig()
        {
            if(IsInitialized)
                return ConfigurationSettings = await SaveManager.GetJsonFile<ConfigModel>("config.json");
            else
            {
                return  await InitializeConfig();
            }
        }

        public static async void SaveConfig(ConfigModel config)
        {
            await SaveManager.SaveJsonFile("config.json", config);
        }

        private static async Task<ConfigModel> InitializeConfig()
        {
            ConfigModel config = new ConfigModel()
            {
                SmtpSettings = new SmtpSettingsModel()
                {
                    SmtpPort = 465,
                    UseSSL = true,
                },
                AppConfig = new AppConfigModel()
                {
                    SendEmails = false,
                    ImageDelta = 7,
                    PixelDelta = .3,
                    AlertThreshold = 2,
                    AlertDelay = 2,
                    CaptureDelay = 500,
                    DarkShiftThreshold = 70,
                    ConfigVersion = configVersion
                },
                SoundConfig = new SoundConfigModel()
                {
                    PlaySounds = false,
                    PlayContinuous = false,
                    ContinuousSecondDelay = 1,
                    SoundName = ""
                }
            };

            if (!await SaveManager.FileExists("config.json"))
            {
                await SaveManager.SaveJsonFile("config.json", config);
            }

            // Apparently there's an edge case where if you create a config file and then attempt to read it right away,
            // it won't exist yet. Doing this little wait and see check fixes it. This also only happens on first run.
            while (!await SaveManager.FileExists("config.json")) { continue; }

            ConfigurationSettings = await SaveManager.GetJsonFile<ConfigModel>("config.json");

            if (ConfigurationSettings.AppConfig.ConfigVersion < configVersion)
            {
                await SaveManager.SaveJsonFile("config.json", config);
            }

            IsInitialized = true;
            return ConfigurationSettings;
        }
    }
}
