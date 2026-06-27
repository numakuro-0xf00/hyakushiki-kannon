using hyakushiki_kannon.Core.Geometry;
using hyakushiki_kannon.Core.Input;

namespace hyakushiki_kannon.Interop;

/// <summary>
/// Win32 implementation of <see cref="IScreenProvider"/>. Reports the bounding rectangle that
/// spans every monitor (the "virtual screen") in physical pixels.
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
}
