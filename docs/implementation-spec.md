# RichMarkdownPad 実装仕様（Implementation Spec）

## 1. 目的
本ドキュメントは、`docs/specsheet.md`で確定した要件に基づき、MVP実装時の構成・責務・処理フローを定義する。

## 2. 前提
- フレームワーク: .NET 10 + WPF
- アーキテクチャ: MVVM
- Markdown変換: Markdig
- プレビュー描画: WebView2（固定）

## 3. プロジェクト構成（案）
- `src/RichMarkdownPad/`
- `src/RichMarkdownPad/Views/`
- `src/RichMarkdownPad/ViewModels/`
- `src/RichMarkdownPad/Models/`
- `src/RichMarkdownPad/Services/`
- `src/RichMarkdownPad/Commands/`

## 4. 主要コンポーネント
### 4.1 View
- `MainWindow.xaml`
  - 左ペイン: Markdownエディタ（`TextBox`）
  - 右ペイン: プレビュー領域（`WebView2`）
  - `GridSplitter`でペイン幅変更

### 4.2 ViewModel
- `MainWindowViewModel`
  - `DocumentText: string`
  - `HtmlPreview: string`（必要に応じて保持）
  - `CurrentFilePath: string?`
  - `IsDirty: bool`
  - `NewCommand`, `OpenCommand`, `SaveCommand`, `SaveAsCommand`

### 4.3 Services
- `IMarkdownRenderer`
  - `string Render(string markdown)`
- `MarkdigMarkdownRenderer`
  - MarkdigでMarkdown -> HTMLへ変換
- `IFileService`
  - `Load`, `Save`（UTF-8 BOM有無対応）

## 5. イベントフロー
1. ユーザーがエディタに入力
2. `DocumentText`更新
3. デバウンス（200ms）
4. `IMarkdownRenderer.Render`実行
5. 生成HTMLを`WebView2`へ反映

## 6. WebView2連携方針
- 初期化完了イベント後に初回HTMLを表示
- 反映方式は`NavigateToString`を基本とする
- 例外時は空白画面にせず、簡易エラーメッセージHTMLを表示

## 7. エラー処理
- Markdown変換例外: ログ出力 + プレビューにエラー表示
- ファイルI/O例外: ダイアログ通知 + 編集継続
- WebView2初期化失敗: リトライ案内またはフォールバック表示

## 8. テスト方針（MVP）
- 単体テスト
  - Markdown変換結果（主要記法）
  - ファイル保存/読込
  - デバウンス制御の発火条件
- 手動テスト
  - 大きなテキスト入力時の操作感
  - 未保存状態で終了時の確認

## 9. 実装順序（MVP）
1. WPFシェル（2ペイン + メニュー）
2. ViewModelとコマンド実装
3. Markdigレンダラー実装
4. WebView2連携
5. ファイル操作
6. 例外処理とテスト

## 10. 更新履歴
- 2026-03-09: 初版作成（specsheet.mdから分離）
