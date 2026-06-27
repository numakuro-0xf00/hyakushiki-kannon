namespace hyakushiki_kannon.Core.Geometry;

/// <summary>
/// Pure grid-subdivision maths. Splits a rectangle into a <c>rows x cols</c> tiling whose
/// cells are returned in row-major order (left-to-right, top-to-bottom), matching the
/// reading order users expect from the on-screen grid labels.
/// </summary>
public static class Grid
{
    /// <summary>
    /// Divides <paramref name="bounds"/> into <paramref name="rows"/> x <paramref name="cols"/>
    /// cells. Cell edges are computed from the outer bounds (rather than by accumulating cell
    /// widths) so the cells tile the rectangle exactly with no gaps or overlap, even when the
    /// size does not divide evenly.
    /// </summary>
    /// <returns>The cells in row-major order; index <c>r * cols + c</c> is row <c>r</c>, column <c>c</c>.</returns>
    public static IReadOnlyList<GridRect> Subdivide(GridRect bounds, int rows, int cols)
    {
        if (rows < 1)
            throw new ArgumentOutOfRangeException(nameof(rows), rows, "rows must be at least 1.");
        if (cols < 1)
            throw new ArgumentOutOfRangeException(nameof(cols), cols, "cols must be at least 1.");

        var cells = new GridRect[rows * cols];
        for (var r = 0; r < rows; r++)
        {
            var top = bounds.Y + bounds.Height * r / rows;
            var bottom = bounds.Y + bounds.Height * (r + 1) / rows;
            for (var c = 0; c < cols; c++)
            {
                var left = bounds.X + bounds.Width * c / cols;
                var right = bounds.X + bounds.Width * (c + 1) / cols;
                cells[r * cols + c] = new GridRect(left, top, right - left, bottom - top);
            }
        }

        return cells;
    }
}
