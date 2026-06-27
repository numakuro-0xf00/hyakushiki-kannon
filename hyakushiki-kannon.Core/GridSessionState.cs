namespace hyakushiki_kannon.Core;

/// <summary>Lifecycle of a grid-mode interaction.</summary>
public enum GridSessionState
{
    /// <summary>Grid mode is off; the global hotkey is the only thing listened for.</summary>
    Inactive,

    /// <summary>The grid is shown and the user is drilling down to a target cell.</summary>
    Selecting,

    /// <summary>A target is chosen; the user can nudge the cursor and click.</summary>
    Acting,
}
