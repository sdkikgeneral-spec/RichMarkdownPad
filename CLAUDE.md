# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build all projects
dotnet build RichMarkdownPad.slnx

# Run all tests
dotnet test RichMarkdownPad.slnx

# Run a single test project with verbose output
dotnet test tests/RichMarkdownPad.HostApp.Tests/RichMarkdownPad.HostApp.Tests.csproj --logger "console;verbosity=detailed"

# Pack NuGet package (dry run)
./scripts/pack-nuget.ps1

# Pack and push to NuGet.org
./scripts/pack-nuget.ps1 -Push -ApiKey <KEY>
```

## Architecture

This is a reusable WPF markdown editor control published as a NuGet package. The solution has three projects:

**`src/RichMarkdownPad.Controller`** — The NuGet-published library. Contains:
- `MarkdownEditorControl` (XAML UserControl) — two-pane layout: TextBox editor (left) + WebView2 HTML preview (right) with a GridSplitter
- `MarkdownEditorControllerViewModel` — MVVM state: debounced preview updates (200ms), dirty tracking, open/save command relay
- `MarkdigMarkdownRenderer` / `IMarkdownRenderer` — pluggable markdown-to-HTML conversion via Markdig
- Public API surface: `OpenRequested` / `SaveRequested` / `DirtyStateChanged` events; `DocumentText` / `IsDirty` properties; `MarkDocumentSaved()` method

**`samples/RichMarkdownPad.HostApp`** — WPF desktop app demonstrating the control. The host owns all file I/O (`DocumentFileService` using UTF-8 without BOM), file dialogs, unsaved-change prompts, and a status panel.

**`tests/RichMarkdownPad.HostApp.Tests`** — xUnit tests covering file service I/O roundtrips and markdown rendering against 8 sample `.md` files (basic, code blocks, tables, Japanese, edge cases).

### Data flow

User types → `DocumentText` binding → ViewModel debounce (200ms) → `MarkdigMarkdownRenderer` → `HtmlPreview` string → WebView2 `NavigateToString`

The control does **not** handle file I/O — the host app subscribes to `OpenRequested`/`SaveRequested` events and calls `MarkDocumentSaved()` after a successful save.

## Key technical details

- Target: `.NET 10.0-windows`, WPF enabled, nullable enabled
- Markdown engine: Markdig 0.37.0 with advanced extensions
- Preview: `Microsoft.Web.WebView2` (requires WebView2 runtime installed on host machine)
- NuGet publish config: `scripts/nuget.publish.json` (output → `artifacts/nuget`, push off by default)
- Symbol packages (`.snupkg`) are generated alongside the main package
