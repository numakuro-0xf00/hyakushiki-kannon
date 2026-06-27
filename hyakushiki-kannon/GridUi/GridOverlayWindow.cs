using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using hyakushiki_kannon.Core;
using hyakushiki_kannon.Core.Geometry;
using hyakushiki_kannon.Interop;

namespace hyakushiki_kannon.GridUi;

/// <summary>
/// The full-screen, dimmed, top-most window that hosts the <see cref="GridOverlayElement"/> and
/// captures the keystrokes that drive grid mode. It spans the entire virtual screen (all
/// monitors) using WPF's DIP virtual-screen metrics, and is made click-through so synthetic
/// clicks reach the application underneath. A single instance is reused across activations
/// (shown/hidden) rather than recreated.
/// </summary>
internal sealed class GridOverlayWindow : Window
{
    private readonly GridOverlayElement _element = new();

    /// <summary>
    /// Invoked for each key pressed while the overlay is focused. Returns <c>true</c> if grid
    /// mode consumed the key (it is then marked handled), <c>false</c> to let it pass through.
    /// </summary>
    public Func<Key, bool>? KeyHandler { get; set; }

    public GridOverlayWindow()
    {
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = true;
        Focusable = true;
        Content = _element;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Make the overlay click-through: synthetic mouse clicks (and any real ones) pass to the
        // application beneath instead of being captured by this top-most window. Keyboard focus is
        // unaffected, so the overlay still receives the keystrokes that drive grid mode.
        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE,
            exStyle | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_LAYERED);
    }

    /// <summary>Positions the window over the whole virtual screen and shows it focused.</summary>
    public void ShowOverlay()
    {
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        Show();
        Activate();
        Focus();
    }

    /// <summary>Hides the overlay, keeping the instance alive for the next activation.</summary>
    public void HideOverlay() => Hide();

    /// <summary>Repaints the overlay as the cell grid for the current drill state.</summary>
    public void ShowGrid(GridRect virtualScreen, GridRect currentBounds, GridConfig config) =>
        _element.UpdateGrid(virtualScreen, currentBounds, config);

    /// <summary>Repaints the overlay as the first-keystroke monitor-selection phase.</summary>
    public void ShowMonitorSelection(
        GridRect virtualScreen,
        IReadOnlyList<GridRect> monitors,
        IReadOnlyList<char> labels,
        int focusedIndex,
        GridConfig config) =>
        _element.UpdateMonitorSelection(virtualScreen, monitors, labels, focusedIndex, config);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        // System keys (e.g. Alt combos) arrive as Key.System; resolve to the real key.
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        // Only swallow the key if grid mode actually consumed it; otherwise let it pass through.
        e.Handled = KeyHandler?.Invoke(key) ?? false;
    }
}
