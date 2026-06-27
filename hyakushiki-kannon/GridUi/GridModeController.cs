using System.Windows.Input;
using hyakushiki_kannon.Core;
using hyakushiki_kannon.Core.Input;
using hyakushiki_kannon.Interop;

namespace hyakushiki_kannon.GridUi;

/// <summary>
/// Top-level glue for the running app. Owns the global hotkey, the grid-mode state machine and
/// the overlay window, and routes overlay keystrokes into the core <see cref="GridInputRouter"/>.
/// This is the one place where Win32, WPF and the core logic meet; all decision-making lives in
/// the (unit-tested) core.
/// </summary>
public sealed class GridModeController : IDisposable
{
    private readonly IScreenProvider _screen;
    private readonly GridSession _session;
    private readonly GridInputRouter _router;
    private readonly GlobalHotkey _hotkey;

    // A single overlay window, reused (shown/hidden) across activations rather than recreated.
    private GridOverlayWindow? _overlay;

    public GridModeController(GridConfig? config = null)
    {
        _screen = new VirtualScreenProvider();
        _session = new GridSession(new Win32PointerDevice(), config ?? GridConfig.Default);
        _router = new GridInputRouter(_session);
        _hotkey = new GlobalHotkey(); // Alt+G
        _hotkey.Pressed += OnHotkeyPressed;
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        if (_session.IsActive)
        {
            // Pressing the hotkey again while active cancels grid mode.
            _session.Cancel();
            _overlay?.HideOverlay();
            return;
        }

        _session.Activate(_screen.VirtualScreenBounds);
        EnsureOverlay().ShowOverlay();
        RefreshOverlay();
    }

    /// <summary>
    /// Routes one overlay keystroke. Returns <c>true</c> if grid mode consumed the key so the
    /// overlay can mark it handled (and let unbound keys pass through).
    /// </summary>
    private bool OnOverlayKey(Key key)
    {
        if (!_session.IsActive)
            return false;

        var boundsBefore = _session.CurrentBounds;

        bool consumed;
        if (WpfKeyTranslator.TryGetSpecial(key, out var special))
            consumed = _router.HandleSpecial(special);
        else if (WpfKeyTranslator.TryGetChar(key, out var c))
            consumed = _router.HandleChar(c);
        else
            consumed = false;

        if (!_session.IsActive)
            _overlay?.HideOverlay();
        else if (_session.CurrentBounds != boundsBefore)
            RefreshOverlay(); // only a drill/back changes the grid; nudges move just the cursor

        return consumed;
    }

    private GridOverlayWindow EnsureOverlay()
    {
        _overlay ??= new GridOverlayWindow { KeyHandler = OnOverlayKey };
        return _overlay;
    }

    private void RefreshOverlay() =>
        _overlay?.Refresh(_screen.VirtualScreenBounds, _session.CurrentBounds, _session.Config);

    public void Dispose()
    {
        _hotkey.Pressed -= OnHotkeyPressed;
        _hotkey.Dispose();
        _overlay?.Close();
        _overlay = null;
    }
}
