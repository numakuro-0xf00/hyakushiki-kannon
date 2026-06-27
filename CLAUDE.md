# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`hyakushiki-kannon` is the codename for **GridMouse** (see `concept.md`, written in
Japanese): a Windows resident (system-tray) tool that lets the user move and click the
mouse cursor **entirely from the keyboard**, without leaving the home row.

Core interaction (the feature to build, not yet implemented):
1. A global hotkey (concept default `Alt+G`) activates "grid mode" over any app.
2. A grid overlay (3×3 / 4×4) is drawn across the screen(s).
3. Keystrokes select a cell, then **recursively subdivide** that cell to home in on a target.
4. The cursor jumps to the selected cell center; arrow-key nudging fine-tunes it.
5. Keystrokes perform left/right/double click and drag. `Esc` cancels grid mode.

Most of the repository is still Visual Studio WPF boilerplate (`MainWindow`, `App`) — the
grid overlay, global hotkey hook, and cursor/click automation are the work to be done.
`concept.md` is the source of truth for intended behavior and MVP scope.

## Stack

- **.NET 10** WPF desktop app (`net10.0-windows`, `WinExe`), C# with nullable + implicit usings enabled.
- Root namespace is `hyakushiki_kannon` (the hyphenated folder/project name is not a valid
  C# identifier, hence the underscore form in code).
- Single project; solution uses the newer `.slnx` XML format (`hyakushiki-kannon.slnx`).

This is a **Windows-only** app and depends on Win32 interop for its core features
(global hotkeys, cursor positioning, synthetic input, multi-monitor geometry) — expect
heavy use of P/Invoke (`user32.dll`: `SetCursorPos`, `SendInput`, `RegisterHotKey`,
`SetWindowsHookEx`, etc.) as these are added.

## Build / run

```powershell
dotnet build hyakushiki-kannon.slnx          # build
dotnet run --project hyakushiki-kannon       # run the WPF app
```

> Note: the build targets `net10.0-windows` and requires the **.NET 10 SDK on Windows**.
> The Linux/WSL environment here ships only the .NET 8 SDK and no Windows targeting pack,
> so builds and runs must happen on the Windows host (or a Windows CI runner), not in WSL.

There is no test project yet. When adding one, prefer xUnit and keep Win32 interop behind
thin, mockable abstractions so logic (grid subdivision, key mapping) is testable without a
real desktop.

## Working in this repo

- Use the **`roslyn-query`** skill for C# symbol navigation (definitions, references, call
  hierarchy, compile diagnostics) instead of grepping by hand.
- Specialized review agents are available and worth using proactively: `twada-test-reviewer`
  (test quality), `unix-architect-reviewer` and `ux-design-reviewer` (design/spec docs).
- Project and design discussion in this repo is conducted in **Japanese** (`concept.md`,
  agent prompts) — match that when writing docs or specs.
