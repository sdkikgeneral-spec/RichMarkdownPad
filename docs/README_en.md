# RichMarkdownPad

Language: [Japanese](../README.md) | English

RichMarkdownPad is a project to build a rich textbox controller for WPF that enables Markdown editing with real-time preview.

## Project Status
- Phase: Specification completed, implementation preparation in progress
- Confirmed decisions:
  - Framework: .NET 10
  - UI: WPF (MVVM)
  - Markdown renderer: Markdig
  - Preview engine: WebView2

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
1. Build controller `UserControl` (two-pane layout)
2. Implement ViewModel and host-notification commands
3. Implement Markdig renderer
4. Integrate WebView2 preview
5. Add host integration and exception handling
6. Prepare tests

## Notes
- This is the initial English README and will be updated as implementation progresses.
