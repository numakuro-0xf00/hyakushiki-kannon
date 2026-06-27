using hyakushiki_kannon.Core;
using hyakushiki_kannon.Core.Geometry;
using hyakushiki_kannon.Core.Input;
using hyakushiki_kannon.Core.Tests.Fakes;
using Xunit;

namespace hyakushiki_kannon.Core.Tests;

public class GridSessionMonitorTests
{
    // Dimensions are multiples of 3 (the grid cols/rows) so cell maths is exact under the
    // record-struct's exact double equality.
    private static readonly GridRect Monitor0 = new(0, 0, 1920, 1080);
    private static readonly GridRect Monitor1 = new(1920, 0, 1920, 1080);
    private static readonly GridRect Monitor2 = new(-1500, 0, 1500, 900);

    private static MonitorLayout Layout(int focusedIndex = 0) =>
        new(new[] { Monitor0, Monitor1, Monitor2 }, focusedIndex);

    private static (GridSession session, FakePointerDevice pointer) NewSession()
    {
        var pointer = new FakePointerDevice();
        return (new GridSession(pointer, GridConfig.Default), pointer);
    }

    [Fact]
    public void Activate_WithLayout_OpensOnFocusedMonitor()
    {
        var (session, _) = NewSession();

        session.Activate(Layout(focusedIndex: 1));

        Assert.Equal(GridSessionState.Selecting, session.State);
        Assert.Equal(1, session.Depth);
        Assert.Equal(Monitor1, session.CurrentBounds);
        Assert.True(session.CanSelectMonitor);
    }

    [Fact]
    public void Activate_WithExplicitRect_DisablesMonitorSelection()
    {
        var (session, _) = NewSession();

        session.Activate(new GridRect(0, 0, 800, 600));

        Assert.False(session.CanSelectMonitor);
    }

    [Fact]
    public void SelectMonitor_RetargetsGridToChosenMonitorWithoutDrilling()
    {
        var (session, pointer) = NewSession();
        session.Activate(Layout(focusedIndex: 0));

        // 'w' is the 2nd monitor key (index 1).
        var handled = session.SelectMonitor('w');

        Assert.True(handled);
        Assert.Equal(1, session.Depth);              // still at the root, just re-scoped
        Assert.Equal(Monitor1, session.CurrentBounds);
        Assert.True(session.CanSelectMonitor);        // can still re-pick until first drill
        Assert.Empty(pointer.Moves);                  // re-scoping does not move the cursor
    }

    [Fact]
    public void SelectMonitor_ThenDrill_SelectsCellOnThatMonitor()
    {
        var (session, pointer) = NewSession();
        session.Activate(Layout(focusedIndex: 0));

        session.SelectMonitor('e'); // monitor index 2 => Monitor2 at (-1500, 0, 1500, 900)
        session.Drill('a');         // top-left cell of Monitor2

        Assert.False(session.CanSelectMonitor);
        Assert.Equal(2, session.Depth);
        var expectedCell = new GridRect(-1500, 0, 500, 300);
        Assert.Equal(expectedCell, session.CurrentBounds);
        Assert.Equal(expectedCell.Center, pointer.Position);
    }

    [Fact]
    public void Drill_FirstKey_StaysOnFocusedMonitor()
    {
        var (session, pointer) = NewSession();
        session.Activate(Layout(focusedIndex: 1));

        // First keystroke is a cell key, not a monitor key => drill on the focused monitor.
        session.Drill('a');

        var expectedCell = new GridRect(1920, 0, 640, 360);
        Assert.Equal(expectedCell, session.CurrentBounds);
        Assert.Equal(expectedCell.Center, pointer.Position);
    }

    [Fact]
    public void CanSelectMonitor_IsFalseAfterFirstDrill()
    {
        var (session, _) = NewSession();
        session.Activate(Layout());

        session.Drill('a');

        Assert.False(session.CanSelectMonitor);
    }

    [Fact]
    public void SelectMonitor_UnboundKeyIsIgnored()
    {
        var (session, _) = NewSession();
        session.Activate(Layout());

        // 'a' is a cell key, not a monitor key.
        Assert.False(session.SelectMonitor('a'));
        Assert.Equal(Monitor0, session.CurrentBounds);
    }

    [Fact]
    public void SelectMonitor_KeyForNonExistentMonitorIsIgnored()
    {
        var (session, _) = NewSession();
        session.Activate(Layout()); // 3 monitors => keys q, w, e valid; r is index 3 (none)

        Assert.False(session.SelectMonitor('r'));
        Assert.Equal(Monitor0, session.CurrentBounds);
    }

    [Fact]
    public void SelectMonitor_ThrowsWhenNotAvailable()
    {
        var (session, _) = NewSession();

        // Inactive.
        Assert.Throws<InvalidOperationException>(() => session.SelectMonitor('q'));

        // After a drill, monitor selection is no longer available.
        session.Activate(Layout());
        session.Drill('a');
        Assert.Throws<InvalidOperationException>(() => session.SelectMonitor('q'));
    }

    [Fact]
    public void Cancel_ClearsMonitorSelectionForNextActivation()
    {
        var (session, _) = NewSession();
        session.Activate(Layout());
        session.Cancel();

        session.Activate(new GridRect(0, 0, 800, 600));

        Assert.False(session.CanSelectMonitor);
    }
}
