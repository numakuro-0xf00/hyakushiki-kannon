using hyakushiki_kannon.Core.Geometry;
using hyakushiki_kannon.Core.Input;

namespace hyakushiki_kannon.Interop;

/// <summary>
/// Win32 implementation of <see cref="IScreenProvider"/>. Reports the virtual-screen bounds and
/// enumerates the individual monitors (with the focused one), in physical pixels.
/// </summary>
public sealed class VirtualScreenProvider : IScreenProvider
{
    public GridRect VirtualScreenBounds
    {
        get
        {
            var x = NativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN);
            var y = NativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN);
            var w = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN);
            var h = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN);
            return new GridRect(x, y, w, h);
        }
    }

    public MonitorLayout GetMonitorLayout()
    {
        var handles = new List<nint>();
        var bounds = new List<GridRect>();

        // EnumDisplayMonitors visits each monitor in a stable order; the callback rect is the
        // monitor's full bounds (no clip), so no extra GetMonitorInfo call is needed.
        bool Collect(nint hMonitor, nint hdc, ref NativeMethods.RECT r, nint data)
        {
            handles.Add(hMonitor);
            bounds.Add(new GridRect(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top));
            return true;
        }

        NativeMethods.EnumDisplayMonitors(nint.Zero, nint.Zero, Collect, nint.Zero);

        if (bounds.Count == 0)
        {
            // Fallback: treat the whole virtual screen as a single monitor.
            return new MonitorLayout(new[] { VirtualScreenBounds }, 0);
        }

        // The focused monitor is the one hosting the foreground window.
        var focusedHandle = NativeMethods.MonitorFromWindow(
            NativeMethods.GetForegroundWindow(), NativeMethods.MONITOR_DEFAULTTONEAREST);
        var focusedIndex = handles.IndexOf(focusedHandle);
        if (focusedIndex < 0)
            focusedIndex = 0;

        return new MonitorLayout(bounds, focusedIndex);
    }
}
