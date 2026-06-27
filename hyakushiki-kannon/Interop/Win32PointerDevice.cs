using System.ComponentModel;
using System.Runtime.InteropServices;
using hyakushiki_kannon.Core.Geometry;
using hyakushiki_kannon.Core.Input;

namespace hyakushiki_kannon.Interop;

/// <summary>
/// Win32 implementation of <see cref="IPointerDevice"/>: positions the real cursor with
/// <c>SetCursorPos</c> and synthesises button events with <c>SendInput</c>. Works in physical
/// screen pixels, matching <see cref="VirtualScreenProvider"/> (the app should run per-monitor
/// DPI aware so these coordinates are not virtualised).
/// </summary>
public sealed class Win32PointerDevice : IPointerDevice
{
    public GridPoint Position
    {
        get
        {
            // Returning a default (0,0) on failure would let a subsequent nudge teleport the
            // cursor to the screen corner; fail loudly instead so the caller can abort.
            if (!NativeMethods.GetCursorPos(out var p))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "GetCursorPos failed.");
            return new GridPoint(p.X, p.Y);
        }
    }

    public void MoveTo(GridPoint point)
    {
        var rounded = point.ToRounded();
        NativeMethods.SetCursorPos((int)rounded.X, (int)rounded.Y);
    }

    public void Click(MouseButton button)
    {
        var (down, up) = Flags(button);
        SendMouse(down);
        SendMouse(up);
    }

    public void DoubleClick(MouseButton button)
    {
        Click(button);
        Click(button);
    }

    private static (uint Down, uint Up) Flags(MouseButton button) => button switch
    {
        MouseButton.Left => (NativeMethods.MOUSEEVENTF_LEFTDOWN, NativeMethods.MOUSEEVENTF_LEFTUP),
        MouseButton.Right => (NativeMethods.MOUSEEVENTF_RIGHTDOWN, NativeMethods.MOUSEEVENTF_RIGHTUP),
        MouseButton.Middle => (NativeMethods.MOUSEEVENTF_MIDDLEDOWN, NativeMethods.MOUSEEVENTF_MIDDLEUP),
        _ => throw new ArgumentOutOfRangeException(nameof(button), button, null),
    };

    private static void SendMouse(uint flags)
    {
        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_MOUSE,
            u = new NativeMethods.InputUnion
            {
                mi = new NativeMethods.MOUSEINPUT { dwFlags = flags },
            },
        };

        NativeMethods.SendInput(1, [input], Marshal.SizeOf<NativeMethods.INPUT>());
    }
}
