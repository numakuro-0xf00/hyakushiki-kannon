namespace hyakushiki_kannon.Core;

/// <summary>
/// Tunable parameters for grid mode: how the screen is divided, which keys select cells, and
/// how far an arrow-key nudge moves the cursor. Validated on construction so a
/// <see cref="GridSession"/> can trust its settings.
/// </summary>
public sealed record GridConfig
{
    /// <summary>
    /// Default home-row layout for a 3x3 grid: the nine keys <c>a s d f g h j k l</c> map to
    /// the nine cells in reading order, keeping the user's hands on the home row (per concept.md).
    /// </summary>
    public const string DefaultCellKeys = "asdfghjkl";

    /// <summary>
    /// Default monitor-selection keys (<c>q w e r t y u i o p</c>): the top keyboard row, kept
    /// disjoint from the home-row cell keys so the first keystroke can pick a monitor (per
    /// concept.md / issue #2). Only the first <c>N</c> keys are used for <c>N</c> monitors.
    /// </summary>
    public const string DefaultMonitorKeys = "qwertyuiop";

    public GridConfig(
        int rows = 3,
        int cols = 3,
        string cellKeys = DefaultCellKeys,
        double nudgeStep = 8.0,
        string monitorKeys = DefaultMonitorKeys)
    {
        if (rows < 1)
            throw new ArgumentOutOfRangeException(nameof(rows), rows, "rows must be at least 1.");
        if (cols < 1)
            throw new ArgumentOutOfRangeException(nameof(cols), cols, "cols must be at least 1.");
        if (nudgeStep <= 0)
            throw new ArgumentOutOfRangeException(nameof(nudgeStep), nudgeStep, "nudgeStep must be positive.");
        ArgumentException.ThrowIfNullOrEmpty(monitorKeys);

        // Building the maps here validates that each layout covers its keys with distinct,
        // non-whitespace characters, turning a misconfiguration into an immediate, clear failure.
        KeyMap = new CellKeyMap(rows * cols, cellKeys);
        MonitorKeyMap = new CellKeyMap(monitorKeys.Length, monitorKeys);

        // Cell keys and monitor keys must be disjoint, otherwise the first keystroke would be
        // ambiguous (monitor pick vs. cell drill). Only the keys actually in use are compared.
        for (var i = 0; i < rows * cols; i++)
        {
            if (MonitorKeyMap.TryGetCell(KeyMap.GetKey(i), out _))
                throw new ArgumentException(
                    $"Cell key '{KeyMap.GetKey(i)}' also appears in the monitor-selection keys; the two sets must be disjoint.",
                    nameof(monitorKeys));
        }

        Rows = rows;
        Cols = cols;
        CellKeys = cellKeys;
        NudgeStep = nudgeStep;
        MonitorKeys = monitorKeys;
    }

    /// <summary>Number of grid rows per subdivision step.</summary>
    public int Rows { get; }

    /// <summary>Number of grid columns per subdivision step.</summary>
    public int Cols { get; }

    /// <summary>The key layout used to select cells (one key per cell, row-major).</summary>
    public string CellKeys { get; }

    /// <summary>Cursor movement, in pixels, for a single arrow-key nudge.</summary>
    public double NudgeStep { get; }

    /// <summary>
    /// The key layout used to pick a monitor with the first keystroke; the <c>i</c>-th key selects
    /// the <c>i</c>-th monitor. Disjoint from <see cref="CellKeys"/>.
    /// </summary>
    public string MonitorKeys { get; }

    /// <summary>Cells per subdivision step (<see cref="Rows"/> * <see cref="Cols"/>).</summary>
    public int CellCount => Rows * Cols;

    /// <summary>The key/cell mapping derived from <see cref="CellKeys"/>.</summary>
    public CellKeyMap KeyMap { get; }

    /// <summary>The key/monitor mapping derived from <see cref="MonitorKeys"/> (key -> monitor index).</summary>
    public CellKeyMap MonitorKeyMap { get; }

    /// <summary>A ready-to-use 3x3 home-row configuration.</summary>
    public static GridConfig Default { get; } = new();

    // Equality is defined over the configuration inputs only. The synthesized record equality
    // would otherwise include the KeyMaps, which have reference identity (no value Equals), so two
    // configs built from identical settings would compare unequal. The maps are fully derived from
    // the key strings, so excluding them is safe.
    public bool Equals(GridConfig? other) =>
        other is not null
        && Rows == other.Rows
        && Cols == other.Cols
        && CellKeys == other.CellKeys
        && NudgeStep.Equals(other.NudgeStep)
        && MonitorKeys == other.MonitorKeys;

    public override int GetHashCode() => HashCode.Combine(Rows, Cols, CellKeys, NudgeStep, MonitorKeys);
}
