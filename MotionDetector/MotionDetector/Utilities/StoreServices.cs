using System;
using System.Threading.Tasks;
using Windows.Services.Store;
using System.ServiceModel;
#if DEBUG
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Storage;
#endif

namespace MotionDetector.Utilities
{
    public static class StoreServices
    {
#if DEBUG
        public static LicenseInformation LicenseInformation { get; private set; }
#else
        static StoreAppLicense _appLicense = null;
#endif
        public static bool RemoveAds { get; set; }
        public static bool IsPremium { get; set; }

        static StoreServices()
        {
            RemoveAds = false;
            IsPremium = false;
        }

        public static async Task<bool> SetupStoreServices()
        {
            try
            {
#if DEBUG
                StorageFolder proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
                StorageFile proxyFile = await proxyDataFolder.GetFileAsync("test.xml");
                await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);

                LicenseInformation = CurrentAppSimulator.LicenseInformation;
#else  
                var context = StoreContext.GetDefault();
                _appLicense = await context.GetAppLicenseAsync();
#endif
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void CheckFreemiumStatus()
        {
#if DEBUG
            if (LicenseInformation.ProductLicenses["RemoveAds"].IsActive)
            {
                RemoveAds = true;
            }
#else
            var subscriptionStoreId = "9NV9QN9LX9FG";

            foreach (var addOnLicense in _appLicense.AddOnLicenses)
            {
                StoreLicense license = addOnLicense.Value;
                if (license.SkuStoreId.StartsWith(subscriptionStoreId))
                {
                    if (license.IsActive)
                    {
                        RemoveAds = true;
                        return;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            RemoveAds = false;
#endif
        }

        public static void CheckForPremiumStatus()
        {
#if DEBUG
            if (LicenseInformation.ProductLicenses["PremiumStatus"].IsActive)
            {
                IsPremium = RemoveAds = true;
            }
#else
            var subscriptionStoreId = "9PMT47KC5W6C";

            foreach (var addOnLicense in _appLicense.AddOnLicenses)
            {
                StoreLicense license = addOnLicense.Value;
                if (license.SkuStoreId.StartsWith(subscriptionStoreId))
                {
                    if (license.IsActive)
                    {
                        IsPremium = RemoveAds = true;
                        return;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            IsPremium = RemoveAds = false;
#endif
        }

        public static async Task<bool> OpenStorePurchasePremium()
        {
#if DEBUG
            if (!LicenseInformation.ProductLicenses["PremiumStatus"].IsActive)
            {
                try
                {

                    PurchaseResults results = await CurrentAppSimulator.RequestProductPurchaseAsync("PremiumStatus");

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
#else
            var context = StoreContext.GetDefault();
            StoreProduct subscriptionStoreProduct = null; 
            StoreProductQueryResult result = await context.GetAssociatedStoreProductsAsync(new string[] { "Durable" });

            var subscriptionStoreId = "9PMT47KC5W6C";
            foreach (var item in result.Products)
            {
                StoreProduct product = item.Value;
                if (product.StoreId == subscriptionStoreId)
                {
                    subscriptionStoreProduct = product;
                }
            }

            // Load the sellable add-ons for this app and check if the trial is still 
            // available for this customer. If they previously acquired a trial they won't 
            // be able to get a trial again, and the StoreProduct.Skus property will 
            // only contain one SKU.
            StorePurchaseResult purchaseResult = await subscriptionStoreProduct?.RequestPurchaseAsync();
            switch (purchaseResult.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                case StorePurchaseStatus.Succeeded:
                    IsPremium = true;
                    break;

                case StorePurchaseStatus.NotPurchased:
                case StorePurchaseStatus.ServerError:
                case StorePurchaseStatus.NetworkError:
                    IsPremium = false;
                    break;
                default:
                    IsPremium = false;
                    break;
            }
#endif
            return IsPremium;
        }

        public static async Task<bool> OpenStoreRemoveAds()
        {
#if DEBUG
            if (!LicenseInformation.ProductLicenses["RemoveAds"].IsActive)
            {
                try
                {

                    PurchaseResults results = await CurrentAppSimulator.RequestProductPurchaseAsync("RemoveAds");

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
#else
            var context = StoreContext.GetDefault();
            StoreProduct subscriptionStoreProduct = null; 
            StoreProductQueryResult result = await context.GetAssociatedStoreProductsAsync(new string[] { "Durable" });

            var subscriptionStoreId = "9NV9QN9LX9FG";
            foreach (var item in result.Products)
            {
                StoreProduct product = item.Value;
                if (product.StoreId == subscriptionStoreId)
                {
                    subscriptionStoreProduct = product;
                }
            }

            // Load the sellable add-ons for this app and check if the trial is still 
            // available for this customer. If they previously acquired a trial they won't 
            // be able to get a trial again, and the StoreProduct.Skus property will 
            // only contain one SKU.
            StorePurchaseResult purchaseResult = await subscriptionStoreProduct?.RequestPurchaseAsync();
            switch (purchaseResult.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                case StorePurchaseStatus.Succeeded:
                    RemoveAds = true;
                    break;

                case StorePurchaseStatus.NotPurchased:
                case StorePurchaseStatus.ServerError:
                case StorePurchaseStatus.NetworkError:
                    RemoveAds = false;
                    break;
                default:
                    RemoveAds = false;
                    break;
            }
#endif
            return RemoveAds;
        }
    }
}
