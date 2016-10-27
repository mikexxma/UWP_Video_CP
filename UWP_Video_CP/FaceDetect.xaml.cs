using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.FaceAnalysis;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Video_CP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FaceDetect : Page
    {
        public FaceDetect()
        {
            this.InitializeComponent();
        }


        private  void SetupVisualization(WriteableBitmap displaySource, IList<DetectedFace> foundFaces)
        {



            // Set the source of the Image control

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = displaySource;
            brush.Stretch = Stretch.Fill;
            this.detectedResult.Background = brush;

            if (foundFaces != null)
            {
                double widthScale = displaySource.PixelWidth / this.detectedResult.ActualWidth;
                double heightScale = displaySource.PixelHeight / this.detectedResult.ActualHeight;

                foreach (DetectedFace face in foundFaces)
                {
                    // Create a rectangle element for displaying the face box but since we're using a Canvas
                    // we must scale the rectangles according to the image’s actual size.
                    // The original FaceBox values are saved in the Rectangle's Tag field so we can update the
                    // boxes when the Canvas is resized.
                    Rectangle box = new Rectangle();
                    box.Tag = face.FaceBox;
                    box.Width = (uint)(face.FaceBox.Width / widthScale);
                    box.Height = (uint)(face.FaceBox.Height / heightScale);
                    box.Fill = new SolidColorBrush(Windows.UI.Colors.Transparent);
                    box.Stroke = new SolidColorBrush(Windows.UI.Colors.Yellow); ;
                    box.StrokeThickness = 2.0;
                    box.Margin = new Thickness((uint)(face.FaceBox.X / widthScale), (uint)(face.FaceBox.Y / heightScale), 0, 0);

                    this.detectedResult.Children.Add(box);
                }
            }

            string message;
            if (foundFaces == null || foundFaces.Count == 0)
            {
                message = "Didn't find any human faces in the image";
            }
            else if (foundFaces.Count == 1)
            {
                message = "Found a human face in the image";
            }
            else
            {
                message = "Found " + foundFaces.Count + " human faces in the image";
            }
            result.Text = message;
        }


        private async void OpenImg_Click(object sender, RoutedEventArgs e)
        {
            IList<DetectedFace> faces = null;
            SoftwareBitmap detectorInput = null;
            WriteableBitmap displaySource = null;

            try
            {
                FileOpenPicker photoPicker = new FileOpenPicker();
                photoPicker.ViewMode = PickerViewMode.Thumbnail;
                photoPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                photoPicker.FileTypeFilter.Add(".jpg");
                photoPicker.FileTypeFilter.Add(".jpeg");
                photoPicker.FileTypeFilter.Add(".png");
                photoPicker.FileTypeFilter.Add(".bmp");

                StorageFile photoFile = await photoPicker.PickSingleFileAsync();
                if (photoFile == null)
                {
                    return;
                }

                // Open the image file and decode the bitmap into memory.
                // We'll need to make 2 bitmap copies: one for the FaceDetector and another to display.
                using (IRandomAccessStream fileStream = await photoFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileStream);
                    sourceImg.Source = bitmapImage;
         
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                    BitmapTransform transform = this.ComputeScalingTransformForSourceImage(decoder);

                    using (SoftwareBitmap originalBitmap = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, BitmapAlphaMode.Ignore, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage))
                    {
                        // We need to convert the image into a format that's compatible with FaceDetector.
                        // Gray8 should be a good type but verify it against FaceDetector’s supported formats.
                        const BitmapPixelFormat InputPixelFormat = BitmapPixelFormat.Gray8;
                        if (FaceDetector.IsBitmapPixelFormatSupported(InputPixelFormat))
                        {
                            using (detectorInput = SoftwareBitmap.Convert(originalBitmap, InputPixelFormat))
                            {
                                // Create a WritableBitmap for our visualization display; copy the original bitmap pixels to wb's buffer.
                                displaySource = new WriteableBitmap(originalBitmap.PixelWidth, originalBitmap.PixelHeight);
                                originalBitmap.CopyToBuffer(displaySource.PixelBuffer);

                                // Initialize our FaceDetector and execute it against our input image.
                                // NOTE: FaceDetector initialization can take a long time, and in most cases
                                // you should create a member variable and reuse the object.
                                // However, for simplicity in this scenario we instantiate a new instance each time.
                                FaceDetector detector = await FaceDetector.CreateAsync();

                                

                                faces = await detector.DetectFacesAsync(detectorInput);
                                // Create our display using the available image and face results.
                                this.SetupVisualization(displaySource, faces);
                            }
                        }
                        else
                        {
                          
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.ClearVisualization();
               
            }
        }

        private BitmapTransform ComputeScalingTransformForSourceImage(BitmapDecoder sourceDecoder)
        {
            BitmapTransform transform = new BitmapTransform();

            if (sourceDecoder.PixelHeight > 1280)
            {
                float scalingFactor = (float)1280 / (float)sourceDecoder.PixelHeight;

                transform.ScaledWidth = (uint)Math.Floor(sourceDecoder.PixelWidth * scalingFactor);
                transform.ScaledHeight = (uint)Math.Floor(sourceDecoder.PixelHeight * scalingFactor);
            }

            return transform;
        }

        private void ClearVisualization()
        {
            this.detectedResult.Children.Clear();
        }
    }
}
