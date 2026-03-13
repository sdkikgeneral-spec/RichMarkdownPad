# Bug Fix Log

## 2026-03-13

コードレビューにて発見された問題を修正。

---

### Fix 1: ViewModel イベントのメモリリーク

**重大度:** HIGH
**ファイル:** `src/RichMarkdownPad.Controller/Controls/MarkdownEditorControl.xaml.cs`

**問題:**
コンストラクタで `_viewModel.OpenRequested` / `_viewModel.SaveRequested` をラムダで購読していたため、`OnUnloaded` で解除できずメモリリークが発生していた。

**修正:**
ラムダを名前付きメソッド `OnViewModelOpenRequested` / `OnViewModelSaveRequested` に変更し、`OnUnloaded` で明示的に解除するよう修正。

```csharp
// Before
_viewModel.OpenRequested += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);
_viewModel.SaveRequested += (_, _) => SaveRequested?.Invoke(this, EventArgs.Empty);

// After
_viewModel.OpenRequested += OnViewModelOpenRequested;
_viewModel.SaveRequested += OnViewModelSaveRequested;

// OnUnloaded に追加
_viewModel.OpenRequested -= OnViewModelOpenRequested;
_viewModel.SaveRequested -= OnViewModelSaveRequested;
```

---

### Fix 2: XSS — プレビュー WebView2 で JavaScript が実行可能だった

**重大度:** HIGH
**ファイル:** `src/RichMarkdownPad.Controller/Controls/MarkdownEditorControl.xaml.cs`

**問題:**
Markdig の出力 HTML をそのまま WebView2 に渡していたため、Markdown 内の `<script>` タグがプレビュー上で実行される可能性があった。信頼できない `.md` ファイルを開いた場合に任意スクリプト実行のリスク。

**修正:**
WebView2 初期化時に `IsScriptEnabled = false` を設定し、プレビューペインでの JavaScript 実行を無効化。

```csharp
await PreviewBrowser.EnsureCoreWebView2Async();
PreviewBrowser.CoreWebView2.Settings.IsScriptEnabled = false;
```

---

### Fix 3: 行番号更新の過剰なメモリアロケーション

**重大度:** HIGH (パフォーマンス)
**ファイル:** `src/RichMarkdownPad.Controller/Controls/MarkdownEditorControl.xaml.cs`

**問題:**
`UpdateLineNumbers()` が毎キーストロークで `Split('\n')` による配列生成と `string.Join` による文字列生成を行っており、大量の GC プレッシャーが発生していた。10,000 行のファイルで毎キーストロークごとに数万件のアロケーション。

**修正:**
- `Split('\n')` を `for` ループによる `\n` カウントに置換（配列生成なし）
- 行数変化がなければ `TextBlock` 更新をスキップ
- `string.Join` + `Enumerable.Range` を `StringBuilder` に置換

```csharp
// Before
var lineCount = Math.Max(1, Editor.Text.Split('\n').Length);
LineNumberBlock.Text = string.Join("\n", Enumerable.Range(1, lineCount));

// After
var text = Editor.Text;
var lineCount = 1;
for (int i = 0; i < text.Length; i++)
    if (text[i] == '\n') lineCount++;

if (lineCount == _lastLineCount) return;
_lastLineCount = lineCount;

var sb = new StringBuilder();
for (int i = 1; i <= lineCount; i++)
{
    if (i > 1) sb.Append('\n');
    sb.Append(i);
}
LineNumberBlock.Text = sb.ToString();
```

---

### Fix 4: WebView2 初期化失敗が完全に握り潰されていた

**重大度:** HIGH
**ファイル:** `src/RichMarkdownPad.Controller/Controls/MarkdownEditorControl.xaml.cs`

**問題:**
`EnsureWebViewReadyAsync` の `catch` ブロックが引数なしで、すべての例外（`OutOfMemoryException` 含む）を無音で握り潰していた。デバッグ不能。

**修正:**
`catch (Exception ex)` に変更し、`Debug.WriteLine` でログ出力するよう修正。

```csharp
// Before
catch
{
    _webViewReady = false;
}

// After
catch (Exception ex)
{
    _webViewReady = false;
    Debug.WriteLine($"[RichMarkdownPad] WebView2 initialization failed: {ex}");
}
```

---

### Fix 5: CRLF での行番号ズレ

**重大度:** MEDIUM
**ファイル:** `src/RichMarkdownPad.Controller/Controls/MarkdownEditorControl.xaml.cs`

**問題:**
`Split('\n')` による行数カウントでは、`\r\n` 改行の場合に各行末に `\r` が残り、行番号表示とテキスト表示がズレる可能性があった。

**修正:**
Fix 3 の `\n` カウント方式への変更と同時に解消。`\r` はカウントに影響しないため、CRLF・LF 両方で正しく動作する。

---

### Fix 6: Markdown レンダリングが UI スレッドをブロック

**重大度:** MEDIUM (パフォーマンス)
**ファイル:** `src/RichMarkdownPad.Controller/ViewModels/MarkdownEditorControllerViewModel.cs`

**問題:**
`_markdownRenderer.Render()` (Markdig の同期処理) を UI スレッド上で直接呼び出していたため、大きな Markdown ファイルで UI がフリーズしていた。

**修正:**
`Task.Run` でバックグラウンドスレッドに移動。キャンセルトークンも渡し、不要なレンダリングを途中で打ち切れるようにした。

```csharp
// Before
var htmlBody = _markdownRenderer.Render(_documentText);

// After
var text = _documentText;
var htmlBody = await Task.Run(() => _markdownRenderer.Render(text), token);
```

---

### Fix 7: CancellationTokenSource のレースコンディション対策

**重大度:** MEDIUM
**ファイル:** `src/RichMarkdownPad.Controller/ViewModels/MarkdownEditorControllerViewModel.cs`

**問題:**
`UpdatePreviewAsync` 内で `_previewCts` を `Cancel()` → `Dispose()` → 新規生成する際、既存タスクが `Dispose` 済みの CTS を参照したまま継続する可能性があった。

**修正:**
`_previewCts.Token` を `var token` にキャプチャしてから `await` に入ることで、CTS が差し替えられても既存タスクが安定したトークンを持ち続けるようにした（Fix 6 と同時対応）。

```csharp
_previewCts = new CancellationTokenSource();
var token = _previewCts.Token; // キャプチャしてから await へ

await Task.Delay(PreviewDebounceMilliseconds, token);
var htmlBody = await Task.Run(() => _markdownRenderer.Render(text), token);
```

---

### Fix 8: FindScrollViewer の再帰深さ無制限

**重大度:** MEDIUM
**ファイル:** `src/RichMarkdownPad.Controller/Controls/MarkdownEditorControl.xaml.cs`

**問題:**
ビジュアルツリーを再帰的に探索する `FindScrollViewer` に深さ制限がなく、複雑なビジュアルツリーで `StackOverflowException` が発生する可能性があった。

**修正:**
`depth` 引数を追加し、深さ 15 を超えた場合は探索を打ち切る。

```csharp
private static ScrollViewer? FindScrollViewer(DependencyObject d, int depth = 0)
{
    if (depth > 15) return null;
    // ...
    var result = FindScrollViewer(child, depth + 1);
```

---

### Fix 9: ファイルサイズ無制限読み込み & エンコーディング不整合

**重大度:** LOW
**ファイル:** `samples/RichMarkdownPad.HostApp/Services/DocumentFileService.cs`

**問題 1 — ファイルサイズ:**
`File.ReadAllTextAsync` はサイズチェックなしでファイル全体をメモリに読み込む。1 GB 超のファイルを開くと `OutOfMemoryException` が発生していた。

**問題 2 — エンコーディング:**
`LoadTextAsync` はエンコーディング未指定（`Encoding.UTF8` デフォルト）、`SaveTextAsync` は `UTF-8 without BOM` を明示しており不整合があった。UTF-16 等のファイルを開いた後に保存すると文字化けの可能性。

**修正:**
- 10 MB 上限チェックを追加（上限超過時は `InvalidOperationException`）
- `LoadTextAsync` にも `Utf8WithoutBom` を明示し、保存と統一

```csharp
private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

public async Task<string> LoadTextAsync(string filePath, ...)
{
    var fileInfo = new FileInfo(filePath);
    if (fileInfo.Length > MaxFileSizeBytes)
        throw new InvalidOperationException($"File is too large ...");

    return await File.ReadAllTextAsync(filePath, Utf8WithoutBom, cancellationToken);
}
```
