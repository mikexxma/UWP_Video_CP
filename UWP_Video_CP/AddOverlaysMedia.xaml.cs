using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class AddOverlaysMedia : Page
    {

        private MediaComposition composition;
        private StorageFile baseVideoFile;
        private StorageFile overlayVideoFile;
        private MediaStreamSource mediaStreamSource;
        public AddOverlaysMedia()
        {
            this.InitializeComponent();
        }

        private async void BaseVideobt_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".mov");
            baseVideoFile = await picker.PickSingleFileAsync();
            if (baseVideoFile == null)
            {
                return;
            }
            BaseVideo.Text = baseVideoFile.Name;
            mediaElement.SetSource(await baseVideoFile.OpenReadAsync(), baseVideoFile.ContentType);
        }

        private async void OverlayVideoBt_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".mov");
            overlayVideoFile = await picker.PickSingleFileAsync();
            if (overlayVideoFile == null)
            {
                return;
            }
            OverlayVideo.Text = overlayVideoFile.Name;
            CreateOverlays();
        }

        private async void CreateOverlays()
        {
            var baseVideoClip = await MediaClip.CreateFromFileAsync(baseVideoFile);
            composition = new MediaComposition();
            composition.Clips.Add(baseVideoClip);

            var overlayVideoClip = await MediaClip.CreateFromFileAsync(overlayVideoFile);

            // Overlay video in upper left corner, retain its native aspect ratio
            Rect videoOverlayPosition;
            var encodingProperties = overlayVideoClip.GetVideoEncodingProperties();
            videoOverlayPosition.Height = mediaElement.ActualHeight / 3;
            videoOverlayPosition.Width = (double)encodingProperties.Width / (double)encodingProperties.Height * videoOverlayPosition.Height;
            videoOverlayPosition.X = 0;
            videoOverlayPosition.Y = 0;

            var videoOverlay = new MediaOverlay(overlayVideoClip);
            videoOverlay.Position = videoOverlayPosition;
            videoOverlay.Opacity = 0.75;

            var overlayLayer = new MediaOverlayLayer();
            overlayLayer.Overlays.Add(videoOverlay);
            composition.OverlayLayers.Add(overlayLayer);

            // Render to MediaElement
            mediaElement.Position = TimeSpan.Zero;
            mediaStreamSource = composition.GeneratePreviewMediaStreamSource((int)mediaElement.ActualWidth, (int)mediaElement.ActualHeight);
            mediaElement.SetMediaStreamSource(mediaStreamSource);
        }
    }
}
