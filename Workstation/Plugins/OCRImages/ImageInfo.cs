using System.Windows.Media.Imaging;

namespace L3.Cargo.Workstation.Plugins.OCRImages
{
    public class ImageInfo
    {
        public BitmapImage imageSource { get; set; }
        public string FullName { get; set; }

        public ImageInfo ()
        {
            imageSource = null;
            FullName = "not set";
        }
    }
}
