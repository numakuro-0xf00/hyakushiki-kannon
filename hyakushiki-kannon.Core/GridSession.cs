using hyakushiki_kannon.Core.Geometry;
using hyakushiki_kannon.Core.Input;

namespace hyakushiki_kannon.Core;

/// <summary>
/// The grid-mode state machine that drives the GridMouse interaction described in concept.md.
/// It owns the drill-down path and translates high-level intents (drill into a cell, nudge,
/// click, cancel) into cursor commands on an injected <see cref="IPointerDevice"/>. It holds no
/// Win32 or UI state, so it is fully unit-testable.
///
/// <para>Flow: <see cref="Activate"/> shows the grid over a rectangle (state
/// <see cref="GridSessionState.Selecting"/>). Each <see cref="Drill"/> picks a cell, moves the
/// cursor to its centre and zooms the grid into that cell, repeatable to home in on a target.
/// <see cref="Confirm"/> switches to <see cref="GridSessionState.Acting"/> where
/// <see cref="Nudge"/> fine-tunes the position and <see cref="Click"/>/<see cref="DoubleClick"/>
/// act and then end the session. <see cref="Cancel"/> leaves grid mode at any time.</para>
/// </summary>
public sealed class GridSession
{
    private readonly IPointerDevice _pointer;
    private readonly GridConfig _config;

    // The drill-down path: the bottom is the full grid bounds, the top is the cell the user has
    // zoomed into so far. Kept as a stack so Back() can undo one level at a time.
    private readonly Stack<GridRect> _path = new();

    // The monitors available this session (null when activated with an explicit rectangle). While
    // set, the very first keystroke may pick a different monitor instead of drilling a cell.
    private MonitorLayout? _monitors;

    public GridSession(IPointerDevice pointer, GridConfig config)
    {
        _pointer = pointer ?? throw new ArgumentNullException(nameof(pointer));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>Current lifecycle state.</summary>
    public GridSessionState State { get; private set; } = GridSessionState.Inactive;

    /// <summary>True while the grid is shown (either selecting or acting).</summary>
    public bool IsActive => State != GridSessionState.Inactive;

    /// <summary>
    /// How many levels deep the drill-down is: 0 when inactive, 1 right after
    /// <see cref="Activate"/>, growing by one per <see cref="Drill"/>.
    /// </summary>
    public int Depth => _path.Count;

    /// <summary>The rectangle the grid currently covers (the latest zoomed-into cell).</summary>
    /// <exception cref="InvalidOperationException">Thrown when the session is inactive.</exception>
    public GridRect CurrentBounds =>
        _path.Count > 0 ? _path.Peek() : throw new InvalidOperationException("Grid mode is not active.");

    /// <summary>The configuration in effect for this session.</summary>
    public GridConfig Config => _config;

    /// <summary>
    /// True while the first keystroke can still pick a monitor: the session was activated with a
    /// <see cref="MonitorLayout"/> and no cell has been drilled yet. Once a cell is drilled (or the
    /// session enters acting), monitor keys no longer apply.
    /// </summary>
    public bool CanSelectMonitor =>
        State == GridSessionState.Selecting && _path.Count == 1 && _monitors is not null;

    /// <summary>
    /// Enters grid mode covering <paramref name="bounds"/> directly (single rectangle, no monitor
    /// selection). The cursor is not moved until the first <see cref="Drill"/>, so cancelling
    /// immediately is a no-op for the user.
    /// </summary>
    public void Activate(GridRect bounds)
    {
        _path.Clear();
        _monitors = null;
        _path.Push(bounds);
        State = GridSessionState.Selecting;
    }

    /// <summary>
    /// Enters grid mode on the focused monitor of <paramref name="layout"/>. Until the first cell
    /// is drilled, the first keystroke may instead pick another monitor via
    /// <see cref="SelectMonitor"/> (see <see cref="CanSelectMonitor"/>).
    /// </summary>
    public void Activate(MonitorLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);

        _path.Clear();
        _monitors = layout;
        _path.Push(layout.FocusedMonitor);
        State = GridSessionState.Selecting;
    }

    /// <summary>
    /// Re-targets the grid to the monitor bound to <paramref name="monitorKey"/> (first keystroke
    /// only). The drill depth stays at 1, so the next keystroke selects a cell on that monitor.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the key maps to an existing monitor; <c>false</c> for an unbound key or a key
    /// that addresses a monitor that does not exist (ignored).
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when monitor selection is not available.</exception>
    public bool SelectMonitor(char monitorKey)
    {
        if (!CanSelectMonitor)
            throw new InvalidOperationException("Monitor selection is not available right now.");

        if (!_config.MonitorKeyMap.TryGetCell(monitorKey, out var monitorIndex))
            return false;
        if (monitorIndex >= _monitors!.Count)
            return false;

        // Re-scope the grid root to the chosen monitor without advancing the drill depth. The
        // cursor is left where it is; the first cell drill on this monitor will move it.
        _path.Clear();
        _path.Push(_monitors.Monitors[monitorIndex]);
        return true;
    }

    /// <summary>
    /// Selects the cell bound to <paramref name="cellKey"/>, moves the cursor to its centre and
    /// zooms the grid into that cell for further drilling.
    /// </summary>
    /// <returns><c>true</c> if the key maps to a cell; <c>false</c> for an unbound key (ignored).</returns>
    /// <exception cref="InvalidOperationException">Thrown when not in <see cref="GridSessionState.Selecting"/>.</exception>
    public bool Drill(char cellKey)
    {
        RequireState(GridSessionState.Selecting);

        if (!_config.KeyMap.TryGetCell(cellKey, out var cellIndex))
            return false;

        var cells = Grid.Subdivide(CurrentBounds, _config.Rows, _config.Cols);
        var cell = cells[cellIndex];
        _path.Push(cell);
        _pointer.MoveTo(cell.Center);
        return true;
    }

    /// <summary>
    /// Undoes the most recent step. While selecting, pops one drill level (moving the cursor
    /// back to the parent cell's centre) and cancels the session if that was the last level.
    /// While acting, returns to selecting so the user can keep drilling.
    /// </summary>
    /// <returns><c>true</c> if a step was undone, <c>false</c> if there was nothing to undo.</returns>
    public bool Back()
    {
        switch (State)
        {
            case GridSessionState.Acting:
                State = GridSessionState.Selecting;
                return true;

            case GridSessionState.Selecting when _path.Count > 1:
                _path.Pop();
                _pointer.MoveTo(CurrentBounds.Center);
                return true;

            case GridSessionState.Selecting:
                // Only the root bounds remain - backing out cancels grid mode.
                Cancel();
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Locks in the current target and switches to <see cref="GridSessionState.Acting"/> so the
    /// user can nudge and click. No-op if already acting.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the session is inactive.</exception>
    public void Confirm()
    {
        RequireActive();
        State = GridSessionState.Acting;
    }

    /// <summary>
    /// Fine-tunes the cursor by one <see cref="GridConfig.NudgeStep"/> in
    /// <paramref name="direction"/>. Allowed while selecting or acting.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the session is inactive.</exception>
    public void Nudge(NudgeDirection direction)
    {
        RequireActive();

        var step = _config.NudgeStep;
        var p = _pointer.Position;
        var moved = direction switch
        {
            NudgeDirection.Up => p with { Y = p.Y - step },
            NudgeDirection.Down => p with { Y = p.Y + step },
            NudgeDirection.Left => p with { X = p.X - step },
            NudgeDirection.Right => p with { X = p.X + step },
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
        _pointer.MoveTo(moved);
    }

    /// <summary>Clicks <paramref name="button"/> at the current cursor position, then ends grid mode.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the session is inactive.</exception>
    public void Click(MouseButton button)
    {
        RequireActive();
        _pointer.Click(button);
        Cancel();
    }

    /// <summary>Double-clicks <paramref name="button"/> at the current position, then ends grid mode.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the session is inactive.</exception>
    public void DoubleClick(MouseButton button)
    {
        RequireActive();
        _pointer.DoubleClick(button);
        Cancel();
    }

    /// <summary>Leaves grid mode without acting. Safe to call when already inactive.</summary>
    public void Cancel()
    {
        _path.Clear();
        _monitors = null;
        State = GridSessionState.Inactive;
    }

    private void RequireActive()
    {
        if (!IsActive)
            throw new InvalidOperationException("Grid mode is not active.");
    }

    private void RequireState(GridSessionState expected)
    {
        if (State != expected)
            throw new InvalidOperationException(
                $"Operation requires state {expected} but the session is {State}.");
    }
}
