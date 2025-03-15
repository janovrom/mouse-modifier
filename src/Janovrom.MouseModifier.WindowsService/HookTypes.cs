namespace Janovrom.MouseModifier.WindowsService;

/// <summary>
/// Each type of hook enables an application to monitor a different aspect of
/// the system's message-handling mechanism. It implements only a subset of all hooks.
/// </summary>
internal static class HookTypes
{
    /// <summary>
    /// The WH_MOUSE_LL hook enables you to monitor mouse input events about to be posted in a thread input queue
    /// </summary>
    public const int WH_MOUSE_LL = 14;

    /// <summary>
    /// The WH_KEYBOARD hook enables an application to monitor message traffic
    /// for WM_KEYDOWN and WM_KEYUP messages about to be returned by the GetMessage
    /// or PeekMessage function. You can use the WH_KEYBOARD hook to monitor 
    /// keyboard input posted to a message queue.
    /// </summary>
    public const int WH_KEYBOARD_LL = 13;
}