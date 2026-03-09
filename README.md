# RichMarkdownPad

## Japanese

WPFアプリに組み込める、Markdown編集 + ライブプレビュー用コントローラーです。

### Features
- 2ペインUI（左: エディタ / 右: WebView2プレビュー）
- MarkdigによるMarkdown -> HTML変換
- デバウンス付きライブプレビュー更新
- Dirty状態管理とOpen/Save要求イベント通知

### Requirements
- .NET 10
- Windows（WPF / WebView2）

### Install
```xml
<ItemGroup>
  <PackageReference Include="RichMarkdownPad.Controller" Version="0.1.0" />
</ItemGroup>
```

### Public API
Main class: `MarkdownEditorControl`

- Events
  - `OpenRequested`
  - `SaveRequested`
  - `DirtyStateChanged` (`EventHandler<bool>`)
- Properties
  - `DocumentText` (`string`)
  - `IsDirty` (`bool`, read-only)
- Methods
  - `MarkDocumentSaved()`

### Quick Usage
```xml
<controller:MarkdownEditorControl x:Name="EditorControl"
                                  OpenRequested="OnOpenRequested"
                                  SaveRequested="OnSaveRequested" />
```

```csharp
private async void OnOpenRequested(object sender, EventArgs e)
{
    EditorControl.DocumentText = await File.ReadAllTextAsync(path);
}

private async void OnSaveRequested(object sender, EventArgs e)
{
    await File.WriteAllTextAsync(path, EditorControl.DocumentText);
    EditorControl.MarkDocumentSaved();
}
```

### Host Responsibilities
- ファイルI/O（Open/Saveダイアログ、読み書き）
- 未保存確認ダイアログ
- アプリ側ステータス表示（必要な場合）

## English

A reusable WPF controller for Markdown editing with live preview.

### Features
- Two-pane UI (left: editor / right: WebView2 preview)
- Markdown to HTML conversion using Markdig
- Debounced live preview updates
- Dirty-state management and Open/Save request events

### Requirements
- .NET 10
- Windows (WPF / WebView2)

### Install
```xml
<ItemGroup>
  <PackageReference Include="RichMarkdownPad.Controller" Version="0.1.0" />
</ItemGroup>
```

### Public API
Main class: `MarkdownEditorControl`

- Events
  - `OpenRequested`
  - `SaveRequested`
  - `DirtyStateChanged` (`EventHandler<bool>`)
- Properties
  - `DocumentText` (`string`)
  - `IsDirty` (`bool`, read-only)
- Methods
  - `MarkDocumentSaved()`

### Quick Usage
```xml
<controller:MarkdownEditorControl x:Name="EditorControl"
                                  OpenRequested="OnOpenRequested"
                                  SaveRequested="OnSaveRequested" />
```

```csharp
private async void OnOpenRequested(object sender, EventArgs e)
{
    EditorControl.DocumentText = await File.ReadAllTextAsync(path);
}

private async void OnSaveRequested(object sender, EventArgs e)
{
    await File.WriteAllTextAsync(path, EditorControl.DocumentText);
    EditorControl.MarkDocumentSaved();
}
```

### Host Responsibilities
- File I/O (open/save dialogs, read/write)
- Unsaved-change confirmation
- Optional app-side status display

## Links
- Repository: https://github.com/sdkikgeneral-spec/RichMarkdownPad
