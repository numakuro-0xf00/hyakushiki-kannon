using hyakushiki_kannon.Core;
using Xunit;

namespace hyakushiki_kannon.Core.Tests;

public class CellKeyMapTests
{
    [Fact]
    public void MapsKeysToCellsInOrder()
    {
        var map = new CellKeyMap(9, "asdfghjkl");

        Assert.True(map.TryGetCell('a', out var first));
        Assert.Equal(0, first);
        Assert.True(map.TryGetCell('l', out var last));
        Assert.Equal(8, last);
    }

    [Fact]
    public void IsCaseInsensitive()
    {
        var map = new CellKeyMap(9, "asdfghjkl");

        Assert.True(map.TryGetCell('A', out var index));
        Assert.Equal(0, index);
    }

    [Fact]
    public void GetKey_IsTheInverseOfTryGetCell()
    {
        var map = new CellKeyMap(9, "asdfghjkl");

        for (var i = 0; i < map.Count; i++)
        {
            var key = map.GetKey(i);
            Assert.True(map.TryGetCell(key, out var roundTrip));
            Assert.Equal(i, roundTrip);
        }
    }

    [Fact]
    public void TryGetCell_ReturnsFalseForUnboundKey()
    {
        var map = new CellKeyMap(9, "asdfghjkl");

        Assert.False(map.TryGetCell('z', out var index));
        Assert.Equal(-1, index);
    }

    [Fact]
    public void IgnoresExtraKeysBeyondTheCellCount()
    {
        // Layout has more keys than cells; only the first three are bound.
        var map = new CellKeyMap(3, "asdfgh");

        Assert.Equal(3, map.Count);
        Assert.True(map.TryGetCell('d', out _));
        Assert.False(map.TryGetCell('f', out _));
    }

    [Fact]
    public void Constructor_RejectsTooFewKeys()
    {
        Assert.Throws<ArgumentException>(() => new CellKeyMap(9, "asdf"));
    }

    [Fact]
    public void Constructor_RejectsDuplicateKeys()
    {
        Assert.Throws<ArgumentException>(() => new CellKeyMap(4, "aabc"));
    }

    [Fact]
    public void Constructor_RejectsDuplicatesDifferingOnlyByCase()
    {
        Assert.Throws<ArgumentException>(() => new CellKeyMap(2, "aA"));
    }

    [Fact]
    public void Constructor_RejectsWhitespaceKeys()
    {
        Assert.Throws<ArgumentException>(() => new CellKeyMap(3, "a b"));
    }

    [Fact]
    public void GetKey_RejectsOutOfRangeIndex()
    {
        var map = new CellKeyMap(9, "asdfghjkl");

        Assert.Throws<ArgumentOutOfRangeException>(() => map.GetKey(9));
        Assert.Throws<ArgumentOutOfRangeException>(() => map.GetKey(-1));
    }
}
