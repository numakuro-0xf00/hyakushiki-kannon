using hyakushiki_kannon.Core.Geometry;
using hyakushiki_kannon.Core.Input;

namespace hyakushiki_kannon.Core.Tests.Fakes;

/// <summary>
/// An in-memory <see cref="IPointerDevice"/> that records every command, so tests can assert
/// what the session asked the cursor/buttons to do without touching a real desktop.
/// </summary>
public sealed class FakePointerDevice : IPointerDevice
{
    public GridPoint Position { get; private set; }

    /// <summary>Every position the cursor was moved to, in order.</summary>
    public List<GridPoint> Moves { get; } = new();

    /// <summary>Single clicks performed, in order.</summary>
    public List<MouseButton> Clicks { get; } = new();

    /// <summary>Double clicks performed, in order.</summary>
    public List<MouseButton> DoubleClicks { get; } = new();

    public void MoveTo(GridPoint point)
    {
        Position = point;
        Moves.Add(point);
    }

    public void Click(MouseButton button) => Clicks.Add(button);

    public void DoubleClick(MouseButton button) => DoubleClicks.Add(button);
}
