namespace hyakushiki_kannon.Core.Geometry;

/// <summary>
/// An axis-aligned rectangle in virtual-screen pixel space, used both for screen/monitor
/// bounds and for the cells produced by grid subdivision.
/// </summary>
public readonly record struct GridRect
{
    public GridRect(double x, double y, double width, double height)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be non-negative.");
        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be non-negative.");

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public double X { get; }
    public double Y { get; }
    public double Width { get; }
    public double Height { get; }

    public double Left => X;
    public double Top => Y;
    public double Right => X + Width;
    public double Bottom => Y + Height;

    /// <summary>The geometric centre of the rectangle - where the cursor jumps when a cell is chosen.</summary>
    public GridPoint Center => new(X + Width / 2.0, Y + Height / 2.0);

    /// <summary>True when <paramref name="point"/> lies within the rectangle (edges inclusive).</summary>
    public bool Contains(GridPoint point) =>
        point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;

    /// <summary>
    /// Returns a copy of <paramref name="point"/> clamped so it stays inside this rectangle.
    /// </summary>
    public GridPoint Clamp(GridPoint point) => new(
        Math.Clamp(point.X, Left, Right),
        Math.Clamp(point.Y, Top, Bottom));

    public override string ToString() => $"[{X:0.##}, {Y:0.##}, {Width:0.##}x{Height:0.##}]";
}
