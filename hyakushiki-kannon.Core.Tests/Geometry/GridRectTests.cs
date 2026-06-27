using hyakushiki_kannon.Core.Geometry;
using Xunit;

namespace hyakushiki_kannon.Core.Tests.Geometry;

public class GridRectTests
{
    [Fact]
    public void Center_IsTheMidpoint()
    {
        var rect = new GridRect(10, 20, 100, 40);

        Assert.Equal(new GridPoint(60, 40), rect.Center);
    }

    [Fact]
    public void Edges_AreDerivedFromOriginAndSize()
    {
        var rect = new GridRect(10, 20, 100, 40);

        Assert.Equal(10, rect.Left);
        Assert.Equal(20, rect.Top);
        Assert.Equal(110, rect.Right);
        Assert.Equal(60, rect.Bottom);
    }

    [Theory]
    [InlineData(10, 20, true)]   // top-left corner (inclusive)
    [InlineData(110, 60, true)]  // bottom-right corner (inclusive)
    [InlineData(60, 40, true)]   // centre
    [InlineData(9, 40, false)]   // just left
    [InlineData(60, 61, false)]  // just below
    public void Contains_RespectsInclusiveEdges(double x, double y, bool expected)
    {
        var rect = new GridRect(10, 20, 100, 40);

        Assert.Equal(expected, rect.Contains(new GridPoint(x, y)));
    }

    [Fact]
    public void Clamp_PullsOutsidePointsToTheNearestEdge()
    {
        var rect = new GridRect(0, 0, 100, 50);

        Assert.Equal(new GridPoint(0, 0), rect.Clamp(new GridPoint(-30, -10)));
        Assert.Equal(new GridPoint(100, 50), rect.Clamp(new GridPoint(200, 80)));
        Assert.Equal(new GridPoint(40, 25), rect.Clamp(new GridPoint(40, 25)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.001)]
    public void Constructor_RejectsNegativeWidth(double width)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GridRect(0, 0, width, 10));
    }

    [Fact]
    public void Constructor_RejectsNegativeHeight()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GridRect(0, 0, 10, -1));
    }
}
