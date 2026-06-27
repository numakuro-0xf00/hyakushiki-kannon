namespace hyakushiki_kannon.Core.Input;

/// <summary>
/// Non-character keys the grid router understands. The UI layer translates platform key
/// events (e.g. WPF <c>Key</c>) into these, keeping the core free of UI dependencies.
/// </summary>
public enum SpecialKey
{
    Escape,
    Backspace,
    Enter,
    Space,
    ArrowUp,
    ArrowDown,
    ArrowLeft,
    ArrowRight,
}
