using hyakushiki_kannon.Core;
using hyakushiki_kannon.Core.Geometry;
using hyakushiki_kannon.Core.Input;
using hyakushiki_kannon.Core.Tests.Fakes;
using Xunit;

namespace hyakushiki_kannon.Core.Tests;

public class GridInputRouterTests
{
    private static readonly GridRect Screen = new(0, 0, 1920, 1080);

    private static (GridInputRouter router, GridSession session, FakePointerDevice pointer) NewRouter()
    {
        var pointer = new FakePointerDevice();
        var session = new GridSession(pointer, GridConfig.Default);
        var router = new GridInputRouter(session, GridKeyBindings.Default);
        return (router, session, pointer);
    }

    [Fact]
    public void KeysAreNotConsumedWhileInactive()
    {
        var (router, _, _) = NewRouter();

        Assert.False(router.HandleChar('a'));
        Assert.False(router.HandleSpecial(SpecialKey.Enter));
        Assert.False(router.HandleSpecial(SpecialKey.ArrowUp));
    }

    [Fact]
    public void CellKeyDrillsWhileSelecting()
    {
        var (router, session, pointer) = NewRouter();
        session.Activate(Screen);

        var consumed = router.HandleChar('g');

        Assert.True(consumed);
        Assert.Equal(2, session.Depth);
        Assert.Equal(new GridPoint(960, 540), pointer.Position);
    }

    [Fact]
    public void UnboundCellKeyIsNotConsumed()
    {
        var (router, session, _) = NewRouter();
        session.Activate(Screen);

        Assert.False(router.HandleChar('z'));
    }

    [Fact]
    public void EnterConfirmsWhileSelectingThenLeftClicksWhileActing()
    {
        var (router, session, pointer) = NewRouter();
        session.Activate(Screen);
        session.Drill('g');

        // First Enter: confirm -> Acting.
        Assert.True(router.HandleSpecial(SpecialKey.Enter));
        Assert.Equal(GridSessionState.Acting, session.State);

        // Second Enter: left click -> session ends.
        Assert.True(router.HandleSpecial(SpecialKey.Enter));
        Assert.Equal(new[] { MouseButton.Left }, pointer.Clicks);
        Assert.False(session.IsActive);
    }

    [Fact]
    public void SpaceBehavesLikeEnter()
    {
        var (router, session, pointer) = NewRouter();
        session.Activate(Screen);
        session.Drill('g');

        Assert.True(router.HandleSpecial(SpecialKey.Space));
        Assert.Equal(GridSessionState.Acting, session.State);

        Assert.True(router.HandleSpecial(SpecialKey.Space));
        Assert.Equal(new[] { MouseButton.Left }, pointer.Clicks);
    }

    [Fact]
    public void EscapeCancelsAndIsConsumed()
    {
        var (router, session, _) = NewRouter();
        session.Activate(Screen);
        session.Drill('g');

        Assert.True(router.HandleSpecial(SpecialKey.Escape));
        Assert.False(session.IsActive);
    }

    [Fact]
    public void BackspaceUndoesADrill()
    {
        var (router, session, _) = NewRouter();
        session.Activate(Screen);
        session.Drill('a');
        session.Drill('g'); // depth 3

        Assert.True(router.HandleSpecial(SpecialKey.Backspace));
        Assert.Equal(2, session.Depth);
    }

    [Fact]
    public void RightClickKeyRightClicksWhileActing()
    {
        var (router, session, pointer) = NewRouter();
        session.Activate(Screen);
        session.Drill('g');
        session.Confirm();

        var consumed = router.HandleChar('r');

        Assert.True(consumed);
        Assert.Equal(new[] { MouseButton.Right }, pointer.Clicks);
        Assert.False(session.IsActive);
    }

    [Fact]
    public void DoubleClickKeyDoubleClicksWhileActing()
    {
        var (router, session, pointer) = NewRouter();
        session.Activate(Screen);
        session.Drill('g');
        session.Confirm();

        var consumed = router.HandleChar('d');

        Assert.True(consumed);
        Assert.Equal(new[] { MouseButton.Left }, pointer.DoubleClicks);
        Assert.False(session.IsActive);
    }

    [Fact]
    public void UnboundActingKeyIsNotConsumed()
    {
        var (router, session, _) = NewRouter();
        session.Activate(Screen);
        session.Drill('g');
        session.Confirm();

        // 'q' is neither right-click nor double-click; in Acting, cell keys do nothing.
        Assert.False(router.HandleChar('q'));
        Assert.True(session.IsActive);
    }

    [Theory]
    [InlineData(SpecialKey.ArrowUp, 960, 535)]
    [InlineData(SpecialKey.ArrowDown, 960, 545)]
    [InlineData(SpecialKey.ArrowLeft, 955, 540)]
    [InlineData(SpecialKey.ArrowRight, 965, 540)]
    public void ArrowKeysNudge(SpecialKey key, double expectedX, double expectedY)
    {
        var pointer = new FakePointerDevice();
        var session = new GridSession(pointer, new GridConfig(nudgeStep: 5));
        var router = new GridInputRouter(session);
        session.Activate(Screen);
        session.Drill('g'); // (960, 540)

        Assert.True(router.HandleSpecial(key));
        Assert.Equal(new GridPoint(expectedX, expectedY), pointer.Position);
    }

    [Fact]
    public void FullDrillNudgeClickFlow()
    {
        var (router, session, pointer) = NewRouter();

        session.Activate(Screen);
        Assert.True(router.HandleChar('a'));          // drill top-left
        Assert.True(router.HandleChar('l'));          // drill bottom-right of that
        Assert.True(router.HandleSpecial(SpecialKey.Enter)); // confirm -> Acting
        Assert.True(router.HandleSpecial(SpecialKey.ArrowDown)); // nudge
        Assert.True(router.HandleChar('r'));          // right click

        Assert.Equal(new[] { MouseButton.Right }, pointer.Clicks);
        Assert.False(session.IsActive);
    }

    [Fact]
    public void Constructor_RejectsNullSession()
    {
        Assert.Throws<ArgumentNullException>(() => new GridInputRouter(null!));
    }
}
