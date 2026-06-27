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

    /// <summary>Maps an A-Z letter key to its lowercase character; returns <c>false</c> otherwise.</summary>
    public static bool TryGetChar(Key key, out char c)
    {
        if (key is >= Key.A and <= Key.Z)
        {
            c = (char)('a' + (key - Key.A));
            return true;
        }

        c = default;
        return false;
    }
}
