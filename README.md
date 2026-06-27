# hyakushiki-kannon / GridMouse

`hyakushiki-kannon` is the codename for **GridMouse**, a Windows resident (system-tray style)
tool that lets you move and click the mouse cursor entirely from the keyboard, without leaving
the home row. See [`concept.md`](concept.md) for the full concept (Japanese).

## How it works

1. Press the global hotkey **`Alt+G`** to overlay a grid on every monitor.
2. Press a **home-row key** (`a s d f g h j k l` for the default 3×3 grid) to select a cell.
   The cursor jumps to that cell's centre and the grid zooms in — repeat to home in on a target.
3. Press **`Enter`/`Space`** to confirm and left-click, **`r`** to right-click, **`d`** to
   double-click. Use the **arrow keys** to nudge, **`Backspace`** to step back one level, and
   **`Esc`** to cancel.

## Projects

| Project | Target | Purpose |
|---|---|---|
| `hyakushiki-kannon` | `net10.0-windows` (WPF) | The app: global hotkey, grid overlay, Win32 cursor/click interop. |
| `hyakushiki-kannon.Core` | `net8.0` | Platform-agnostic interaction logic — grid geometry, key mapping, and the grid-mode state machine — behind interop interfaces. No Win32/WPF dependencies, so it is fully unit-testable on any OS. |
| `hyakushiki-kannon.Core.Tests` | `net8.0` (xUnit) | Unit tests for the core logic. |

The Win32/WPF layer implements the core's `IPointerDevice` and `IScreenProvider` interfaces
(`Win32PointerDevice`, `VirtualScreenProvider`), so all decision-making stays in the tested core.

## Build & test

The core library and its tests build and run anywhere with the .NET 8+ SDK:

```bash
dotnet test hyakushiki-kannon.Core.Tests/hyakushiki-kannon.Core.Tests.csproj
```

The WPF app targets `net10.0-windows` and therefore requires the **.NET 10 SDK on Windows**
(it cannot be built on Linux/WSL, which lacks the Windows targeting pack):

```powershell
dotnet build hyakushiki-kannon/hyakushiki-kannon.csproj
dotnet run --project hyakushiki-kannon
```

> Note: for accurate cursor placement on multi-monitor / mixed-DPI setups the process should run
> per-monitor DPI aware; that refinement is tracked separately.
