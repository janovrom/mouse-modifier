using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Janovrom.MouseModifier.WindowsService;

public partial class Hook
{
    private const int _SupressKey = 1;

    private static nint _hookID = IntPtr.Zero;
    private static bool _mouseModifierActive = false;

    public static void Start()
    {
        _hookID = SetHook(MouseHookCallback, HookTypes.WH_MOUSE_LL);
        SetHook(KeyboardHookCallback, HookTypes.WH_KEYBOARD_LL);
    }

    public static void Stop()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static nint SetHook(LowLevelProc proc, int hookType)
    {
        using Process process = Process.GetCurrentProcess();
        using ProcessModule? module = process.MainModule;

        return module is null
            ? throw new InvalidOperationException("Failed to get current module.")
            : SetWindowsHookEx(hookType, proc, GetModuleHandleA(module.ModuleName), 0);
    }

    private delegate nint LowLevelProc(int nCode, nint wParam, nint lParam);

    private static nint MouseHookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            var mouseStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            if (wParam == MouseAndKeyboardInputs.VM_XBUTTONDOWN)
            {
                if (mouseStruct.mouseData >> 16 == MouseAndKeyboardInputs.WM_XBUTTON2)
                {
                    _mouseModifierActive = true;
                }
            }
            else if (wParam == MouseAndKeyboardInputs.VM_XBUTTONUP)
            {
                _mouseModifierActive = false;
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static nint KeyboardHookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && _mouseModifierActive && wParam == 0x100) // WM_KEYDOWN
        {
            int vkCode = Marshal.ReadInt32(lParam);

            // Map numbers 1-0 to F1-F10
            switch (vkCode)
            {
                case >= (int)Keys.D1 and <= (int)Keys.D9:
                    SendKey((ushort)(Keys.F1 + (vkCode - (int)Keys.D1)));
                    return _SupressKey;
                case (int)Keys.D0:
                    SendKey((ushort)Keys.F10);
                    return _SupressKey;
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static void SendKey(ushort keyCode)
    {
        INPUT[] inputs =
        [
            INPUT.ForKeyDown(keyCode),
            INPUT.ForKeyUp(keyCode)
        ];
        Debug.WriteLine($"Sending key: {(Keys)keyCode}");
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// Contains information about a low-level mouse input event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public nint dwExtraInfo;
    }

    /// <summary>
    /// The POINT structure defines the x- and y-coordinates of a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExA", SetLastError = true)]
    private static partial nint SetWindowsHookEx(int idHook, LowLevelProc lpfn, nint hMod, uint dwThreadId);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnhookWindowsHookEx(nint hhk);

    [LibraryImport("user32.dll")]
    private static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint GetModuleHandleA(string lpModuleName);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint MapVirtualKeyA(uint uCode, uint uMapType);

    /// <summary>
    /// Used by SendInput to store information for synthesizing input events
    /// such as keystrokes, mouse movement, and mouse clicks.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        /// <summary>
        /// The event is a keyboard event. Use the ki structure of the union.
        /// </summary>
        internal const int INPUT_KEYBOARD = 1;

        public int type;
        public InputUnion u;

        internal static INPUT ForKeyboard(KEYBDINPUT ki) => new() { type = INPUT_KEYBOARD, u = new InputUnion { ki = ki } };

        internal static INPUT ForKeyUp(ushort keyCode) => ForKeyboard(new KEYBDINPUT
        {
            wVk = keyCode,
            dwFlags = MouseAndKeyboardInputs.KEYEVENTF_KEYUP,
            wScan = (ushort)MapVirtualKeyA(keyCode, MouseAndKeyboardInputs.MAPVK_VK_TO_VSC),
        });

        internal static INPUT ForKeyDown(ushort keyCode) => ForKeyboard(new KEYBDINPUT
        {
            wVk = keyCode,
            wScan = (ushort)MapVirtualKeyA(keyCode, MouseAndKeyboardInputs.MAPVK_VK_TO_VSC),
        });
    }

    /// <summary>
    /// Implements a union for the <see cref="INPUT"/> struct.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KEYBDINPUT ki;

        [FieldOffset(0)]
        public MOUSEINPUT mi;

        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }

    /// <summary>
    /// Contains information about a simulated keyboard event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }

    /// <summary>
    /// Contains information about a simulated mouse event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }

    /// <summary>
    /// Contains information about a simulated message generated
    /// by an input device other than a keyboard or mouse.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }
}
