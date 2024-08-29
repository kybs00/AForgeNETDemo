using System.Drawing;
using AForge.Video.DirectShow;
using AForge.Video;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Interop;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;

namespace CameraShowDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            // 获取所有视频输入设备
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                // 选择第一个视频输入设备
                var videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                // 注册NewFrame事件处理程序
                videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);
                // 开始摄像头视频源
                videoSource.Start();
            }
        }

        private async void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // 获取当前的视频帧，显示
            var image = ToBitmapImage(eventArgs.Frame);
            await Dispatcher.InvokeAsync(() => { CaptureImage.Source = image; });
        }

        /// <summary>
        /// Bitmap转换为BitmapSource
        /// </summary>
        /// <param name="bitmap">System.Drawing.<see cref="Bitmap"/>位图</param>
        /// <returns></returns>
        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private void StartCaptureButton_OnClick(object sender, RoutedEventArgs e)
        {
            // 获取所有视频输入设备
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                // 选择第一个视频输入设备
                var videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                // 注册NewFrame事件处理程序
                videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame1);
                // 开始摄像头视频源
                videoSource.Start();
            }
        }
        private async void videoSource_NewFrame1(object sender, NewFrameEventArgs eventArgs)
        {
            // 获取当前的视频帧
            // 将Bitmap转换为byte[]，用于流媒体传输
            byte[] byteArray = BitmapToByteArray(eventArgs.Frame, out int stride);
            // 将byte[]转换为BitmapImage
            BitmapImage image = ByteArrayToBitmapImage(byteArray, eventArgs.Frame.Width, eventArgs.Frame.Height, stride, eventArgs.Frame.PixelFormat);

            await Dispatcher.InvokeAsync(() => { CaptureImage.Source = image; });
        }
    // 将Bitmap转换为byte[]
    public byte[] BitmapToByteArray(Bitmap bitmap, out int stride)
    {
        Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
        //stride是分辨率水平值，如3840
        stride = bitmapData.Stride;
        int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
        byte[] rgbValues = new byte[bytes];

        // 复制位图数据到字节数组
        Marshal.Copy(bitmapData.Scan0, rgbValues, 0, bytes);

        bitmap.UnlockBits(bitmapData);
        return rgbValues;
    }

    // 将byte[]转换为BitmapImage
    public BitmapImage ByteArrayToBitmapImage(byte[] byteArray, int width, int height, int stride, PixelFormat pixelFormat)
    {
        var bitmapImage = new BitmapImage();
        using (var memoryStream = new MemoryStream())
        {
            var bmp = new Bitmap(width, height, stride, pixelFormat, Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0));
            // 保存到MemoryStream中
            bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            memoryStream.Seek(0, SeekOrigin.Begin);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
        }
        return bitmapImage;
    }
    }
}