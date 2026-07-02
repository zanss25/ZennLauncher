using Microsoft.Win32;
using System;
using System.Diagnostics;

public static class RegistryHelper
{
    /// <summary>
    /// Menyimpan atau memperbarui nilai di Windows Registry (HKCU).
    /// </summary>
    /// <param name="subKeyPath">Jalur subkey di dalam HKCU (misal: @"Software\ZennLauncher\Settings")</param>
    /// <param name="keyName">Nama variabel/key yang ingin disimpan</param>
    /// <param name="value">Nilai yang ingin disimpan (bisa string, int, bool, dll)</param>
    /// <returns>True jika berhasil, False jika gagal atau diblokir OS/Antivirus</returns>
    public static bool SaveSetting(string subKeyPath, string keyName, object value)
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(subKeyPath, true))
            {
                if (key != null)
                {
                    // Registry secara native mengenali tipe data string, int (DWORD), dll.
                    key.SetValue(keyName, value);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RegistryHelper] Gagal menyimpan {keyName}: {ex.Message}");
        }
        return false;
    }

    /// <summary>
    /// Menghapus sebuah key/variabel dari Windows Registry (HKCU).
    /// </summary>
    public static bool DeleteSetting(string subKeyPath, string keyName)
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subKeyPath, true))
            {
                if (key != null && key.GetValue(keyName) != null)
                {
                    key.DeleteValue(keyName);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RegistryHelper] Gagal menghapus {keyName}: {ex.Message}");
        }
        return false;
    }
}