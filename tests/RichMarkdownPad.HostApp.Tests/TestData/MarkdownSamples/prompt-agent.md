# Agent Workflow Prompt Sample

## Context

- Repository: `RichMarkdownPad`
- Branch: `main`
- Runtime: `.NET 10`

## Task

Design a reusable markdown editor controller and verify behavior.

### Steps

1. Read project files.
2. Implement missing components.
3. Run build and tests.
4. Report only actionable outcomes.

### Constraints

- Do not use destructive git commands.
- Preserve existing unrelated changes.
- Provide file references in reports.

### Example File References

- `src/RichMarkdownPad.Controller/Controls/MarkdownEditorControl.xaml`
- `samples/RichMarkdownPad.HostApp/MainWindow.xaml.cs:42`

---

#### Notes

Use fenced blocks for command examples:

```bash
dotnet build RichMarkdownPad.slnx
dotnet test RichMarkdownPad.slnx
```
