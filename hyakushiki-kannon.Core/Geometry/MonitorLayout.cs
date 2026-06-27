namespace hyakushiki_kannon.Core.Geometry;

/// <summary>
/// The set of monitors available when grid mode starts, together with which one currently has
/// focus. Grid mode opens on <see cref="FocusedMonitor"/>; the first keystroke may instead pick
/// another monitor by index (see <see cref="GridSession"/>).
/// </summary>
public sealed class MonitorLayout
{
    /// <param name="monitors">Each monitor's bounds, in a stable order (index 0..n-1).</param>
    /// <param name="focusedIndex">Index of the monitor that currently has focus.</param>
    public MonitorLayout(IReadOnlyList<GridRect> monitors, int focusedIndex)
    {
        ArgumentNullException.ThrowIfNull(monitors);
        if (monitors.Count == 0)
            throw new ArgumentException("At least one monitor is required.", nameof(monitors));
        if (focusedIndex < 0 || focusedIndex >= monitors.Count)
            throw new ArgumentOutOfRangeException(
                nameof(focusedIndex), focusedIndex, "Focused index is outside the monitor list.");

        Monitors = monitors;
        FocusedIndex = focusedIndex;
    }

    /// <summary>Each monitor's bounds, in a stable order; index <c>i</c> is the <c>i</c>-th selectable monitor.</summary>
    public IReadOnlyList<GridRect> Monitors { get; }

    /// <summary>Index (into <see cref="Monitors"/>) of the monitor with focus when grid mode started.</summary>
    public int FocusedIndex { get; }

    /// <summary>The monitor grid mode opens on by default.</summary>
    public GridRect FocusedMonitor => Monitors[FocusedIndex];

    /// <summary>Number of monitors.</summary>
    public int Count => Monitors.Count;
}
