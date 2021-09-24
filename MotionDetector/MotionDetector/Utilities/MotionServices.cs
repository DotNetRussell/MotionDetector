using Models.MotionDetector;
using System;
using System.Collections.Generic;

namespace MotionDetector.Utilities
{
    public static class MotionServices
    {
        /// <summary>
        /// Determins if the image being examined is a dark image. If it is, then true will be returned
        /// </summary>
        /// <param name="testImage">The image being examined</param>
        /// <returns>Checks if image is dark</returns>
        private static bool CheckIfImageIsDarkShifted(ConfigModel ConfigurationSettings, byte[] testImage)
        {
            double total = 0;
            foreach (byte image in testImage)
            {
                total += Convert.ToDouble(image);
            }
            double average = total / testImage.Length;

            return average < ConfigurationSettings.AppConfig.DarkShiftThreshold;
        }

        /// <summary>
        /// Uses the parameters set in the config file to test the image passed in against all baseline images. If any of the baseline
        /// images match, then false is returned. Otherwise true is returned.
        /// </summary>
        /// <param name="testImage">The image you wish to test against all baseline images.</param>
        /// <returns>Returns true if none of the baseline images match the image passed in.</returns>
        public static bool CheckForMotion(ConfigModel ConfigurationSettings, byte[] testImage, IEnumerable<byte[]> baselineImages)
        {
            double configPixelDelta = ConfigurationSettings.AppConfig.PixelDelta;
            List<bool> baseLineChecks = new List<bool>();

            foreach (byte[] baseline in baselineImages)
            {
                int changedPixels = 0;

                for (int x = 0; x < baseline.Length; x++)
                {
                    double pixelDelta = Convert.ToDouble(baseline[x]) / Convert.ToDouble(testImage[x]);
                    if (pixelDelta > (1.0 + configPixelDelta)
                        || pixelDelta < (1.0 - configPixelDelta))
                    {
                        changedPixels++;
                    }
                }

                baseLineChecks.Add(Convert.ToDouble(changedPixels) / Convert.ToDouble(baseline.Length)
                    > (Convert.ToDouble(ConfigurationSettings.AppConfig.ImageDelta) / 100d));
            }

            if (!baseLineChecks.Contains(false)
                    && !CheckIfImageIsDarkShifted(ConfigurationSettings, testImage))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
