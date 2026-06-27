using hyakushiki_kannon.Core.Geometry;

namespace hyakushiki_kannon.Core.Input;

/// <summary>
/// Abstraction over the system mouse cursor and buttons. The real implementation wraps the
/// Win32 <c>GetCursorPos</c>/<c>SetCursorPos</c>/<c>SendInput</c> calls; tests substitute a
/// fake so the grid-mode logic can be verified without a desktop.
/// </summary>
public interface IPointerDevice
{
    /// <summary>The current cursor position in virtual-screen pixels.</summary>
    GridPoint Position { get; }

    /// <summary>Moves the cursor to <paramref name="point"/>.</summary>
    void MoveTo(GridPoint point);

    /// <summary>Presses and releases <paramref name="button"/> at the current position.</summary>
    void Click(MouseButton button);

    /// <summary>Performs a double click with <paramref name="button"/> at the current position.</summary>
    void DoubleClick(MouseButton button);
}
