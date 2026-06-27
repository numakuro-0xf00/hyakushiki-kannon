namespace hyakushiki_kannon.Core;

/// <summary>
/// Bidirectional mapping between the keys the user presses and the grid cell indices they
/// select. Cells are numbered in row-major order (see <see cref="Geometry.Grid.Subdivide"/>),
/// so the <c>n</c>-th key in the layout selects the <c>n</c>-th cell. Matching is
/// case-insensitive.
/// </summary>
public sealed class CellKeyMap
{
    private readonly char[] _keyForCell;
    private readonly Dictionary<char, int> _cellForKey;

    /// <param name="cellCount">Number of cells in the grid (<c>rows * cols</c>).</param>
    /// <param name="keys">
    /// The key layout; the first <paramref name="cellCount"/> characters are used, one per
    /// cell in row-major order. Must contain enough distinct, non-whitespace characters.
    /// </param>
    public CellKeyMap(int cellCount, string keys)
    {
        if (cellCount < 1)
            throw new ArgumentOutOfRangeException(nameof(cellCount), cellCount, "cellCount must be at least 1.");
        ArgumentNullException.ThrowIfNull(keys);

        var normalized = keys.ToLowerInvariant();
        if (normalized.Length < cellCount)
            throw new ArgumentException(
                $"Key layout supplies {normalized.Length} keys but the grid needs {cellCount}.", nameof(keys));

        _keyForCell = new char[cellCount];
        _cellForKey = new Dictionary<char, int>(cellCount);
        for (var i = 0; i < cellCount; i++)
        {
            var key = normalized[i];
            if (char.IsWhiteSpace(key))
                throw new ArgumentException("Key layout must not contain whitespace.", nameof(keys));
            if (!_cellForKey.TryAdd(key, i))
                throw new ArgumentException($"Key layout contains a duplicate key '{key}'.", nameof(keys));
            _keyForCell[i] = key;
        }
    }

    /// <summary>Number of cells (and keys) this map covers.</summary>
    public int Count => _keyForCell.Length;

    /// <summary>
    /// Resolves the cell index a key selects. Returns <c>false</c> for keys that are not part
    /// of the layout, leaving <paramref name="cellIndex"/> set to <c>-1</c>.
    /// </summary>
    public bool TryGetCell(char key, out int cellIndex)
    {
        if (_cellForKey.TryGetValue(char.ToLowerInvariant(key), out cellIndex))
            return true;

        cellIndex = -1;
        return false;
    }

    /// <summary>The key that selects cell <paramref name="cellIndex"/> (used to draw labels).</summary>
    public char GetKey(int cellIndex)
    {
        if (cellIndex < 0 || cellIndex >= _keyForCell.Length)
            throw new ArgumentOutOfRangeException(nameof(cellIndex), cellIndex, "Cell index is outside the grid.");
        return _keyForCell[cellIndex];
    }
}
