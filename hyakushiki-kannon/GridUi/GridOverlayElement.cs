using System.Globalization;
using System.Windows;
using System.Windows.Media;
using hyakushiki_kannon.Core;
using hyakushiki_kannon.Core.Geometry;

namespace hyakushiki_kannon.GridUi;

/// <summary>
/// Draws the grid overlay: a dimmed backdrop, the current cell grid, and the home-row key
/// label centred in each cell. Core coordinates are in virtual-screen pixels; this element
/// covers the whole virtual screen, so it maps a pixel point to its own (DIP) space by simple
/// proportion - which is DPI-independent because both describe the same physical extent.
/// </summary>
internal sealed class GridOverlayElement : FrameworkElement
{
    private const double HintMargin = 10.0; // DIP inset of the monitor-key guide from the corner

    private static readonly Brush DimBrush = new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x00, 0x00));
    private static readonly Pen LinePen = new(new SolidColorBrush(Color.FromArgb(0xCC, 0x33, 0xCC, 0xFF)), 1.0);
    private static readonly Brush LabelBrush = Brushes.White;
    private static readonly Brush LabelBackBrush = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x33, 0x66));
    private static readonly Typeface LabelTypeface = new("Consolas");

    // Cues for the monitor-selection phase: an amber border + amber guide background mark the
    // focused monitor, where typing a cell key (asdf...) acts immediately - no monitor key needed.
    private static readonly Pen FocusPen = new(new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xCC, 0x33)), 3.0);
    private static readonly Brush FocusHintBackBrush = new SolidColorBrush(Color.FromArgb(0xE0, 0xCC, 0x66, 0x00));

    static GridOverlayElement()
    {
        DimBrush.Freeze();
        LinePen.Freeze();
        LabelBackBrush.Freeze();
        FocusPen.Freeze();
        FocusHintBackBrush.Freeze();
    }

    private enum OverlayMode
    {
        Grid,
        MonitorSelect,
    }

    private OverlayMode _mode = OverlayMode.Grid;
    private GridRect _virtualScreen;
    private GridRect _currentBounds;
    private GridConfig _config = GridConfig.Default;
    private IReadOnlyList<GridRect> _monitors = Array.Empty<GridRect>();
    private IReadOnlyList<char> _monitorLabels = Array.Empty<char>();
    private int _focusedIndex;

    // Cached cell labels. The text/typeface/size are constant per (config, dpi), so the
    // (relatively expensive) FormattedText layout is built once and only the draw position
    // changes per frame. Rebuilt when the config or DPI changes.
    private FormattedText[]? _labels;
    private GridConfig? _labelConfig;
    private double _labelDpi;

    /// <summary>Shows the cell grid for the current drill state and requests a repaint.</summary>
    public void UpdateGrid(GridRect virtualScreen, GridRect currentBounds, GridConfig config)
    {
        _mode = OverlayMode.Grid;
        _virtualScreen = virtualScreen;
        _currentBounds = currentBounds;
        _config = config;
        InvalidateVisual();
    }

    /// <summary>
    /// First-keystroke phase: shows the cell grid on every monitor, with each monitor's selection
    /// key in its top-left corner and the focused monitor highlighted (where a cell key acts
    /// without choosing a monitor first).
    /// </summary>
    public void UpdateMonitorSelection(
        GridRect virtualScreen,
        IReadOnlyList<GridRect> monitors,
        IReadOnlyList<char> labels,
        int focusedIndex,
        GridConfig config)
    {
        _mode = OverlayMode.MonitorSelect;
        _virtualScreen = virtualScreen;
        _monitors = monitors;
        _monitorLabels = labels;
        _focusedIndex = focusedIndex;
        _config = config;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(DimBrush, null, new Rect(0, 0, ActualWidth, ActualHeight));

        if (_virtualScreen.Width <= 0 || _virtualScreen.Height <= 0)
            return;

        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        if (_mode == OverlayMode.MonitorSelect)
            RenderMonitorSelection(dc, dpi);
        else
            RenderGrid(dc, dpi);
    }

    private void RenderGrid(DrawingContext dc, double dpi) =>
        DrawCellGrid(dc, _currentBounds, GetLabels(dpi));

    private void RenderMonitorSelection(DrawingContext dc, double dpi)
    {
        var cellLabels = GetLabels(dpi);

        for (var i = 0; i < _monitors.Count; i++)
        {
            var monitor = _monitors[i];

            // Every monitor shows the full asdf grid - a live preview of where a cell key lands.
            DrawCellGrid(dc, monitor, cellLabels);

            // Highlight the focused monitor: a cell key acts here straight away (skip the monitor key).
            if (i == _focusedIndex)
                dc.DrawRectangle(null, FocusPen, new Rect(Map(monitor.Left, monitor.Top), Map(monitor.Right, monitor.Bottom)));

            // Monitor-selection key guide in the monitor's top-left corner.
            if (i < _monitorLabels.Count)
            {
                var hint = new FormattedText(
                    _monitorLabels[i].ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    LabelTypeface, 32, LabelBrush, dpi);
                var corner = Map(monitor.Left, monitor.Top);
                var origin = new Point(corner.X + HintMargin, corner.Y + HintMargin);
                DrawTextBox(dc, hint, origin, i == _focusedIndex ? FocusHintBackBrush : LabelBackBrush);
            }
        }
    }

    private void DrawCellGrid(DrawingContext dc, GridRect bounds, FormattedText[] labels)
    {
        var cells = Grid.Subdivide(bounds, _config.Rows, _config.Cols);
        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            dc.DrawRectangle(null, LinePen, new Rect(Map(cell.Left, cell.Top), Map(cell.Right, cell.Bottom)));

            if (i >= labels.Length)
                continue;
            var center = Map(cell.Center.X, cell.Center.Y);
            var text = labels[i];
            DrawTextBox(dc, text, new Point(center.X - text.Width / 2, center.Y - text.Height / 2), LabelBackBrush);
        }
    }

    private static void DrawTextBox(DrawingContext dc, FormattedText text, Point origin, Brush background)
    {
        dc.DrawRectangle(background, null,
            new Rect(origin.X - 3, origin.Y - 1, text.Width + 6, text.Height + 2));
        dc.DrawText(text, origin);
    }

    private FormattedText[] GetLabels(double dpi)
    {
        if (_labels is not null && _labelDpi == dpi && _config.Equals(_labelConfig))
            return _labels;

        var labels = new FormattedText[_config.CellCount];
        for (var i = 0; i < labels.Length; i++)
        {
            var label = char.ToUpperInvariant(_config.KeyMap.GetKey(i)).ToString();
            labels[i] = new FormattedText(
                label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                LabelTypeface, 20, LabelBrush, dpi);
        }

        _labels = labels;
        _labelConfig = _config;
        _labelDpi = dpi;
        return labels;
    }

    private Point Map(double pixelX, double pixelY) => new(
        (pixelX - _virtualScreen.X) / _virtualScreen.Width * ActualWidth,
        (pixelY - _virtualScreen.Y) / _virtualScreen.Height * ActualHeight);
}
