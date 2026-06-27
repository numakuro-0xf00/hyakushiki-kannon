using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Interop;

namespace hyakushiki_kannon.Interop;

/// <summary>
/// Registers a system-wide hotkey (default <c>Alt+G</c>, per concept.md) and raises
/// <see cref="Pressed"/> when it is hit from any application. Hosts a hidden message-only
/// <see cref="HwndSource"/> to receive <c>WM_HOTKEY</c>, so no visible window is required.
/// </summary>
public sealed class GlobalHotkey : IDisposable
{
    private const int HotkeyId = 0xA17F; // arbitrary, unique within this process

    private readonly HwndSource _source;
    private bool _registered;

    /// <summary>Raised on the UI thread each time the hotkey is pressed.</summary>
    public event EventHandler? Pressed;

    /// <param name="modifiers">Modifier keys (e.g. <c>MOD_ALT</c>).</param>
    /// <param name="key">Virtual-key code of the trigger key.</param>
    public GlobalHotkey(uint modifiers = NativeMethods.MOD_ALT, Key key = Key.G)
    {
        // A zero-sized message-only window is enough to own the hotkey and pump WM_HOTKEY.
        var parameters = new HwndSourceParameters("GridMouseHotkeyWindow")
        {
            Width = 0,
            Height = 0,
            ParentWindow = new nint(-3), // HWND_MESSAGE
        };
        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);

        var vk = (uint)KeyInterop.VirtualKeyFromKey(key);
        _registered = NativeMethods.RegisterHotKey(
            _source.Handle, HotkeyId, modifiers | NativeMethods.MOD_NOREPEAT, vk);
        if (!_registered)
            throw new Win32Exception("Failed to register the global hotkey (already in use?).");
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return nint.Zero;
    }

    public void Dispose()
    {
        if (_registered)
        {
            NativeMethods.UnregisterHotKey(_source.Handle, HotkeyId);
            _registered = false;
        }

        _source.RemoveHook(WndProc);
        _source.Dispose();
    }
}
