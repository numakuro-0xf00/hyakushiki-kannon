using hyakushiki_kannon.Core.Geometry;

namespace hyakushiki_kannon.Core.Input;

/// <summary>
/// Supplies monitor geometry. The real implementation queries the Win32 virtual screen and
/// per-monitor metrics; tests provide fixed rectangles.
/// </summary>
public interface IScreenProvider
{
    /// <summary>
    /// The bounding rectangle that encloses every monitor (the Win32 "virtual screen"). The grid
    /// overlay window spans this area.
    /// </summary>
    GridRect VirtualScreenBounds { get; }

    /// <summary>
    /// The current monitors and which one has focus. Grid mode opens on the focused monitor; the
    /// first keystroke may pick another monitor instead. Queried at activation time because the
    /// focused monitor changes as the user works.
    /// </summary>
    MonitorLayout GetMonitorLayout();
}
