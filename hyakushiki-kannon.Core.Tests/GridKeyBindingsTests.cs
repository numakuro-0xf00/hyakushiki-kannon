using hyakushiki_kannon.Core.Input;
using Xunit;

namespace hyakushiki_kannon.Core.Tests;

public class GridKeyBindingsTests
{
    [Fact]
    public void Default_UsesRForRightAndDForDouble()
    {
        var bindings = GridKeyBindings.Default;

        Assert.Equal('r', bindings.RightClick);
        Assert.Equal('d', bindings.DoubleClick);
    }

    [Fact]
    public void NormalizesKeysToLowercase()
    {
        var bindings = new GridKeyBindings(rightClick: 'R', doubleClick: 'X');

        Assert.Equal('r', bindings.RightClick);
        Assert.Equal('x', bindings.DoubleClick);
    }

    [Fact]
    public void RejectsCollidingClickKeys()
    {
        Assert.Throws<ArgumentException>(() => new GridKeyBindings(rightClick: 'r', doubleClick: 'R'));
    }
}
