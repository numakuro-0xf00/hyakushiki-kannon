using hyakushiki_kannon.Core;
using Xunit;

namespace hyakushiki_kannon.Core.Tests;

public class GridConfigTests
{
    [Fact]
    public void Default_IsThreeByThreeHomeRow()
    {
        var config = GridConfig.Default;

        Assert.Equal(3, config.Rows);
        Assert.Equal(3, config.Cols);
        Assert.Equal(9, config.CellCount);
        Assert.Equal("asdfghjkl", config.CellKeys);
        Assert.Equal(9, config.KeyMap.Count);
    }

    [Fact]
    public void SupportsLargerGridsWithEnoughKeys()
    {
        var config = new GridConfig(rows: 4, cols: 4, cellKeys: "asdfghjklqwertyu");

        Assert.Equal(16, config.CellCount);
        Assert.Equal(16, config.KeyMap.Count);
    }

    [Fact]
    public void Constructor_RejectsKeyLayoutTooSmallForGrid()
    {
        // 4x4 needs 16 keys but only 9 are supplied.
        Assert.Throws<ArgumentException>(() => new GridConfig(rows: 4, cols: 4, cellKeys: "asdfghjkl"));
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    public void Constructor_RejectsNonPositiveDimensions(int rows, int cols)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GridConfig(rows, cols));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public void Constructor_RejectsNonPositiveNudgeStep(double step)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GridConfig(nudgeStep: step));
    }

    [Fact]
    public void ConfigsBuiltFromIdenticalSettingsAreEqual()
    {
        // Regression: the derived (reference-typed) KeyMap must not break value equality.
        var a = new GridConfig();
        var b = new GridConfig();

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.Equal(GridConfig.Default, new GridConfig());
    }

    [Fact]
    public void ConfigsWithDifferentSettingsAreNotEqual()
    {
        Assert.NotEqual(new GridConfig(nudgeStep: 8), new GridConfig(nudgeStep: 12));
        Assert.NotEqual(
            new GridConfig(rows: 3, cols: 3),
            new GridConfig(rows: 4, cols: 4, cellKeys: "asdfghjklqwertyu"));
    }
}
