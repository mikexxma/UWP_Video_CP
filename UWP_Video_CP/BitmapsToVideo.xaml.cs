using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static UWP_Video_CP.GetSingleFrame;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Video_CP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BitmapsToVideo : Page
    {

        private List<StorageFile> m_files ;
        MediaComposition composition;
        public BitmapsToVideo()
        {
            m_files = new List<StorageFile>();
            this.InitializeComponent();
        }

        private async void encodeImages_Click(object sender, RoutedEventArgs e)
        {

            FileOpenPicker openPicker = new FileOpenPicker();
            foreach (string extension in FileExtensions.Image)
            {
                openPicker.FileTypeFilter.Add(extension);
            }
            IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
            composition = new MediaComposition();
            foreach (StorageFile file in files)
            {
                m_files.Add(file);
                var clip =await MediaClip.CreateFromImageFileAsync(file, TimeSpan.FromMilliseconds(1000));
                composition.Clips.Add(clip);
            }

            Debug.WriteLine(composition.Clips.Count);
            //StorageFile fileSave = await openPicker.PickSaveFileAsync();
            //if (file != null)
            //{
            //    var saveOperation = composition.RenderToFileAsync(file, MediaTrimmingPreference.Precise);
            //    await composition.RenderToFileAsync("C://Mike");
            //}
        }

        private  void showVideo_Click(object sender, RoutedEventArgs e)

        {
            GC.Collect();
            //FileSavePicker savePicker = new FileSavePicker();
            //savePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            //savePicker.FileTypeChoices.Add("MP4 File", new List<string>() { ".mp4" });
            //savePicker.SuggestedFileName = "output";
            //StorageFile videoFile = await savePicker.PickSaveFileAsync();
            //await composition.RenderToFileAsync(videoFile);
            encodeImagesResult.Position = TimeSpan.Zero;
            IMediaSource mediaStreamSource = composition.GeneratePreviewMediaStreamSource(400, 400);
            encodeImagesResult.SetMediaStreamSource(mediaStreamSource);
        }
    }
}
