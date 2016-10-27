using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Video_CP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GetSingleFrame : Page
    {
        
        public GetSingleFrame()
        {
           
            this.InitializeComponent();
        }

        private async void GET_SIMGLE_FRAME_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            foreach (string extension in FileExtensions.Video)
            {
                openPicker.FileTypeFilter.Add(extension);
            }
            StorageFile file = await openPicker.PickSingleFileAsync();

            //get fps
            List<string> encodingPropertiesToRetrieve = new List<string>();
            encodingPropertiesToRetrieve.Add("System.Video.FrameRate");
            IDictionary<string, object> encodingProperties = await file.Properties.RetrievePropertiesAsync(encodingPropertiesToRetrieve);
            uint frameRateX1000 = (uint)encodingProperties["System.Video.FrameRate"];
            FPS.Text ="Frame rate is:"+Convert.ToString(frameRateX1000);

            //var thumbnail1 = await GetThumbnailAsync(file,0);
            //var thumbnail2 = await GetThumbnailAsync(file, 1000);
            //var thumbnail3 = await GetThumbnailAsync(file, 2000);
            for (int i = 0; i < 6; i++)
            {
                var thumbnail = await GetThumbnailAsync(file, i * 1000);
                BitmapImage bitmapImage = new BitmapImage();
                InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                await RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);
                randomAccessStream.Seek(0);
                bitmapImage.SetSource(randomAccessStream);
                Image img = (Image)this.FindName("VideoFrame" + i);
                img.Source = bitmapImage;
            }

            //for (int i = 0; i <60; i++)
            //{
            //    var thumbnail = await GetThumbnailAsync(file, i * 1000);
            //    BitmapImage bitmapImage = new BitmapImage();
            //    InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            //    await RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);
            //    randomAccessStream.Seek(0);


            //    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
            //    WriteableBitmap bmp = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            //    bmp.SetSource(randomAccessStream);


            //    bitmapImage.SetSource(randomAccessStream);
            //    // Image img = (Image)this.FindName("VideoFrame" + i);
            //    // img.Source = bitmapImage;
            //    await SaveBitmapToFileAsync(bmp, "mike" + i);
            //    //App.listImg.Add(bmp);
            //}
            Debug.WriteLine(App.listImg.Count);
            
            //BitmapImage bitmapImage1 = new BitmapImage();
            //InMemoryRandomAccessStream randomAccessStream1 = new InMemoryRandomAccessStream();
            //await RandomAccessStream.CopyAsync(thumbnail1, randomAccessStream1);
            //randomAccessStream1.Seek(0);
            //bitmapImage1.SetSource(randomAccessStream1);
            //VideoFrame1.Source = bitmapImage1;

            //BitmapImage bitmapImage2 = new BitmapImage();
            //InMemoryRandomAccessStream randomAccessStream2 = new InMemoryRandomAccessStream();
            //await RandomAccessStream.CopyAsync(thumbnail2, randomAccessStream2);
            //randomAccessStream2.Seek(0);
            //bitmapImage2.SetSource(randomAccessStream2);
            //VideoFrame2.Source = bitmapImage2;

            //BitmapImage bitmapImage3 = new BitmapImage();
            //InMemoryRandomAccessStream randomAccessStream3 = new InMemoryRandomAccessStream();
            //await RandomAccessStream.CopyAsync(thumbnail3, randomAccessStream3);
            //randomAccessStream3.Seek(0);
            //bitmapImage3.SetSource(randomAccessStream3);
            //VideoFrame3.Source = bitmapImage3;

            
        }
        public async Task<IInputStream> GetThumbnailAsync(StorageFile file,int milliseconds)
        {
            var mediaClip = await MediaClip.CreateFromFileAsync(file);
            var mediaComposition = new MediaComposition();
            mediaComposition.Clips.Add(mediaClip);
            return await mediaComposition.GetThumbnailAsync(TimeSpan.FromMilliseconds(milliseconds), 200, 200, VideoFramePrecision.NearestFrame);
        }

        internal class FileExtensions
        {
            public static readonly string[] Video = new string[] { ".mp4", ".mov" };
            public static readonly string[] Image = new string[] { ".jpg", ".png" };
        }

        public static async Task SaveBitmapToFileAsync(WriteableBitmap image, string userId)
        {
            //C:\Users\mike\AppData\Local\Packages\9ac56cb2-44b1-4055-8c9f-f4d47f946fe7_1pdywbn5ngrp6\LocalState\ProfilePictures
            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePictures", CreationCollisionOption.OpenIfExists);
            var file = await pictureFolder.CreateFileAsync(userId + ".jpg", CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream.AsRandomAccessStream());
                var pixelStream = image.PixelBuffer.AsStream();
                byte[] pixels = new byte[image.PixelBuffer.Length];

                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)image.PixelWidth, (uint)image.PixelHeight, 96, 96, pixels);

                await encoder.FlushAsync();
            }
        }

        public static async Task<WriteableBitmap> GetProfilePictureAsync(string userId)
        {
            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("ProfilePictures");
            StorageFile pictureFile = await pictureFolder.GetFileAsync(userId + ".jpg");

            using (IRandomAccessStream stream = await pictureFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                WriteableBitmap bmp = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

                await bmp.SetSourceAsync(stream);

                return bmp;
            }
        }

    }
}
