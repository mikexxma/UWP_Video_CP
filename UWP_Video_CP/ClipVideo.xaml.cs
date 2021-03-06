﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Effects;
using Windows.Media.Transcoding;
using Windows.Storage;
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
    public sealed partial class ClipVideo : Page
    {
        private StorageFile pickedFile;
        private MediaComposition composition;
        private MediaStreamSource mediaStreamSource;
        public ClipVideo()
        {
            this.InitializeComponent();
        }

        private async void ChooseFile_Click(object sender, RoutedEventArgs e)
        {           
            // Get file
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
           
            picker.FileTypeFilter.Add(".mov");
            picker.FileTypeFilter.Add(".mp4");
            pickedFile = await picker.PickSingleFileAsync();
            if (pickedFile == null)
            {                
                return;
            }

            // These files could be picked from a location that we won't have access to later
            // (especially if persisting the MediaComposition to disk and loading it later). 
            // Use the StorageItemAccessList in order to keep access permissions to that
            // file for later use. Be aware that this access list needs to be cleared
            // periodically or the app will run out of entries.
            //storageItemAccessList.Add(pickedFile);
            mediaElement.SetSource(await pickedFile.OpenReadAsync(), pickedFile.ContentType);
            trimClip.IsEnabled = true;
        }

        private async void TrimClip_Click(object sender, RoutedEventArgs e)
        {
            // Trim the front and back 25% from the clip
            var clip = await MediaClip.CreateFromFileAsync(pickedFile);
            clip.TrimTimeFromStart = new TimeSpan((long)(clip.OriginalDuration.Ticks * 0.25));
            clip.TrimTimeFromEnd = new TimeSpan((long)(clip.OriginalDuration.Ticks * 0.25));

            // Create a MediaComposition containing the clip and set it on the MediaElement.
            composition = new MediaComposition();
            composition.Clips.Add(clip);
            mediaElement.Position = TimeSpan.Zero;
            mediaStreamSource = composition.GenerateMediaStreamSource();
            mediaElement.SetMediaStreamSource(mediaStreamSource);            
            save.IsEnabled = true;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            save.IsEnabled = false;
            chooseFile.IsEnabled = false;
            trimClip.IsEnabled = false;
               var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeChoices.Add("MOV files", new List<string>() { ".mov" });
            picker.SuggestedFileName = "TrimmedClip.mov";

            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                var saveOperation = composition.RenderToFileAsync(file, MediaTrimmingPreference.Precise);
                saveOperation.Progress = new AsyncOperationProgressHandler<TranscodeFailureReason, double>(async (info, progress) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                    {
                        ResultMessage.Text = string.Format("Saving file... Progress: {0:F0}%", progress);
                    }));
                });
                saveOperation.Completed = new AsyncOperationWithProgressCompletedHandler<TranscodeFailureReason, double>(async (info, status) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                    {
                        try
                        {
                            var results = info.GetResults();
                            if (results != TranscodeFailureReason.None || status != AsyncStatus.Completed)
                            {
                                ResultMessage.Text = "saving error";
                            }
                            else
                            {
                                ResultMessage.Text = "saving success";
                            }
                        }
                        finally
                        {
                            save.IsEnabled = true;
                            chooseFile.IsEnabled = true;
                            trimClip.IsEnabled = true;

                        }
                    }));
                });
            }
        }

        private async void specialVideo_Click(object sender, RoutedEventArgs e)
        {

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mov");
            picker.FileTypeFilter.Add(".mp4");
            StorageFile pickedFilespecial = await picker.PickSingleFileAsync();
            if (pickedFilespecial == null)
            {
                return;
            }
            var clip = await MediaClip.CreateFromFileAsync(pickedFilespecial);
            composition = new MediaComposition();
            composition.Clips.Add(clip);
            var videoEffectDefinition = new VideoEffectDefinition("VideoEffectComponent.ExampleVideoEffect", new PropertySet() { { "FadeValue", .9 } });
            clip.VideoEffectDefinitions.Add(videoEffectDefinition);
            MediaStreamSource mediaStreamSource = composition.GenerateMediaStreamSource();
            mediaElement.SetMediaStreamSource(mediaStreamSource);
        }
    }
}
