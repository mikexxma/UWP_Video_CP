using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Video_CP
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddAudioTracks : Page
    {
        private MediaComposition composition;
        private StorageFile pickedFile;
        private StorageFile audioFile;
        private MediaStreamSource mediaStreamSource;

        byte[] FileBytes;
        public AddAudioTracks()
        {
            this.InitializeComponent();
        }

        public async void getFileBytes(StorageFile sampleFile)
        {
            
            using (Stream stream = await sampleFile.OpenStreamForReadAsync())
            {
                using (var memoryStream = new MemoryStream())
                {

                    stream.CopyTo(memoryStream);
                    FileBytes = memoryStream.ToArray();
                }
            }
        }

        private async void ChoseVideo_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mov");
            picker.FileTypeFilter.Add(".avi");
            pickedFile = await picker.PickSingleFileAsync();
            if (pickedFile == null)
            {
                return;
            }
            VideoFile.Text = pickedFile.Name;
            mediaElement.SetSource(await pickedFile.OpenReadAsync(), pickedFile.ContentType);
        }

        private async void ChoseAudio_Click(object sender, RoutedEventArgs e)
        {
            var clip = await MediaClip.CreateFromFileAsync(pickedFile);
            composition = new MediaComposition();
            composition.Clips.Add(clip);

            // Add background audio
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".flac");
            audioFile = await picker.PickSingleFileAsync();
            if (audioFile == null)
            {
                return;
            }
            AudioFile.Text = audioFile.Name;
        }

        private async void AddAudioTrack_Click(object sender, RoutedEventArgs e)
        {

            var backgroundTrack = await BackgroundAudioTrack.CreateFromFileAsync(audioFile);
            composition.BackgroundAudioTracks.Add(backgroundTrack);
            // Render to MediaElement
            mediaElement.Position = TimeSpan.Zero;
            mediaStreamSource = composition.GeneratePreviewMediaStreamSource((int)mediaElement.ActualWidth, (int)mediaElement.ActualHeight);
            mediaElement.SetMediaStreamSource(mediaStreamSource);
        }
    }
}
