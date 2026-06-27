using hyakushiki_kannon.Core.Geometry;
using Xunit;

namespace hyakushiki_kannon.Core.Tests.Geometry;

public class GridTests
{
    [Fact]
    public void Subdivide_ProducesRowsTimesColsCells()
    {
        var cells = Grid.Subdivide(new GridRect(0, 0, 90, 90), 3, 3);

        Assert.Equal(9, cells.Count);
    }

    [Fact]
    public void Subdivide_NumbersCellsInRowMajorOrder()
    {
        // 3x3 over a 90x90 square at origin => 30x30 cells.
        var cells = Grid.Subdivide(new GridRect(0, 0, 90, 90), 3, 3);

        // index 0 = top-left
        Assert.Equal(new GridRect(0, 0, 30, 30), cells[0]);
        // index 1 = same row, next column
        Assert.Equal(new GridRect(30, 0, 30, 30), cells[1]);
        // index 3 = next row, first column
        Assert.Equal(new GridRect(0, 30, 30, 30), cells[3]);
        // index 8 = bottom-right
        Assert.Equal(new GridRect(60, 60, 30, 30), cells[8]);
    }

    [Fact]
    public void Subdivide_HonoursTheBoundsOriginAndOffset()
    {
        var cells = Grid.Subdivide(new GridRect(100, 200, 40, 20), 2, 2);

        Assert.Equal(new GridRect(100, 200, 20, 10), cells[0]);
        Assert.Equal(new GridRect(120, 200, 20, 10), cells[1]);
        Assert.Equal(new GridRect(100, 210, 20, 10), cells[2]);
        Assert.Equal(new GridRect(120, 210, 20, 10), cells[3]);
    }

    [Fact]
    public void Subdivide_TilesExactlyWhenSizeDoesNotDivideEvenly()
    {
        // 100 / 3 is not an integer; cells must still cover [0,100] with no gap or overlap.
        var cells = Grid.Subdivide(new GridRect(0, 0, 100, 100), 3, 3);

        // The right edge of the last column equals the bounds' right edge exactly.
        Assert.Equal(100, cells[2].Right, precision: 10);
        // Adjacent cells share an edge: cell 0's right == cell 1's left.
        Assert.Equal(cells[0].Right, cells[1].Left, precision: 10);
        // Last row's bottom equals the bounds' bottom.
        Assert.Equal(100, cells[8].Bottom, precision: 10);
    }

    [Fact]
    public void Subdivide_SupportsNonSquareGrids()
    {
        var cells = Grid.Subdivide(new GridRect(0, 0, 80, 30), 1, 4);

        Assert.Equal(4, cells.Count);
        Assert.Equal(new GridRect(0, 0, 20, 30), cells[0]);
        Assert.Equal(new GridRect(60, 0, 20, 30), cells[3]);
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    [InlineData(-1, 3)]
    public void Subdivide_RejectsNonPositiveDimensions(int rows, int cols)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Grid.Subdivide(new GridRect(0, 0, 10, 10), rows, cols));
    }
}
