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
  <PackageReference Include="RichMarkdownPad.Controller" Version="0.1.2" />
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
  <PackageReference Include="RichMarkdownPad.Controller" Version="0.1.2" />
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

## Changelog

### 0.1.2 — 日本語 (2026-03-13)

#### 新機能

- **行番号ガター** — エディタ左端に行番号を表示するガターを追加。エディタのスクロールに同期して行番号も連動してスクロールする。

#### バグ修正

- **[HIGH] ViewModel イベントのメモリリーク修正** — `OpenRequested` / `SaveRequested` をラムダではなく名前付きメソッドで購読し、`OnUnloaded` で明示的に解除するよう変更。
- **[HIGH] XSS: プレビュー WebView2 での JavaScript 実行を無効化** — `IsScriptEnabled = false` を設定し、Markdown 内の `<script>` タグがプレビューで実行されないよう修正。
- **[HIGH] 行番号更新の過剰なメモリアロケーション修正** — `Split('\n')` / `string.Join` を `for` ループ + `StringBuilder` に置換し、GC プレッシャーを大幅削減。行数変化がない場合は更新をスキップ。
- **[HIGH] WebView2 初期化失敗が握り潰されていた問題を修正** — `catch` ブロックを `catch (Exception ex)` に変更し、`Debug.WriteLine` でログ出力するよう修正。
- **[MEDIUM] CRLF での行番号ズレ修正** — `\n` カウント方式への変更に伴い CRLF / LF 両対応。
- **[MEDIUM] Markdown レンダリングが UI スレッドをブロックする問題を修正** — `Task.Run` でバックグラウンドスレッドに移動し、キャンセルトークンを渡して不要なレンダリングを中断可能に。
- **[MEDIUM] CancellationTokenSource のレースコンディション対策** — `_previewCts.Token` をキャプチャしてから `await` に入ることで、CTS 差し替え時の参照崩壊を防止。
- **[MEDIUM] `FindScrollViewer` の再帰深さ無制限問題を修正** — 深さ 15 超で探索を打ち切り、`StackOverflowException` を防止。
- **[LOW] ファイルサイズ無制限読み込みとエンコーディング不整合を修正** — 10 MB 上限チェックを追加。`LoadTextAsync` に `UTF-8 without BOM` を明示し、保存と統一。

### 0.1.2 — English (2026-03-13)

#### New Features

- **Line number gutter** — Added a line number panel to the left edge of the editor. The numbers scroll in sync with the editor's vertical scroll position.

#### Bug Fixes

- **[HIGH] Fixed memory leak in ViewModel event subscriptions** — `OpenRequested` / `SaveRequested` are now subscribed via named methods instead of lambdas, and explicitly unsubscribed in `OnUnloaded`.
- **[HIGH] XSS: Disabled JavaScript execution in preview WebView2** — Set `IsScriptEnabled = false` to prevent `<script>` tags in Markdown from executing in the preview pane.
- **[HIGH] Fixed excessive memory allocations in line number updates** — Replaced `Split('\n')` / `string.Join` with a `for` loop and `StringBuilder`, significantly reducing GC pressure. Updates are skipped when the line count has not changed.
- **[HIGH] Fixed silently swallowed WebView2 initialization failures** — Changed bare `catch` to `catch (Exception ex)` and added `Debug.WriteLine` logging.
- **[MEDIUM] Fixed line number misalignment with CRLF line endings** — Resolved as part of the `\n`-counting approach; both CRLF and LF are now handled correctly.
- **[MEDIUM] Fixed Markdown rendering blocking the UI thread** — Moved `_markdownRenderer.Render()` to a background thread via `Task.Run` with cancellation token support.
- **[MEDIUM] Fixed race condition in CancellationTokenSource replacement** — Capture `_previewCts.Token` into a local variable before entering `await` to prevent stale references after CTS is replaced.
- **[MEDIUM] Fixed unbounded recursion depth in `FindScrollViewer`** — Added a depth limit of 15 to prevent `StackOverflowException` on complex visual trees.
- **[LOW] Fixed unbounded file size loading and encoding inconsistency** — Added a 10 MB size cap (throws `InvalidOperationException` on excess). `LoadTextAsync` now explicitly uses `UTF-8 without BOM`, consistent with `SaveTextAsync`.

---

## Links
- Repository: https://github.com/sdkikgeneral-spec/RichMarkdownPad
