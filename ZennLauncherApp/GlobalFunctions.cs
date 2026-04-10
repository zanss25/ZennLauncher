using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZennLauncherApp
{
    public class GlobalFunctions
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, string lpIconPath, ref ushort lpiIcon);

        public int build = 10230;
        public string version = "0.2 Beta (Indev 3)";

        public string rootPath = AppDomain.CurrentDomain.BaseDirectory;

        public string dataPath = System.IO.Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                    "Zenn Launcher");

        public string dataPathDebug = System.IO.Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                    "Zenn Launcher", "DEBUG");

        public Process runningProcess = null;

        public ImageSource GetIconNative(string filePath)
        {
            try
            {
                ushort iconIndex = 0;
                IntPtr hIcon = ExtractAssociatedIcon(IntPtr.Zero, filePath, ref iconIndex);
                if (hIcon != IntPtr.Zero)
                {
                    return Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to retrieve the native icon: " + ex.Message);
            }
            return null;

        }

        public void LaunchFile(string filePath, string args)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(filePath)
                {
                    Arguments = args,
                    UseShellExecute = true,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(filePath)
                };

                runningProcess = Process.Start(startInfo);
            }
            catch (Win32Exception ex)
            {
                // Kode error 1223 adalah kode standar Windows untuk "User Cancelled"
                if (ex.NativeErrorCode == 1223)
                {
                    Debug.WriteLine("User gak jadi.");
                }
                else
                {
                    MessageBox.Show("Launch failed: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("System error: " + ex.Message);
            }
        }

        public List<SortCriterion> SortOptions = new List<SortCriterion>
        {
            new SortCriterion
            {
                SortBy = "title",
                DisplayName = "Alphabetical"
            },

            new SortCriterion
            {
                SortBy = "launch_times",
                DisplayName = "Most Launched"
            },

            new SortCriterion
            {
                SortBy = "last_launched",
                DisplayName = "Last Launched"
            },

            new SortCriterion
            {
                SortBy = "id",
                DisplayName = "Last Added"
            }
        };

        public static class SecurityHelper
        {
            public static string Encrypt(string plainText)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
                    return Convert.ToBase64String(encryptedData);
                }
                catch { return ""; }
            }

            public static string Decrypt(string cipherText)
            {
                try
                {
                    byte[] data = Convert.FromBase64String(cipherText);
                    byte[] decryptedData = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(decryptedData);
                }
                catch { return ""; }
            }
        }
    }
}
