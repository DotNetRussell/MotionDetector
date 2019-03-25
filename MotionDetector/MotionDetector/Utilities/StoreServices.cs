using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Services.Store;
using Windows.Storage;
using Windows.System;

namespace MotionDetector.Utilities
{
    public static class StoreServices
    {
#if DEBUG

        public static LicenseInformation LicenseInformation = CurrentAppSimulator.LicenseInformation;
#else
        public static LicenseInformation LicenseInformation = CurrentApp.LicenseInformation;
#endif
        public static bool RemoveAds { get; set; }
        public static bool IsPremium { get; set; }

        static StoreServices()
        {
            RemoveAds = false;
            IsPremium = false;
        }

        public static async Task<bool> SetupDemoStore()
        {
            StorageFolder proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile proxyFile = await proxyDataFolder.GetFileAsync("test.xml");
            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);

            LicenseInformation = CurrentAppSimulator.LicenseInformation;

            return true;
        }

        public static void CheckFreemiumStatus()
        {
            if (LicenseInformation.ProductLicenses["RemoveAds"].IsActive)
            {
                RemoveAds = true;
            }
        }

        public static void CheckForPremiumStatus()
        {
            if (LicenseInformation.ProductLicenses["PremiumStatus"].IsActive)
            {
                IsPremium = RemoveAds = true;
            }
        }

        public static async Task<bool> OpenStorePurchasePremium()
        {
            if (!LicenseInformation.ProductLicenses["PremiumStatus"].IsActive)
            {
                try
                {
#if DEBUG
                    PurchaseResults results = await CurrentAppSimulator.RequestProductPurchaseAsync("PremiumStatus");
#else
                    PurchaseResults results = await CurrentApp.RequestProductPurchaseAsync("PremiumStatus");
#endif

                    if (results.Status == ProductPurchaseStatus.Succeeded || results.Status == ProductPurchaseStatus.AlreadyPurchased)
                    {
                        IsPremium = RemoveAds = true;                         
                    }
                }
                catch (Exception)
                {
                    // The in-app purchase was not completed because
                    // an error occurred.
                }
            }
            else
            {
                IsPremium = RemoveAds = true;
            }

            return IsPremium;
        }

        public static async Task<bool> OpenStoreRemoveAds()
        {
            if (!LicenseInformation.ProductLicenses["RemoveAds"].IsActive)
            {
                try
                {
#if DEBUG
                    PurchaseResults results = await CurrentAppSimulator.RequestProductPurchaseAsync("RemoveAds");
#else
                    PurchaseResults results = await CurrentApp.RequestProductPurchaseAsync("RemoveAds");
#endif

                    if (results.Status == ProductPurchaseStatus.Succeeded || results.Status == ProductPurchaseStatus.AlreadyPurchased)
                    {
                        RemoveAds = true;
                    }
                }
                catch (Exception)
                {
                    // The in-app purchase was not completed because
                    // an error occurred.
                }
            }
            else
            {
                RemoveAds = true;
            }

            return RemoveAds;
        }
    }
}
