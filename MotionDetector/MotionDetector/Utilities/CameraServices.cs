using MotionDetector.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace MotionDetector.Utilities
{
    public static class CameraServices
    {
        private static LowLagPhotoCapture lowLagCapture = null;               

        public static async Task<SoftwareBitmap> CaptureImage(MediaCapture MediaCaptureElement)
        {
            if(lowLagCapture == null)
               lowLagCapture = await MediaCaptureElement.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));

            try
            {

                CapturedPhoto capturedPhoto = await lowLagCapture.CaptureAsync();
                SoftwareBitmap softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;

                return softwareBitmap;
            }
            catch
            {
                lowLagCapture = null;
                return await CaptureImage(MediaCaptureElement);
            }
        }


        public static async void SaveImage(AlertDisplayImageModel SelectedAlertImage)
        {
            if (SelectedAlertImage != null)
            {
                FileSavePicker picker = new FileSavePicker();
                picker.FileTypeChoices.Add("PNG File", new List<string> { ".png" });
                StorageFile destFile = await picker.PickSaveFileAsync();

                if (destFile != null)
                {

                    using (IRandomAccessStream stream = await destFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                        Stream pixelStream = SelectedAlertImage.AlertDisplayImage.PixelBuffer.AsStream();
                        byte[] pixels = new byte[pixelStream.Length];
                        await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                    (uint)SelectedAlertImage.AlertDisplayImage.PixelWidth, (uint)SelectedAlertImage.AlertDisplayImage.PixelHeight, 96.0, 96.0, pixels);
                        await encoder.FlushAsync();
                    }
                }
            }
        }
    }
}
