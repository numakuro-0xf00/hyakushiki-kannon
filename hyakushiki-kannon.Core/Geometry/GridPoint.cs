namespace hyakushiki_kannon.Core.Geometry;

/// <summary>
/// A point in virtual-screen pixel space. Coordinates are kept as <see cref="double"/>
/// so repeated grid subdivision does not accumulate rounding error; convert to integer
/// device pixels only at the boundary (see <see cref="ToRounded"/>).
/// </summary>
public readonly record struct GridPoint(double X, double Y)
{
    /// <summary>Returns a copy with both coordinates rounded to the nearest integer.</summary>
    public GridPoint ToRounded() => new(Math.Round(X), Math.Round(Y));

    public override string ToString() => $"({X:0.##}, {Y:0.##})";
}
