using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net.Sockets;
using System.IO;
using System.Threading;
using ComToPhone;

namespace JBluetoothScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Drawing.Bitmap completePicture;
        public MainWindow()
        {
            InitializeComponent();

            
            DeviceInfo[] devices = BluetoothImageCom.getConnectedDevices();
            for (int i=0; i<devices.Length;i++)
            {
                ComboBoxItem newDevice = new ComboBoxItem();
                newDevice.Content = devices[i].Name;
                newDevice.Tag = devices[i].Address;
                ComboBox_deviceList.Items.Add(newDevice);
            }
            ComboBox_deviceList.SelectedIndex = 0;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
        
            button1.IsEnabled = false;
            System.Drawing.Image image = BluetoothImageCom.takePhoto((long)((ComboBoxItem)ComboBox_deviceList.SelectedItem).Tag);
            button1.IsEnabled = true;
            canvasForImage1.Background = new ImageBrush(ConvertDrawingImageToWPFImage(image, 480, 720).Source);
            completePicture = (System.Drawing.Bitmap)image;
        }
        private System.Windows.Controls.Image ConvertDrawingImageToWPFImage(System.Drawing.Image gdiImg,int height,int width)
        {


            System.Windows.Controls.Image img = new System.Windows.Controls.Image();

            //convert System.Drawing.Image to WPF image
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(gdiImg);
            IntPtr hBitmap = bmp.GetHbitmap();
            System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            img.Source = WpfBitmap;
            img.Width = width;
            img.Height = height;
            img.Stretch = System.Windows.Media.Stretch.Fill;
            return img;
        }

        private void copyphoto_Click(object sender, RoutedEventArgs e)
        {
            if (Crop.IsChecked==true && CropArea!=null)
            {
                double left = Canvas.GetLeft(CropArea);
                double right = left + CropArea.Width;
                double top = Canvas.GetTop(CropArea);
                double bottom = top + CropArea.Height;

                int propLeft = (int)(left * (completePicture.Width / canvasForImage1.Width));
                int propRight = (int)(right * (completePicture.Width / canvasForImage1.Width));
                int propTop = (int)(top * (completePicture.Height / canvasForImage1.Height));
                int propBottom = (int)(bottom * (completePicture.Height / canvasForImage1.Height));


                Int32Rect region = new Int32Rect(propLeft,propTop,propRight-propLeft,propBottom-propTop);

                CroppedBitmap croppedbmp = new CroppedBitmap(
                    System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        completePicture.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()),
                    region);
                //BitmapSource bs = 
                Clipboard.SetImage(croppedbmp);
            }
            else
            {
                Clipboard.SetImage(
                    System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    completePicture.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()));
            }
        }

        double StartX;
        double StartY;
        double FinishX;
        double FinishY;
        Rectangle CropArea;
        private void canvasForImage1_down(object sender, MouseButtonEventArgs e)
        {
            if (Crop.IsChecked==true && e.ChangedButton==MouseButton.Left)
            {
                canvasForImage1.Children.Remove(CropArea);
                StartX = e.GetPosition(canvasForImage1).X;
                StartY = e.GetPosition(canvasForImage1).Y;
            }
        }

        private void canvasForImage1_up(object sender, MouseButtonEventArgs e)
        {
            if (Crop.IsChecked == true && e.ChangedButton == MouseButton.Left)
            {
                FinishX = e.GetPosition(canvasForImage1).X;
                FinishY = e.GetPosition(canvasForImage1).Y;

                CropArea = new Rectangle();
                CropArea.Stroke = Brushes.Black;
                CropArea.StrokeThickness = 3;
                CropArea.Fill = new SolidColorBrush(Color.FromArgb(50,20,20,20));
                
                
                
                if (FinishX>StartX)
                    Canvas.SetLeft(CropArea, StartX);
                else
                    Canvas.SetLeft(CropArea, FinishX);
                if (FinishY > StartY)
                    Canvas.SetTop(CropArea, StartY);
                else
                    Canvas.SetTop(CropArea, FinishY);

                CropArea.Width = Math.Abs(StartX - FinishX);
                CropArea.Height = Math.Abs(StartY - FinishY);
                canvasForImage1.Children.Add(CropArea);
                
            }
        }

    }
     
}
