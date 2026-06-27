namespace hyakushiki_kannon.Core.Input;

/// <summary>
/// Translates raw key events into <see cref="GridSession"/> intents according to the current
/// phase. This is the seam the UI layer talks to: it forwards each key press as either a
/// character (<see cref="HandleChar"/>) or a <see cref="SpecialKey"/> (<see cref="HandleSpecial"/>)
/// and uses the returned flag to decide whether to swallow the key.
///
/// <para>Bindings by phase:</para>
/// <list type="bullet">
///   <item><b>Selecting</b> - cell keys drill down; Enter/Space confirm (switch to Acting);
///   Backspace undoes a drill; Escape cancels; arrows nudge.</item>
///   <item><b>Acting</b> - Enter/Space left-click; the right-click and double-click keys act;
///   Backspace returns to selecting; Escape cancels; arrows nudge.</item>
/// </list>
/// </summary>
public sealed class GridInputRouter
{
    private readonly GridSession _session;
    private readonly GridKeyBindings _bindings;

    public GridInputRouter(GridSession session, GridKeyBindings? bindings = null)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _bindings = bindings ?? GridKeyBindings.Default;
    }

    /// <summary>
    /// Routes a printable-character key press.
    /// </summary>
    /// <returns><c>true</c> if the key was consumed by grid mode; <c>false</c> if it should pass through.</returns>
    public bool HandleChar(char c)
    {
        if (!_session.IsActive)
            return false;

        switch (_session.State)
        {
            case GridSessionState.Selecting:
                return _session.Drill(c);

            case GridSessionState.Acting:
                var key = char.ToLowerInvariant(c);
                if (key == _bindings.RightClick)
                {
                    _session.Click(MouseButton.Right);
                    return true;
                }
                if (key == _bindings.DoubleClick)
                {
                    _session.DoubleClick(MouseButton.Left);
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Routes a non-character key press.
    /// </summary>
    /// <returns><c>true</c> if the key was consumed by grid mode; <c>false</c> if it should pass through.</returns>
    public bool HandleSpecial(SpecialKey key)
    {
        if (!_session.IsActive)
            return false;

        switch (key)
        {
            case SpecialKey.Escape:
                _session.Cancel();
                return true;

            case SpecialKey.Backspace:
                return _session.Back();

            case SpecialKey.Enter:
            case SpecialKey.Space:
                if (_session.State == GridSessionState.Selecting)
                    _session.Confirm();
                else
                    _session.Click(MouseButton.Left);
                return true;

            case SpecialKey.ArrowUp:
                _session.Nudge(NudgeDirection.Up);
                return true;
            case SpecialKey.ArrowDown:
                _session.Nudge(NudgeDirection.Down);
                return true;
            case SpecialKey.ArrowLeft:
                _session.Nudge(NudgeDirection.Left);
                return true;
            case SpecialKey.ArrowRight:
                _session.Nudge(NudgeDirection.Right);
                return true;

            default:
                return false;
        }
    }
}
