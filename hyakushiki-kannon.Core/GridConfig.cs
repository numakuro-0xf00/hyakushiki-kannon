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

    public GridConfig(int rows = 3, int cols = 3, string cellKeys = DefaultCellKeys, double nudgeStep = 8.0)
    {
        if (rows < 1)
            throw new ArgumentOutOfRangeException(nameof(rows), rows, "rows must be at least 1.");
        if (cols < 1)
            throw new ArgumentOutOfRangeException(nameof(cols), cols, "cols must be at least 1.");
        if (nudgeStep <= 0)
            throw new ArgumentOutOfRangeException(nameof(nudgeStep), nudgeStep, "nudgeStep must be positive.");

        // Building the map here validates that cellKeys covers every cell with distinct keys,
        // turning a misconfiguration into an immediate, clear failure.
        KeyMap = new CellKeyMap(rows * cols, cellKeys);

        Rows = rows;
        Cols = cols;
        CellKeys = cellKeys;
        NudgeStep = nudgeStep;
    }

    /// <summary>Number of grid rows per subdivision step.</summary>
    public int Rows { get; }

    /// <summary>Number of grid columns per subdivision step.</summary>
    public int Cols { get; }

    /// <summary>The key layout used to select cells (one key per cell, row-major).</summary>
    public string CellKeys { get; }

    /// <summary>Cursor movement, in pixels, for a single arrow-key nudge.</summary>
    public double NudgeStep { get; }

    /// <summary>Cells per subdivision step (<see cref="Rows"/> * <see cref="Cols"/>).</summary>
    public int CellCount => Rows * Cols;

    /// <summary>The key/cell mapping derived from <see cref="CellKeys"/>.</summary>
    public CellKeyMap KeyMap { get; }

    /// <summary>A ready-to-use 3x3 home-row configuration.</summary>
    public static GridConfig Default { get; } = new();
}
