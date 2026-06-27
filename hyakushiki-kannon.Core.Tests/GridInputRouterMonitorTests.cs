using hyakushiki_kannon.Core;
using hyakushiki_kannon.Core.Geometry;
using hyakushiki_kannon.Core.Input;
using hyakushiki_kannon.Core.Tests.Fakes;
using Xunit;

namespace hyakushiki_kannon.Core.Tests;

public class GridInputRouterMonitorTests
{
    private static readonly GridRect Monitor0 = new(0, 0, 1920, 1080);
    private static readonly GridRect Monitor1 = new(1920, 0, 1920, 1080);

    private static MonitorLayout Layout(int focusedIndex) =>
        new(new[] { Monitor0, Monitor1 }, focusedIndex);

    private static (GridInputRouter router, GridSession session, FakePointerDevice pointer) NewRouter()
    {
        var pointer = new FakePointerDevice();
        var session = new GridSession(pointer, GridConfig.Default);
        return (new GridInputRouter(session), session, pointer);
    }

    [Fact]
    public void FirstKey_MonitorKey_SelectsMonitor()
    {
        var (router, session, pointer) = NewRouter();
        session.Activate(Layout(focusedIndex: 0));

        var consumed = router.HandleChar('w'); // monitor index 1

        Assert.True(consumed);
        Assert.Equal(1, session.Depth);
        Assert.Equal(Monitor1, session.CurrentBounds);
        Assert.Empty(pointer.Moves);
    }

    [Fact]
    public void FirstKey_CellKey_DrillsOnFocusedMonitor()
    {
        var (router, session, pointer) = NewRouter();
        session.Activate(Layout(focusedIndex: 1));

        var consumed = router.HandleChar('a'); // cell key => drill on focused monitor (Monitor1)

        Assert.True(consumed);
        Assert.Equal(2, session.Depth);
        var expected = new GridRect(1920, 0, 640, 360);
        Assert.Equal(expected.Center, pointer.Position);
    }

    [Fact]
    public void MonitorThenCell_DrillsOnSelectedMonitor()
    {
        var (router, session, pointer) = NewRouter();
        session.Activate(Layout(focusedIndex: 0));

        router.HandleChar('w'); // pick Monitor1
        router.HandleChar('a'); // drill its top-left cell

        var expected = new GridRect(1920, 0, 640, 360);
        Assert.Equal(expected.Center, pointer.Position);
    }

    [Fact]
    public void MonitorKeyAfterDrill_IsNotConsumed()
    {
        var (router, session, _) = NewRouter();
        session.Activate(Layout(focusedIndex: 0));
        router.HandleChar('a'); // first drill => monitor selection no longer available

        // 'w' is now just an unbound cell key (not in "asdfghjkl").
        Assert.False(router.HandleChar('w'));
    }
}
