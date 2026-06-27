using hyakushiki_kannon.Core.Geometry;

namespace hyakushiki_kannon.Core.Input;

/// <summary>
/// Supplies monitor geometry. The real implementation queries the Win32 virtual screen
/// metrics; tests provide fixed rectangles.
/// </summary>
public interface IScreenProvider
{
    /// <summary>
    /// The bounding rectangle that encloses every monitor (the Win32 "virtual screen").
    /// This is the area the grid initially covers when grid mode starts.
    /// </summary>
    GridRect VirtualScreenBounds { get; }
}
