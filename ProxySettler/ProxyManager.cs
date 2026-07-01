using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ProxySettler;

/// <summary>
/// Reads and writes the current user's system-wide (WinINet) proxy settings.
/// These are the same settings shown under Windows Settings -> Network -> Proxy,
/// so they affect Edge and most apps that respect the system proxy.
/// </summary>
internal static class ProxyManager
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
    private const int INTERNET_OPTION_REFRESH = 37;

    [DllImport("wininet.dll", SetLastError = true)]
    private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

    public static (bool Enabled, string? Server) GetStatus()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: false);
        if (key == null)
        {
            return (false, null);
        }

        var enabled = Convert.ToInt32(key.GetValue("ProxyEnable", 0)) == 1;
        var server = key.GetValue("ProxyServer") as string;
        return (enabled, server);
    }

    public static void Connect(string host, int port)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true)
            ?? throw new InvalidOperationException("Unable to open the Internet Settings registry key.");

        key.SetValue("ProxyServer", $"{host}:{port}", RegistryValueKind.String);
        key.SetValue("ProxyEnable", 1, RegistryValueKind.DWord);

        NotifySystem();
    }

    public static void Disconnect()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true)
            ?? throw new InvalidOperationException("Unable to open the Internet Settings registry key.");

        key.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);

        NotifySystem();
    }

    private static void NotifySystem()
    {
        // Tell already-running processes (Edge, Explorer, etc.) to re-read the settings
        // we just wrote to the registry - without this, changes only take effect after reboot/relogin.
        InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
        InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
    }
}
