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
    private static readonly Brush DimBrush = new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x00, 0x00));
    private static readonly Pen LinePen = new(new SolidColorBrush(Color.FromArgb(0xCC, 0x33, 0xCC, 0xFF)), 1.0);
    private static readonly Brush LabelBrush = Brushes.White;
    private static readonly Brush LabelBackBrush = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x33, 0x66));
    private static readonly Typeface LabelTypeface = new("Consolas");

    static GridOverlayElement()
    {
        DimBrush.Freeze();
        LinePen.Freeze();
        LabelBackBrush.Freeze();
    }

    private GridRect _virtualScreen;
    private GridRect _currentBounds;
    private GridConfig _config = GridConfig.Default;

    /// <summary>Updates what the overlay shows and requests a repaint.</summary>
    public void Update(GridRect virtualScreen, GridRect currentBounds, GridConfig config)
    {
        _virtualScreen = virtualScreen;
        _currentBounds = currentBounds;
        _config = config;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(DimBrush, null, new Rect(0, 0, ActualWidth, ActualHeight));

        if (_virtualScreen.Width <= 0 || _virtualScreen.Height <= 0)
            return;

        var cells = Grid.Subdivide(_currentBounds, _config.Rows, _config.Cols);
        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            var topLeft = Map(cell.Left, cell.Top);
            var bottomRight = Map(cell.Right, cell.Bottom);
            dc.DrawRectangle(null, LinePen, new Rect(topLeft, bottomRight));

            var label = char.ToUpperInvariant(_config.KeyMap.GetKey(i)).ToString();
            var text = new FormattedText(
                label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                LabelTypeface, 20, LabelBrush, dpi);

            var center = Map(cell.Center.X, cell.Center.Y);
            var origin = new Point(center.X - text.Width / 2, center.Y - text.Height / 2);
            dc.DrawRectangle(LabelBackBrush, null,
                new Rect(origin.X - 3, origin.Y - 1, text.Width + 6, text.Height + 2));
            dc.DrawText(text, origin);
        }
    }

    private Point Map(double pixelX, double pixelY) => new(
        (pixelX - _virtualScreen.X) / _virtualScreen.Width * ActualWidth,
        (pixelY - _virtualScreen.Y) / _virtualScreen.Height * ActualHeight);
}
