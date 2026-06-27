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
            CloseOverlay();
            return;
        }

        _session.Activate(_screen.VirtualScreenBounds);

        _overlay = new GridOverlayWindow();
        _overlay.KeyPressed += OnOverlayKey;
        _overlay.ShowOverlay();
        RefreshOverlay();
    }

    private void OnOverlayKey(object? sender, Key key)
    {
        if (WpfKeyTranslator.TryGetSpecial(key, out var special))
            _router.HandleSpecial(special);
        else if (WpfKeyTranslator.TryGetChar(key, out var c))
            _router.HandleChar(c);

        // Any action (click/cancel) may have ended the session; reflect that in the UI.
        if (_session.IsActive)
            RefreshOverlay();
        else
            CloseOverlay();
    }

    private void RefreshOverlay() =>
        _overlay?.Refresh(_screen.VirtualScreenBounds, _session.CurrentBounds, _session.Config);

    private void CloseOverlay()
    {
        if (_overlay is null)
            return;

        _overlay.KeyPressed -= OnOverlayKey;
        _overlay.Close();
        _overlay = null;
    }

    public void Dispose()
    {
        CloseOverlay();
        _hotkey.Pressed -= OnHotkeyPressed;
        _hotkey.Dispose();
    }
}
