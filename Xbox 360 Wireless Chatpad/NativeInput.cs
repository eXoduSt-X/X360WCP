using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Xbox360WirelessChatpad
{
    /// <summary>
    /// Envío de input nativo a Windows (teclado + app commands multimedia).
    /// Usa keybd_event porque SendInput es descartado silenciosamente por
    /// Windows cuando el proceso no es elevado y el foco no está en el mismo
    /// nivel de integridad.
    /// </summary>
    internal static class NativeInput
    {
        // =========================================================
        // P/INVOKE
        // =========================================================

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        // =========================================================
        // CONSTANTES
        // =========================================================

        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        private const int WM_APPCOMMAND = 0x0319;
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);

        // Virtual key codes
        public const ushort VK_SHIFT = 0x10;
        public const ushort VK_LSHIFT = 0xA0;
        public const ushort VK_CONTROL = 0x11;
        public const ushort VK_MENU = 0x12;   // Alt
        public const ushort VK_LWIN = 0x5B;
        public const ushort VK_TAB = 0x09;
        public const ushort VK_ESCAPE = 0x1B;
        public const ushort VK_F4 = 0x73;
        public const ushort VK_LEFT = 0x25;
        public const ushort VK_RIGHT = 0x27;
        public const ushort VK_M = 0x4D;

        public const ushort VK_VOLUME_UP = 0xAF;
        public const ushort VK_VOLUME_DOWN = 0xAE;
        public const ushort VK_MEDIA_NEXT = 0xB0;
        public const ushort VK_MEDIA_PREV = 0xB1;

        // =========================================================
        // APP COMMANDS
        // =========================================================

        public enum AppCommand : int
        {
            VolumeDown = 9,
            VolumeUp = 10,
            MediaNext = 11,
            MediaPrevious = 12
        }

        public static void SendAppCommand(AppCommand cmd)
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                hwnd = HWND_BROADCAST;

            IntPtr lParam = (IntPtr)(((int)cmd) << 16);
            SendMessage(hwnd, WM_APPCOMMAND, hwnd, lParam);
        }

        // =========================================================
        // FOCO
        // =========================================================

        public static void ReleaseFocusIfOwnedBy(IntPtr windowHandle)
        {
            if (GetForegroundWindow() == windowHandle)
                SetForegroundWindow(GetDesktopWindow());
        }

        // =========================================================
        // TECLAS — keybd_event
        // =========================================================

        public static void SendKey(ushort vKey, bool keyUp)
        {
            SendKeysInternal(new[] { (vKey, keyUp) });
        }

        public static void SendKeys(params (ushort vk, bool up)[] keys)
        {
            SendKeysInternal(keys);
        }

        public static void PressChord(ushort modifier, ushort key)
        {
            SendKeysInternal(new[]
            {
                (modifier, false),
                (key, false),
                (key, true),
                (modifier, true)
            });
        }

        public static void HoldChord(ushort modifier, ushort key, bool release)
        {
            if (release)
                SendKeysInternal(new[] { (key, true), (modifier, true) });
            else
                SendKeysInternal(new[] { (modifier, false), (key, false) });
        }

        public static void HoldKey(ushort vk, bool release)
        {
            SendKey(vk, release);
        }

        // =========================================================
        // IMPLEMENTACIÓN INTERNA
        // =========================================================

        private static void SendKeysInternal((ushort vk, bool up)[] keys)
        {
            if (keys == null || keys.Length == 0) return;

            foreach (var entry in keys)
            {
                ushort vk = entry.vk;
                bool isUp = entry.up;
                bool extended = IsExtendedKey(vk);

                uint flags = 0;
                if (isUp) flags |= KEYEVENTF_KEYUP;
                if (extended) flags |= KEYEVENTF_EXTENDEDKEY;

                keybd_event((byte)vk, 0, flags, UIntPtr.Zero);
            }
        }

        private static bool IsExtendedKey(ushort vk)
        {
            switch (vk)
            {
                case VK_VOLUME_UP:
                case VK_VOLUME_DOWN:
                case VK_MEDIA_NEXT:
                case VK_MEDIA_PREV:
                case VK_LEFT:
                case VK_RIGHT:
                case VK_LWIN:
                    return true;
                default:
                    return false;
            }
        }
    }
}