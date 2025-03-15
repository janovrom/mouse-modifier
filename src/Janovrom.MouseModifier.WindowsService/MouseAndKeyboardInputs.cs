namespace Janovrom.MouseModifier.WindowsService;

internal static class MouseAndKeyboardInputs
{
    /// <summary>
    /// If specified, the key is being released. If not specified, the key is being pressed. 
    /// </summary>
    internal const int KEYEVENTF_KEYUP = 0x0002;

    /// <summary>
    /// The XBUTTON1 was clicked.
    /// </summary>
    public const nint WM_XBUTTON1 = 1;

    /// <summary>
    /// The XBUTTON2 was clicked.
    /// </summary>
    public const nint WM_XBUTTON2 = 2;

    /// <summary>
    /// The WM_XBUTTONDOWN message is posted when the XBUTTON1 or XBUTTON2 button is pressed.
    /// </summary>
    public const int VM_XBUTTONDOWN = 0x020B;

    /// <summary>
    /// The WM_XBUTTONUP message is posted when the XBUTTON1 or XBUTTON2 button is released.
    /// </summary>
    public const int VM_XBUTTONUP = 0x020C;

    /// <summary>
    /// The uCode parameter is a virtual-key code and is translated into a scan code.
    /// If it is a virtual-key code that does not distinguish between left- and right-hand keys,
    /// the left-hand scan code is returned. If there is no translation, the function returns 0.
    /// </summary>
    public const int MAPVK_VK_TO_VSC = 0;
}