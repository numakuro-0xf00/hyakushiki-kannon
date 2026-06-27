using hyakushiki_kannon.Core;
using hyakushiki_kannon.Core.Geometry;
using hyakushiki_kannon.Core.Input;
using hyakushiki_kannon.Core.Tests.Fakes;
using Xunit;

namespace hyakushiki_kannon.Core.Tests;

public class GridSessionTests
{
    private static readonly GridRect Screen = new(0, 0, 1920, 1080);

    private static (GridSession session, FakePointerDevice pointer) NewSession(GridConfig? config = null)
    {
        var pointer = new FakePointerDevice();
        var session = new GridSession(pointer, config ?? GridConfig.Default);
        return (session, pointer);
    }

    [Fact]
    public void StartsInactive()
    {
        var (session, _) = NewSession();

        Assert.Equal(GridSessionState.Inactive, session.State);
        Assert.False(session.IsActive);
        Assert.Equal(0, session.Depth);
    }

    [Fact]
    public void Activate_EntersSelectingAtDepthOneWithoutMovingCursor()
    {
        var (session, pointer) = NewSession();

        session.Activate(Screen);

        Assert.Equal(GridSessionState.Selecting, session.State);
        Assert.True(session.IsActive);
        Assert.Equal(1, session.Depth);
        Assert.Equal(Screen, session.CurrentBounds);
        // Cancelling immediately should be harmless: the cursor was never touched.
        Assert.Empty(pointer.Moves);
    }

    [Fact]
    public void Drill_MovesCursorToSelectedCellCenterAndZoomsIn()
    {
        var (session, pointer) = NewSession();
        session.Activate(Screen);

        // 'a' is cell 0 (top-left) of the 3x3 grid over 1920x1080 => 640x360 cell.
        var handled = session.Drill('a');

        Assert.True(handled);
        Assert.Equal(2, session.Depth);
        Assert.Equal(new GridRect(0, 0, 640, 360), session.CurrentBounds);
        Assert.Equal(new GridPoint(320, 180), pointer.Position);
    }

    [Fact]
    public void Drill_CenterCellLandsOnScreenCenter()
    {
        var (session, pointer) = NewSession();
        session.Activate(Screen);

        // 'g' is the middle key (index 4) => centre cell, centre of screen.
        session.Drill('g');

        Assert.Equal(new GridPoint(960, 540), pointer.Position);
    }

    [Fact]
    public void Drill_IsRecursiveAndHomesInOnTheTarget()
    {
        var (session, pointer) = NewSession();
        session.Activate(Screen);

        session.Drill('a'); // top-left 640x360 at (0,0)
        session.Drill('a'); // top-left of that: 213.33x120 at (0,0)

        Assert.Equal(3, session.Depth);
        var bounds = session.CurrentBounds;
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(1920.0 / 9, bounds.Width, precision: 6);
        Assert.Equal(1080.0 / 9, bounds.Height, precision: 6);
        Assert.Equal(bounds.Center, pointer.Position);
    }

    [Fact]
    public void Drill_WithUnboundKeyIsIgnored()
    {
        var (session, pointer) = NewSession();
        session.Activate(Screen);

        var handled = session.Drill('z'); // not in "asdfghjkl"

        Assert.False(handled);
        Assert.Equal(1, session.Depth);
        Assert.Empty(pointer.Moves);
    }

    [Fact]
    public void Drill_ThrowsWhenNotSelecting()
    {
        var (session, _) = NewSession();

        Assert.Throws<InvalidOperationException>(() => session.Drill('a'));
    }

    [Fact]
    public void Back_PopsOneDrillLevelAndRecentersCursor()
    {
        var (session, pointer) = NewSession();
        session.Activate(Screen);
        session.Drill('a'); // depth 2
        session.Drill('g'); // depth 3
        pointer.Moves.Clear();

        var handled = session.Back();

        Assert.True(handled);
        Assert.Equal(2, session.Depth);
        // Cursor recentres on the parent cell (top-left 640x360).
        Assert.Equal(new GridPoint(320, 180), pointer.Position);
    }

    [Fact]
    public void Back_AtRootCancelsTheSession()
    {
        var (session, _) = NewSession();
        session.Activate(Screen);

        var handled = session.Back();

        Assert.True(handled);
        Assert.Equal(GridSessionState.Inactive, session.State);
        Assert.Equal(0, session.Depth);
    }

    [Fact]
    public void Confirm_SwitchesToActing()
    {
        var (session, _) = NewSession();
        session.Activate(Screen);
        session.Drill('g');

        session.Confirm();

        Assert.Equal(GridSessionState.Acting, session.State);
    }

    [Fact]
    public void Back_FromActingReturnsToSelecting()
    {
        var (session, _) = NewSession();
        session.Activate(Screen);
        session.Drill('g');
        session.Confirm();

        var handled = session.Back();

        Assert.True(handled);
        Assert.Equal(GridSessionState.Selecting, session.State);
    }

    [Fact]
    public void Nudge_MovesCursorByConfiguredStep()
    {
        var (session, pointer) = NewSession(new GridConfig(nudgeStep: 10));
        session.Activate(Screen);
        session.Drill('g'); // cursor at (960, 540)

        session.Nudge(NudgeDirection.Up);
        Assert.Equal(new GridPoint(960, 530), pointer.Position);

        session.Nudge(NudgeDirection.Down);
        Assert.Equal(new GridPoint(960, 540), pointer.Position);

        session.Nudge(NudgeDirection.Left);
        Assert.Equal(new GridPoint(950, 540), pointer.Position);

        session.Nudge(NudgeDirection.Right);
        Assert.Equal(new GridPoint(960, 540), pointer.Position);
    }

    [Fact]
    public void Nudge_WorksWhileActing()
    {
        var (session, pointer) = NewSession(new GridConfig(nudgeStep: 5));
        session.Activate(Screen);
        session.Drill('g');
        session.Confirm();

        session.Nudge(NudgeDirection.Right);

        Assert.Equal(new GridPoint(965, 540), pointer.Position);
    }

    [Fact]
    public void Nudge_ThrowsWhenInactive()
    {
        var (session, _) = NewSession();

        Assert.Throws<InvalidOperationException>(() => session.Nudge(NudgeDirection.Up));
    }

    [Theory]
    [InlineData(MouseButton.Left)]
    [InlineData(MouseButton.Right)]
    public void Click_PerformsClickAndEndsSession(MouseButton button)
    {
        var (session, pointer) = NewSession();
        session.Activate(Screen);
        session.Drill('g');
        session.Confirm();

        session.Click(button);

        Assert.Equal(new[] { button }, pointer.Clicks);
        Assert.Equal(GridSessionState.Inactive, session.State);
        Assert.Equal(0, session.Depth);
    }

    [Fact]
    public void DoubleClick_PerformsDoubleClickAndEndsSession()
    {
        var (session, pointer) = NewSession();
        session.Activate(Screen);
        session.Drill('g');
        session.Confirm();

        session.DoubleClick(MouseButton.Left);

        Assert.Equal(new[] { MouseButton.Left }, pointer.DoubleClicks);
        Assert.False(session.IsActive);
    }

    [Fact]
    public void Click_ClicksAtTheCellCenterThatWasSelected()
    {
        var (session, pointer) = NewSession();
        session.Activate(Screen);
        session.Drill('a'); // cursor moved to (320, 180)
        session.Confirm();

        var positionAtClick = pointer.Position;
        session.Click(MouseButton.Left);

        // The cursor was already placed before clicking; the click happens there.
        Assert.Equal(new GridPoint(320, 180), positionAtClick);
        Assert.Single(pointer.Clicks);
    }

    [Fact]
    public void Click_ThrowsWhenInactive()
    {
        var (session, _) = NewSession();

        Assert.Throws<InvalidOperationException>(() => session.Click(MouseButton.Left));
    }

    [Fact]
    public void Cancel_EndsSessionWithoutClicking()
    {
        var (session, pointer) = NewSession();
        session.Activate(Screen);
        session.Drill('g');

        session.Cancel();

        Assert.Equal(GridSessionState.Inactive, session.State);
        Assert.Equal(0, session.Depth);
        Assert.Empty(pointer.Clicks);
    }

    [Fact]
    public void Cancel_WhenAlreadyInactiveIsHarmless()
    {
        var (session, _) = NewSession();

        session.Cancel(); // should not throw

        Assert.False(session.IsActive);
    }

    [Fact]
    public void CurrentBounds_ThrowsWhenInactive()
    {
        var (session, _) = NewSession();

        Assert.Throws<InvalidOperationException>(() => session.CurrentBounds);
    }

    [Fact]
    public void Activate_AfterAPreviousSessionResetsCleanly()
    {
        var (session, _) = NewSession();
        session.Activate(Screen);
        session.Drill('a');
        session.Drill('a'); // depth 3

        session.Activate(new GridRect(0, 0, 800, 600));

        Assert.Equal(1, session.Depth);
        Assert.Equal(new GridRect(0, 0, 800, 600), session.CurrentBounds);
    }

    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new GridSession(null!, GridConfig.Default));
        Assert.Throws<ArgumentNullException>(() => new GridSession(new FakePointerDevice(), null!));
    }
}
