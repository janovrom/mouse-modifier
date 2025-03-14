using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    static void Main()
    {
        Hook.Start();
        Application.Run();
        Hook.Stop();
    }
}

public partial class Hook
{
    private static IntPtr _hookID = IntPtr.Zero;
    private const int WH_MOUSE_LL = 14;
    private const int WH_KEYBOARD_LL = 13;

    private static bool mouseModifierActive = false;

    public static void Start()
    {
        _hookID = SetHook(MouseHookCallback, WH_MOUSE_LL);
        SetHook(KeyboardHookCallback, WH_KEYBOARD_LL);
    }

    public static void Stop()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelProc proc, int hookType)
    {
        using Process process = Process.GetCurrentProcess();
        using ProcessModule? module = process.MainModule;

        return module is null
            ? throw new InvalidOperationException("Failed to get current module.")
            : SetWindowsHookEx(hookType, proc, GetModuleHandle(module.ModuleName), 0);
    }

    private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var mouseStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            if (wParam == (IntPtr)0x020B) // WM_XBUTTONDOWN
            {
                if ((mouseStruct.mouseData >> 16) == 1) // XButton1 (Back Button)
                {
                    mouseModifierActive = true;
                }
            }
            else if (wParam == (IntPtr)0x020C) // WM_XBUTTONUP
            {
                mouseModifierActive = false;
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && mouseModifierActive && wParam == (IntPtr)0x100) // WM_KEYDOWN
        {
            int vkCode = Marshal.ReadInt32(lParam);

            // Map numbers 1-0 to F1-F10
            if (vkCode >= (int)Keys.D1 && vkCode <= (int)Keys.D9)
            {
                SendKey((ushort)(Keys.F1 + (vkCode - (int)Keys.D1)));
                return (IntPtr)1; // Suppress the original key
            }
            else if (vkCode == (int)Keys.D0) // '0' to F10
            {
                SendKey((ushort)Keys.F10);
                return (IntPtr)1; // Suppress the original key
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static void SendKey(ushort keyCode)
    {
        INPUT[] inputs =
        [
            new INPUT
            {
                type = 1, // Keyboard input
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        dwFlags = 0 // Key down
                    }
                }
            },
            new INPUT
            {
                type = 1,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = keyCode,
                        dwFlags = 2 // Key up
                    }
                }
            }
        ];
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnhookWindowsHookEx(IntPtr hhk);

    [LibraryImport("user32.dll")]
    private static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr GetModuleHandle(string lpModuleName);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}
