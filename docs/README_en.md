# RichMarkdownPad

Language: [Japanese](../README.md) | English

RichMarkdownPad is a project to build a rich textbox controller for WPF that enables Markdown editing with real-time preview.

## Project Status
- Phase: MVP implementation in progress (core features and test foundation are in place)
- Confirmed decisions:
  - Framework: .NET 10
  - UI: WPF (MVVM)
  - Markdown renderer: Markdig
  - Preview engine: WebView2

## Implemented
- `MarkdownEditorControl` (two-pane layout + `GridSplitter`)
- Live preview via Markdig + WebView2 (debounced updates)
- Dirty-state notification and Open/Save request events
- Real host-side file I/O in sample app (Open/Save dialogs, unsaved-change prompts)
- xUnit tests (file service, complex markdown ingestion, verbose logs)

## Goals (MVP)
- Provide an embeddable `UserControl` for host screens
- Two-pane layout (left: editor, right: preview)
- Live preview updates with debounce on text changes
- Notify host about dirty state and save requests

## Docs
- Requirements spec: `docs/specsheet.md`
- Implementation spec: `docs/implementation-spec.md`

## Planned Architecture
- MVVM-based design
- Service separation
  - `IMarkdownRenderer` (Markdown -> HTML)
  - `IEditorHostBridge` (host integration)
- Preview rendering via WebView2 `NavigateToString`

## Development Plan
1. Refine public controller API (dependency properties, event contracts)
2. Expand tests (UI integration, regression, stress cases)
3. Prepare distribution model (NuGet packaging)
4. Expand docs and samples

## Notes
- Test command: `dotnet test RichMarkdownPad.slnx`
- Verbose ingestion logs: `dotnet test tests/RichMarkdownPad.HostApp.Tests/RichMarkdownPad.HostApp.Tests.csproj --logger "console;verbosity=detailed"`
