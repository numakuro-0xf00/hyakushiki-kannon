using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using hyakushiki_kannon.Core;
using hyakushiki_kannon.Core.Geometry;

namespace hyakushiki_kannon.GridUi;

/// <summary>
/// The full-screen, click-through-dimmed, top-most window that hosts the
/// <see cref="GridOverlayElement"/> and captures the keystrokes that drive grid mode. It spans
/// the entire virtual screen (all monitors) using WPF's DIP virtual-screen metrics.
/// </summary>
internal sealed class GridOverlayWindow : Window
{
    private readonly GridOverlayElement _element = new();

    /// <summary>Raised for every key pressed while the overlay is focused.</summary>
    public event EventHandler<Key>? KeyPressed;

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

    /// <summary>Repaints the overlay for the current drill state.</summary>
    public void Refresh(GridRect virtualScreen, GridRect currentBounds, GridConfig config) =>
        _element.Update(virtualScreen, currentBounds, config);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        // System keys (e.g. Alt combos) arrive as Key.System; resolve to the real key.
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        KeyPressed?.Invoke(this, key);
        e.Handled = true;
    }
}
