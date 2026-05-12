using Microsoft.Win32;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;

internal class Program
{
    private const string PIPE_NAME = "ghub-rgb-cycler";

    // Dynamic Lighting setting (per user) [2](https://www.reddit.com/r/LogitechG/comments/n7xz5n/g_hub_doesnt_switch_profiles_automatically_even/)[3](https://forum.turtlecraft.gg/viewtopic.php?p=155416)
    private const string DL_KEY = @"Software\Microsoft\Lighting";
    private const string DL_VALUE = "AmbientLightingEnabled";

    // Hotkeys: F13/F14/F15
    private const int HOTKEY_F13 = 0xF13;
    private const int HOTKEY_F14 = 0xF14;
    private const int HOTKEY_F15 = 0xF15;

    private const uint MOD_NONE = 0x0000;
    private const uint VK_F13 = 0x7C;
    private const uint VK_F14 = 0x7D;
    private const uint VK_F15 = 0x7E;

    private const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(in MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(in MSG lpMsg);

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public nuint wParam;
        public nint lParam;
        public uint time;
        public POINT pt;
        public uint lPrivate;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    static void Main()
    {
        Console.WriteLine("GhubRgbAgent started (listening for F13/F14/F15).");

        if (!RegisterHotKey(IntPtr.Zero, HOTKEY_F13, MOD_NONE, VK_F13) ||
            !RegisterHotKey(IntPtr.Zero, HOTKEY_F14, MOD_NONE, VK_F14) ||
            !RegisterHotKey(IntPtr.Zero, HOTKEY_F15, MOD_NONE, VK_F15))
        {
            Console.WriteLine($"RegisterHotKey failed: {Marshal.GetLastWin32Error()}");
            return;
        }

        try
        {
            while (GetMessage(out var msg, IntPtr.Zero, 0, 0) != 0)
            {
                if (msg.message == WM_HOTKEY)
                {
                    int mode = msg.wParam switch
                    {
                        HOTKEY_F13 => 1,
                        HOTKEY_F14 => 2,
                        HOTKEY_F15 => 3,
                        _ => 0
                    };

                    if (mode != 0)
                    {
                        // Tell service what mode we're in (optional)
                        //_ = TrySendToService($"set={mode}");

                        // Apply your rule: mode 3 => Dynamic Lighting ON, else OFF
                        SetDynamicLighting(enabled: mode == 3);

                        Console.WriteLine($"Mode={mode} => Dynamic Lighting {(mode == 3 ? "ON" : "OFF")}");
                    }
                }

                TranslateMessage(msg);
                DispatchMessage(msg);
            }
        }
        finally
        {
            UnregisterHotKey(IntPtr.Zero, HOTKEY_F13);
            UnregisterHotKey(IntPtr.Zero, HOTKEY_F14);
            UnregisterHotKey(IntPtr.Zero, HOTKEY_F15);
        }
    }

    private static void SetDynamicLighting(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(DL_KEY, writable: true);
        key?.SetValue(DL_VALUE, enabled ? 1 : 0, RegistryValueKind.DWord);
    }

    private static bool TrySendToService(string message)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut);
            client.Connect(150);
            WriteMessage(client, message);
            _ = ReadMessage(client);
            return true;
        }
        catch { return false; }
    }

    private static void WriteMessage(PipeStream pipe, string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        pipe.Write(bytes, 0, bytes.Length);
        pipe.Flush();
    }

    private static string ReadMessage(PipeStream pipe)
    {
        var buffer = new byte[1024];
        var sb = new StringBuilder();
        do
        {
            int read = pipe.Read(buffer, 0, buffer.Length);
            if (read == 0) break;
            sb.Append(Encoding.UTF8.GetString(buffer, 0, read));
        } while (!pipe.IsMessageComplete);
        return sb.ToString().Trim();
    }
}