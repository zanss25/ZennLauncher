using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ZennLauncherApp
{
    public class ProfileImageConverter : IValueConverter
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 'value' adalah ID dari binding
            if (value == null) return GetDefault();

            // Ambil Base Directory (Variabel di C#)
            string parentDir = func.dataPath;
            string profileId = value.ToString();

            // Susun path folder: FolderExe/Profiles/ID/
            string folderPath = Path.Combine(parentDir, profileId);

            if (Directory.Exists(folderPath))
            {
                // Cek ekstensi mana yang ada (avatar.png atau avatar.jpg)
                string[] extensions = { ".png", ".jpg", ".jpeg" };
                foreach (var ext in extensions)
                {
                    string fullPath = Path.Combine(folderPath, "avatar" + ext);
                    if (File.Exists(fullPath))
                    {
                        // Gunakan CacheOption agar file tidak "terkunci" oleh WPF
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        return bitmap;
                    }
                }
            }

            return GetDefault();
        }

        private BitmapImage GetDefault()
        {
            return new BitmapImage(new Uri("pack://application:,,,/default_avatar.jpg"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}