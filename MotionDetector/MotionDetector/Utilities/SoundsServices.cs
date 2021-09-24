using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace MotionDetector.Utilities
{
    public static class SoundsServices
    {
        private static MediaElement _mediaElement = new MediaElement();

        public static async Task<bool> ImportCustomSound()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".wav");

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null && file.IsAvailable)
            {
                StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("Assets\\Sounds");
                await file.CopyAsync(folder);
            }
            return true;
        }

        public static async Task<bool> PlayAlertSound(string sound)
        {
            if (string.IsNullOrEmpty(sound))
            {
                return false;
            }
            else
            {
                StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("Assets\\Sounds");
                StorageFile file = await folder.GetFileAsync(sound);
                IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                _mediaElement.SetSource(stream, "");
                _mediaElement.Play();
            }
            return true;
        }

        public static async Task<IEnumerable<string>> GetAvailableSounds()
        {
            List<string> returnValues = new List<string>();
            try
            {
                StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("Assets\\Sounds");
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();

                foreach (StorageFile file in files)
                {
                    returnValues.Add(file.Name);
                }
            }
            catch
            {
                MessageDialog dialog = new MessageDialog("There was an exception while gathering the included sound files. Please make sure the app has the proper permissions");
                await dialog.ShowAsync();
            }

            return returnValues;
        }
    }
}
