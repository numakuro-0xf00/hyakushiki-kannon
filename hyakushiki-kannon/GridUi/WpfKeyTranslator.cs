using System.Windows.Input;
using hyakushiki_kannon.Core.Input;

namespace hyakushiki_kannon.GridUi;

/// <summary>
/// Maps WPF <see cref="Key"/> values to the framework-agnostic inputs understood by
/// <see cref="GridInputRouter"/>: a <see cref="SpecialKey"/> for control keys, or a character
/// for letter keys. Keeps WPF types out of the core.
/// </summary>
internal static class WpfKeyTranslator
{
    /// <summary>Maps the navigation/action keys; returns <c>false</c> for ordinary character keys.</summary>
    public static bool TryGetSpecial(Key key, out SpecialKey special)
    {
        switch (key)
        {
            case Key.Escape: special = SpecialKey.Escape; return true;
            case Key.Back: special = SpecialKey.Backspace; return true;
            case Key.Enter: special = SpecialKey.Enter; return true;
            case Key.Space: special = SpecialKey.Space; return true;
            case Key.Up: special = SpecialKey.ArrowUp; return true;
            case Key.Down: special = SpecialKey.ArrowDown; return true;
            case Key.Left: special = SpecialKey.ArrowLeft; return true;
            case Key.Right: special = SpecialKey.ArrowRight; return true;
            default: special = default; return false;
        }
    }

    /// <summary>
    /// Maps a letter (A-Z) or digit (top-row or numpad) key to its lowercase character so any
    /// such cell-key layout configured in <c>GridConfig</c> is selectable; returns <c>false</c>
    /// for keys that don't correspond to a cell character.
    /// </summary>
    public static bool TryGetChar(Key key, out char c)
    {
        if (key is >= Key.A and <= Key.Z)
        {
            c = (char)('a' + (key - Key.A));
            return true;
        }

        if (key is >= Key.D0 and <= Key.D9)
        {
            c = (char)('0' + (key - Key.D0));
            return true;
        }

        if (key is >= Key.NumPad0 and <= Key.NumPad9)
        {
            c = (char)('0' + (key - Key.NumPad0));
            return true;
        }

        c = default;
        return false;
    }
}
