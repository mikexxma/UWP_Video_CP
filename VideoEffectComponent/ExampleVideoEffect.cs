using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace VideoEffectComponent
{

    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }
    public sealed class ExampleVideoEffect : IBasicVideoEffect
    {
        

        public double FadeValue
        {
            get
            {
                object val;
                if (configuration != null && configuration.TryGetValue("FadeValue", out val))
                {
                    return (double)val;
                }
                return .5;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                var encodingProperties = new VideoEncodingProperties();
                encodingProperties.Subtype = "ARGB32";
                return new List<VideoEncodingProperties>() { encodingProperties };

            }
        }

        public MediaMemoryTypes SupportedMemoryTypes
        {
            get { return MediaMemoryTypes.Cpu; }//cpu 处理softwabitma gpu处理IDirect3DSurface
        }

        public bool TimeIndependent
        {
            get { return true; }
        }

        public void Close(MediaEffectClosedReason reason)
        {
            //特效关闭的时候 call 
            //reason 表示特写花生了什么才关闭的
        }

        private int frameCount;

        public void DiscardQueuedFrames()
        {
            frameCount = 0;
            //特效重置的时候call
        }

        public unsafe void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (BitmapBuffer buffer = context.InputFrame.SoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Read))
            using (BitmapBuffer targetBuffer = context.OutputFrame.SoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            {
                using (var reference = buffer.CreateReference())
                using (var targetReference = targetBuffer.CreateReference())
                {
                    byte* dataInBytes;
                    uint capacity;
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                    byte* targetDataInBytes;
                    uint targetCapacity;
                    ((IMemoryBufferByteAccess)targetReference).GetBuffer(out targetDataInBytes, out targetCapacity);

                    var fadeValue = FadeValue;

                    // Fill-in the BGRA plane
                    BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);
                    for (int i = 0; i < bufferLayout.Height; i++)
                    {
                        for (int j = 0; j < bufferLayout.Width; j++)
                        {

                            byte value = (byte)((float)j / bufferLayout.Width * 255);

                            int bytesPerPixel = 4;
                            if (encodingProperties.Subtype != "ARGB32")
                            {
                                // If you support other encodings, adjust index into the buffer accordingly
                            }


                            int idx = bufferLayout.StartIndex + bufferLayout.Stride * i + bytesPerPixel * j;

                            targetDataInBytes[idx + 0] = (byte)(fadeValue * (float)dataInBytes[idx + 0]);
                            //targetDataInBytes[idx + 1] = (byte)(fadeValue * (float)dataInBytes[idx + 1]);
                            //targetDataInBytes[idx + 2] = (byte)(fadeValue * (float)dataInBytes[idx + 2]);
                            //targetDataInBytes[idx + 3] = dataInBytes[idx + 3];
                        }
                    }
                }
            }

        }


        private VideoEncodingProperties encodingProperties;

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            //拿到需要增加特效视频的  encodingProperties 
            this.encodingProperties = encodingProperties;
        }


        
        private IPropertySet configuration;

        public void SetProperties(IPropertySet configuration)
        {
            //允许用户更改IPropertySet
            this.configuration = configuration;
        }


    }

}
