using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;

namespace GPanoConverter
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

        static void ConvertToGPano(string sourceFile, string convertedFilePath)
        {
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(sourceFile);
            int width = image.Width;
            int height = image.Height;
            image.Dispose();

            int fullPanoWidthPixels = width;
            int fullPanoHeightPixels = height;
            int croppedAreaTopPixels = 0;

            if(width < height*2)
            {
                fullPanoWidthPixels = height * 2;
            }
            else if(width>height*2)
            {
                fullPanoHeightPixels = (int)(width / 2);
                croppedAreaTopPixels = (int)((fullPanoHeightPixels - height) / 2);
            }

            //Adding XMP Metadata according to https://developers.google.com/photo-sphere/metadata/
            FileStream f = File.Open(sourceFile, FileMode.Open);
            BitmapDecoder decoder = JpegBitmapDecoder.Create(f, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
            BitmapMetadata metadata = (BitmapMetadata)decoder.Frames[0].Metadata;
            BitmapMetadata md = new BitmapMetadata("jpg");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:UsePanoramaViewer", "True");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:CaptureSoftware", "GPanoConverter");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:StitchingSoftware", "Photo Sphere");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:ProjectionType", "equirectangular");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:PoseHeadingDegrees", "350.0");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:InitialViewHeadingDegrees", "90.0");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:InitialViewPitchDegrees", "0.0");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:InitialViewRollDegrees", "0.0");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:InitialHorizontalFOVDegrees", "75.0");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:CroppedAreaLeftPixels", "0");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:CroppedAreaTopPixels", croppedAreaTopPixels.ToString());
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:CroppedAreaImageWidthPixels", width.ToString());
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:CroppedAreaImageHeightPixels", height.ToString());
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:FullPanoWidthPixels", fullPanoWidthPixels.ToString());
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:FullPanoHeightPixels", fullPanoHeightPixels.ToString());
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:FirstPhotoDate", "2012-13-14T15:16:17.181Z");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:LastPhotoDate", "2012-13-14T15:16:17.181Z");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:SourcePhotosCount", "42");
            md.SetQuery(@"/xmp/{wstr=http://ns.google.com/photos/1.0/panorama/}:ExposureLockUsed", "False");
            BitmapFrame frame = BitmapFrame.Create(decoder.Frames[0], decoder.Frames[0].Thumbnail, md, decoder.Frames[0].ColorContexts);
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(frame);
            FileStream of = File.Open(convertedFilePath, FileMode.Create, FileAccess.Write);
            encoder.Save(of);
            of.Close();
            f.Close();
        }
        
        private void bConvert_Click(object sender, RoutedEventArgs e)
        {
            string sourceFile = tbPhotoPath.Text;
            string convertedFile = sourceFile.Remove(sourceFile.LastIndexOf("\\")) + "\\" + DateTime.Now.ToString("PANO_yyyyMMdd_HHmmss") + ".jpg";
            ConvertToGPano(sourceFile, convertedFile);
            if(cbReplace.IsChecked.Value)
            {
                File.Delete(sourceFile);
            }
            MessageBox.Show("File was converrted successfully!");
            tbPhotoPath.Text = "";
            tbWarning.Visibility = System.Windows.Visibility.Hidden;
        }

        private void bOpenPhoto_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPEG (.jpg)|*.jpg";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                tbPhotoPath.Text = dlg.FileName;
                tbWarning.Visibility = System.Windows.Visibility.Visible;
            }
        }


    }
}