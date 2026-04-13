using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SopranosCharactersCms.Converters
{
    public class RelativeImagePathToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string relativePath = value as string;
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string placeholderPath = Path.Combine(baseDirectory, "Resources", "Images", "placeholder.png");

            string finalPath = placeholderPath;
            if (!string.IsNullOrWhiteSpace(relativePath))
            {
                string candidatePath = Path.IsPathRooted(relativePath)
                    ? relativePath
                    : Path.Combine(baseDirectory, relativePath);

                if (!File.Exists(candidatePath) && !Path.IsPathRooted(relativePath))
                {
                    string projectDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "..", ".."));
                    candidatePath = Path.Combine(projectDirectory, relativePath);
                }

                if (File.Exists(candidatePath))
                {
                    finalPath = candidatePath;
                }
            }

            if (!File.Exists(finalPath))
            {
                return null;
            }

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(finalPath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
