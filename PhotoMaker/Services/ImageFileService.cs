using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoMaker.Client.Services
{
    internal class ImageFileService : Interfaces.IFileService
    {
        public object? Open(string filepath)
        {
            var bitmap = new BitmapImage();

            try
            {
                using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();

                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = null;
                    bitmap.StreamSource = fs;

                    bitmap.EndInit();
                    bitmap.Freeze();
                }

                // Конвертация в Pbgra32
                var converted = new FormatConvertedBitmap();
                converted.BeginInit();

                converted.Source = bitmap;
                converted.DestinationFormat = PixelFormats.Pbgra32;

                converted.EndInit();
                converted.Freeze();

                return converted;
            }
            catch (Exception) { return null; }
        }


        public void Save(string filepath, object data)
        {
            if (data == null || data is not BitmapSource bitmap) return;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write))
            {
                encoder.Save(fs);
            }
        }


        public ImageFileService() { }
    }
}
