using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
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
    public sealed partial class Transcoding_Media : Page
    {

        CancellationTokenSource _cts;
        private StorageFile pickedFile;
        private StorageFile outputFile;
        CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        MediaEncodingProfile _Profile;
        string outputFile_name = "TranscodeSampleOutput.mp4";
        string outputType;
        public Transcoding_Media()
        {
            _cts = new CancellationTokenSource();
            this.InitializeComponent();
        }

        private async void  PickVideo_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mov");
            pickedFile = await picker.PickSingleFileAsync();
            //if (pickedFile == null)
            //{
            //    rootPage.NotifyUser("File picking cancelled", NotifyType.ErrorMessage);
            //    return;
            //}

            // These files could be picked from a location that we won't have access to later
            // (especially if persisting the MediaComposition to disk and loading it later). 
            // Use the StorageItemAccessList in order to keep access permissions to that
            // file for later use. Be aware that this access list needs to be cleared
            // periodically or the app will run out of entries.
            //storageItemAccessList.Add(pickedFile);
            mediaElement.SetSource(await pickedFile.OpenReadAsync(), pickedFile.ContentType);
        }

        private void TransCode_Click(object sender, RoutedEventArgs e)
        {
            TranscodePreset(sender,e);
        }
        void TranscodeProgress(double percent)
        {
            OutputText("Progress:  " + percent.ToString().Split('.')[0] + "%");
        }
        async void OutputText(string text)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OutputMsg.Foreground = new SolidColorBrush(Windows.UI.Colors.Green);
                OutputMsg.Text = text;
            });
        }
        async void TranscodePreset(Object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            //PickVideo.IsEnabled = false;
            //TransCode.IsEnabled = false;
            MediaTranscoder _Transcoder = new MediaTranscoder();
            GetPresetProfile(ProfileSelect);

            // Clear messages
            StatusMessage.Text = "";

            try
            {
                if (pickedFile != null)
                {
                    //var picker = new Windows.Storage.Pickers.FileSavePicker();
                    //picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
                    //picker.FileTypeChoices.Add("MOV files", new List<string>() { ".mov" });
                    //picker.SuggestedFileName = "TrimmedClip.mov";

                    //StorageFile file = await picker.PickSaveFileAsync();

                    outputFile = await KnownFolders.VideosLibrary.CreateFileAsync(outputFile_name, CreationCollisionOption.GenerateUniqueName);
                    var preparedTranscodeResult = await _Transcoder.PrepareFileTranscodeAsync(pickedFile, outputFile, _Profile);
                     _Transcoder.VideoProcessingAlgorithm = MediaVideoProcessingAlgorithm.Default;
                  
                    if (preparedTranscodeResult.CanTranscode)
                    {
                        var progress = new Progress<double>(TranscodeProgress);
                        await preparedTranscodeResult.TranscodeAsync().AsTask(_cts.Token, progress);
                        TranscodeComplete();
                    }
                    else
                    {
                        TranscodeFailure(preparedTranscodeResult.FailureReason);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                OutputText("");
                TranscodeError("Transcode Canceled");
            }
            catch (Exception exception)
            {
                TranscodeError(exception.Message);
            }
        }

        void GetPresetProfile(ComboBox combobox)
        {
            _Profile = null;
            VideoEncodingQuality videoEncodingProfile = VideoEncodingQuality.Wvga;
            switch (combobox.SelectedIndex)
            {
                case 0:
                    videoEncodingProfile = VideoEncodingQuality.HD1080p;
                    break;
                case 1:
                    videoEncodingProfile = VideoEncodingQuality.HD720p;
                    break;
                case 2:
                    videoEncodingProfile = VideoEncodingQuality.Wvga;
                    break;
                case 3:
                    videoEncodingProfile = VideoEncodingQuality.Ntsc;
                    break;
                case 4:
                    videoEncodingProfile = VideoEncodingQuality.Pal;
                    break;
                case 5:
                    videoEncodingProfile = VideoEncodingQuality.Vga;
                    break;
                case 6:
                    videoEncodingProfile = VideoEncodingQuality.Qvga;
                    break;
            }

            switch (outputType)
            {
                case "AVI":
                    _Profile = MediaEncodingProfile.CreateAvi(videoEncodingProfile);
                    break;
                case "WMV":
                    _Profile = MediaEncodingProfile.CreateWmv(videoEncodingProfile);
                    break;
                default:
                    _Profile = MediaEncodingProfile.CreateMp4(videoEncodingProfile);
                    break;
            }

            /*
            For transcoding to audio profiles, create the encoding profile using one of these APIs:
                MediaEncodingProfile.CreateMp3(audioEncodingProfile)
                MediaEncodingProfile.CreateM4a(audioEncodingProfile)
                MediaEncodingProfile.CreateWma(audioEncodingProfile)
                MediaEncodingProfile.CreateWav(audioEncodingProfile)

            where audioEncodingProfile is one of these presets:
                AudioEncodingQuality.High
                AudioEncodingQuality.Medium
                AudioEncodingQuality.Low
            */
        }

        void OnTargetFormatChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (TargetFormat.SelectedIndex)
            {
                case 1:
                    outputFile_name = "TranscodeSampleOutput.wmv";
                    outputType = "WMV";
                    EnableNonSquarePARProfiles();
                    break;
                case 2:
                    outputFile_name = "TranscodeSampleOutput.avi";
                    outputType = "AVI";

                    // Disable NTSC and PAL profiles as non-square pixel aspect ratios are not supported by AVI
                    DisableNonSquarePARProfiles();
                    break;
                default:
                    outputFile_name = "TranscodeSampleOutput.mp4";
                    outputType = "MP4";
                    EnableNonSquarePARProfiles();
                    break;
            }
        }

        void EnableNonSquarePARProfiles()
        {
            ComboBoxItem_NTSC.IsEnabled = true;
            ComboBoxItem_PAL.IsEnabled = true;
        }

        void DisableNonSquarePARProfiles()
        {
            ComboBoxItem_NTSC.IsEnabled = false;
            ComboBoxItem_PAL.IsEnabled = false;

            // Ensure a valid profile is set
            if ((ProfileSelect.SelectedIndex == 3) || (ProfileSelect.SelectedIndex == 4))
            {
                ProfileSelect.SelectedIndex = 2;
            }
        }


        //transcode error handle
        async void TranscodeError(string error)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StatusMessage.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                StatusMessage.Text = error;
            });
        }

        async void TranscodeFailure(TranscodeFailureReason reason)
        {
            try
            {
                if (outputFile != null)
                {
                    await outputFile.DeleteAsync();
                }
            }
            catch (Exception exception)
            {
                TranscodeError(exception.Message);
            }

            switch (reason)
            {
                case TranscodeFailureReason.CodecNotFound:
                    TranscodeError("Codec not found.");
                    break;
                case TranscodeFailureReason.InvalidProfile:
                    TranscodeError("Invalid profile.");
                    break;
                default:
                    TranscodeError("Unknown failure.");
                    break;
            }
        }

        async void TranscodeComplete()
        {
            OutputText("Transcode completed.");
            IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.Read);
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mediaElement.SetSource(stream, outputFile.ContentType);
            });
        }

    }
}

