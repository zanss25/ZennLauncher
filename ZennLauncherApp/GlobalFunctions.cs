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
        public static GlobalFunctions Instance { get; } = new GlobalFunctions();

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, string lpIconPath, ref ushort lpiIcon);

        public int build = 10400;
        public string version = "0.4 Beta (Indev)";

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

        public enum LaunchResult
        {
            Success,
            CancelledByUser,  // UAC cancel atau user cancel
            Failed
        }

        public LaunchResult LaunchFile(string filePath, string args)
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

                // Kalau null, berarti UAC di-cancel atau proses gagal start
                if (runningProcess == null)
                {
                    return LaunchResult.CancelledByUser;
                }

                return LaunchResult.Success;
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1223) // User cancelled UAC
                {
                    Debug.WriteLine("User cancelled UAC.");
                    return LaunchResult.CancelledByUser;
                }

                MessageBox.Show("Launch failed: " + ex.Message);
                return LaunchResult.Failed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("System error: " + ex.Message);
                return LaunchResult.Failed;
            }
        }

        public List<TabModel> SidebarTabs = new List<TabModel>
        {
            new TabModel
            {
                ItemTag = "games",
                DisplayIcon = "\uf11b;",
                DisplayName = "Games",
                TargetView = typeof(GamesView)
            },

            new TabModel
            {
                ItemTag = "tools",
                DisplayIcon = "\uf552;", //f7d9
                DisplayName = "Tools",
                TargetView = typeof(ToolsView)
            },

            new TabModel
            {
                ItemTag = "categories",
                DisplayIcon = "\uf5fd;", 
                DisplayName = "Library",
                TargetView = typeof(CategoriesView)
            },

            new TabModel
            {
                ItemTag = "boards",
                DisplayIcon = "\uf51b;",
                DisplayName = "Boards",
                TargetView = typeof(BoardsView)
            }
        };

        public List<SortCriterion> SortOptions = new List<SortCriterion>
        {
            new SortCriterion
            {
                SortByTag = "title",
                DisplayName = "Alphabetical"
            },

            new SortCriterion
            {
                SortByTag = "launch_times",
                DisplayName = "Most Launched"
            },

            new SortCriterion
            {
                SortByTag = "last_launched",
                DisplayName = "Last Launched"
            },

            new SortCriterion
            {
                SortByTag = "id",
                DisplayName = "Last Added"
            }
        };

        public List<TabModel> SettingsTabs = new List<TabModel>
        {
            new TabModel
            {
                ItemTag = "general",
                DisplayName = "General",
                TargetView = typeof(GeneralSettingsView)
            },

            new TabModel
            {
                ItemTag = "myprofile",
                DisplayName = "Profile",
                TargetView = typeof(MyProfileSettingsView)
            },

            new TabModel
            {
                ItemTag = "interface",
                DisplayName = "Appearance",
                TargetView = typeof(InterfaceSettingsView)
            },

            new TabModel
            {
                ItemTag = "folders",
                DisplayName = "Folders",
                TargetView = typeof(FoldersSettingsView)
            },

            new TabModel
            {
                ItemTag = "categories",
                DisplayName = "Categories",
                TargetView = typeof(CategoriesSettingsView)
            },

            new TabModel
            {
                ItemTag = "advanced",
                DisplayName = "Advanced",
                TargetView = typeof(AdvancedSettingsView)
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

        public T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T t)
                    return t;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

    }
}
