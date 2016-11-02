using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
   
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FaceDetect : Page
    {
        StorageFile photoFile;
        List<WriteableBitmap> facelist;
        /// <summary>
        public FaceDetect()
        {
            facelist = new List<WriteableBitmap>();
            this.InitializeComponent();
        }

       

        private async void SetupVisualization(WriteableBitmap displaySource, IList<DetectedFace> foundFaces)
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
                    Rectangle box = new Rectangle();
                    box.Tag = face.FaceBox;
                    box.Width = (uint)(face.FaceBox.Width / widthScale);
                    box.Height = (uint)(face.FaceBox.Height / heightScale);
                    box.Fill = new SolidColorBrush(Windows.UI.Colors.Transparent);
                    box.Stroke = new SolidColorBrush(Windows.UI.Colors.Yellow); ;
                    box.StrokeThickness = 2.0;
                    box.Margin = new Thickness((uint)(face.FaceBox.X / widthScale), (uint)(face.FaceBox.Y / heightScale), 0, 0);

                    Point point = new Point();
                    point.X = face.FaceBox.X;
                    point.Y = face.FaceBox.Y;
                    Size size = new Size();
                    size.Height = face.FaceBox.Height;
                    size.Width = face.FaceBox.Width;


                    facelist.Add(await GetCroppedBitmapAsync(photoFile, point, size, 1));
                    this.detectedResult.Children.Add(box);
                   
                }

                for (int i = 0; i < facelist.Count; i++)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                    Stream pixelStream = facelist[i].PixelBuffer.AsStream();
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, randomAccessStream);

                    byte[] pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                    // Save the image file with jpg extension 
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)facelist[i].PixelWidth, (uint)facelist[i].PixelHeight, 96.0, 96.0, pixels);
                    await encoder.FlushAsync();

                    //randomAccessStream.Seek(0);

                    bitmapImage.SetSource(randomAccessStream);
                    Image img = (Image)this.FindName("face" + i);
                    img.Source = bitmapImage;
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
                photoFile = await photoPicker.PickSingleFileAsync();
                if (photoFile == null)
                {
                    return;
                }

                using (IRandomAccessStream fileStream = await photoFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileStream);
                    sourceImg.Source = bitmapImage;
         
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                    BitmapTransform transform = this.ComputeScalingTransformForSourceImage(decoder);

                    using (SoftwareBitmap originalBitmap = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, BitmapAlphaMode.Ignore, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage))
                    {
                    
                        // face can detect Gray8 file 
                        const BitmapPixelFormat InputPixelFormat = BitmapPixelFormat.Gray8;
                        if (FaceDetector.IsBitmapPixelFormatSupported(InputPixelFormat))
                        {
                            using (detectorInput = SoftwareBitmap.Convert(originalBitmap, InputPixelFormat))
                            {
                                // Create a WritableBitmap for our visualization display; copy the original bitmap pixels to wb's buffer.
                                displaySource = new WriteableBitmap(originalBitmap.PixelWidth, originalBitmap.PixelHeight);
                                originalBitmap.CopyToBuffer(displaySource.PixelBuffer);
                                FaceDetector detector = await FaceDetector.CreateAsync();  // should reuse the detect obj                           
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


       



        async public static Task<WriteableBitmap> GetCroppedBitmapAsync(StorageFile originalImageFile, Point startPoint, Size corpSize, double scale)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale))
            {
                scale = 1;
            }

            // Convert start point and size to integer.
            uint startPointX = (uint)Math.Floor(startPoint.X * scale);
            uint startPointY = (uint)Math.Floor(startPoint.Y * scale);
            uint height = (uint)Math.Floor(corpSize.Height * scale);
            uint width = (uint)Math.Floor(corpSize.Width * scale);
            using (IRandomAccessStream stream = await originalImageFile.OpenReadAsync())
            {
                // Create a decoder from the stream. With the decoder, we can get 
                // the properties of the image.
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                // The scaledSize of original image
                uint scaledWidth = (uint)Math.Floor(decoder.PixelWidth * scale);
                uint scaledHeight = (uint)Math.Floor(decoder.PixelHeight * scale);                        
                // Refine the start point and the size. 
                if (startPointX + width > scaledWidth)
                {
                    startPointX = scaledWidth - width;
                }            
                if (startPointY + height > scaledHeight)
                {
                    startPointY = scaledHeight - height;
                }
                // Get the cropped pixels.
                byte[] pixels = await GetPixelData(decoder, startPointX, startPointY, width, height,scaledWidth, scaledHeight);
                // Stream the bytes into a WriteableBitmap
                WriteableBitmap cropBmp = new WriteableBitmap((int)width, (int)height);
                Stream pixStream = cropBmp.PixelBuffer.AsStream();
                pixStream.Write(pixels, 0, (int)(width * height * 4));
                return cropBmp;
            }
        }

        async static private Task<byte[]> GetPixelData(BitmapDecoder decoder, uint startPointX, uint startPointY,
            uint width, uint height, uint scaledWidth, uint scaledHeight)
        {
            BitmapTransform transform = new BitmapTransform();
            BitmapBounds bounds = new BitmapBounds();
            bounds.X = startPointX;
            bounds.Y = startPointY;
            bounds.Height = height;
            bounds.Width = width;
            transform.Bounds = bounds;
            transform.ScaledWidth = scaledWidth;
            transform.ScaledHeight = scaledHeight;
            // Get the cropped pixels within the bounds of transform.
            PixelDataProvider pix = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.ColorManageToSRgb);
            byte[] pixels = pix.DetachPixelData();
            return pixels;
        }
    }
}
