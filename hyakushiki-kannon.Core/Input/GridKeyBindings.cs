namespace hyakushiki_kannon.Core.Input;

/// <summary>
/// The character keys that trigger click actions while in the Acting phase. Cell-selection
/// keys live in <c>GridConfig</c>; left click is additionally bound to Enter/Space by the router.
/// Keys are matched case-insensitively.
/// </summary>
public sealed record GridKeyBindings
{
    public GridKeyBindings(char rightClick = 'r', char doubleClick = 'd')
    {
        RightClick = char.ToLowerInvariant(rightClick);
        DoubleClick = char.ToLowerInvariant(doubleClick);

        if (RightClick == DoubleClick)
            throw new ArgumentException("Right-click and double-click keys must differ.");
    }

    /// <summary>Key that performs a right click (default <c>r</c>).</summary>
    public char RightClick { get; }

    /// <summary>Key that performs a left double click (default <c>d</c>).</summary>
    public char DoubleClick { get; }

    /// <summary>The default Acting-phase click bindings.</summary>
    public static GridKeyBindings Default { get; } = new();
}
