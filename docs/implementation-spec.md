# RichMarkdownPad 実装仕様（Implementation Spec）

## 1. 目的
本ドキュメントは、`docs/specsheet.md`で確定した要件に基づき、リッチテキストボックス・コントローラーのMVP実装時の構成・責務・処理フローを定義する。

## 2. 前提
- フレームワーク: .NET 10 + WPF
- アーキテクチャ: MVVM
- Markdown変換: Markdig
- プレビュー描画: WebView2（固定）

## 3. プロジェクト構成（案）
- `src/RichMarkdownPad.Controller/`
- `src/RichMarkdownPad.Controller/Controls/`
- `src/RichMarkdownPad.Controller/ViewModels/`
- `src/RichMarkdownPad.Controller/Models/`
- `src/RichMarkdownPad.Controller/Services/`
- `samples/RichMarkdownPad.HostApp/`（動作確認用）

## 4. 主要コンポーネント
### 4.1 View
- `MarkdownEditorControl.xaml`（`UserControl`）
  - 左ペイン: Markdownエディタ（`TextBox`）
  - 右ペイン: プレビュー領域（`WebView2`）
  - `GridSplitter`でペイン幅変更

### 4.2 ViewModel
- `MarkdownEditorControllerViewModel`
  - `DocumentText: string`
  - `HtmlPreview: string`（必要に応じて保持）
  - `IsDirty: bool`
  - `RequestSaveCommand`, `RequestOpenCommand`（ホスト通知用）

### 4.3 Services
- `IMarkdownRenderer`
  - `string Render(string markdown)`
- `MarkdigMarkdownRenderer`
  - MarkdigでMarkdown -> HTMLへ変換
- `IEditorHostBridge`
  - ファイルI/Oやダイアログはホスト側へ委譲

## 5. イベントフロー
1. ユーザーがエディタに入力
2. `DocumentText`更新
3. デバウンス（200ms）
4. `IMarkdownRenderer.Render`実行
5. 生成HTMLを`WebView2`へ反映
6. `IsDirty`や変更イベントをホストへ通知

## 6. WebView2連携方針
- 初期化完了イベント後に初回HTMLを表示
- 反映方式は`NavigateToString`を基本とする
- 例外時は空白画面にせず、簡易エラーメッセージHTMLを表示

## 7. エラー処理
- Markdown変換例外: ログ出力 + プレビューにエラー表示
- ファイルI/O例外: ホスト側責務（コントローラーは通知のみ）
- WebView2初期化失敗: リトライ案内またはフォールバック表示

## 8. テスト方針（MVP）
- 単体テスト
  - Markdown変換結果（主要記法）
  - ホスト通知（Dirty遷移、保存要求イベント）
  - デバウンス制御の発火条件
- 手動テスト
  - 大きなテキスト入力時の操作感
  - ホストアプリに埋め込んだ際の連携確認

## 9. 実装順序（MVP）
- [x] `UserControl`シェル（2ペイン）
- [x] Controller ViewModelと通知コマンド実装
- [x] Markdigレンダラー実装
- [x] WebView2連携
- [x] ホスト連携インターフェース実装
- [x] 例外処理（MVP範囲）
- [ ] テスト実装（単体テスト/結合テスト）

## 10. 更新履歴
- 2026-03-09: 初版作成（specsheet.mdから分離）
- 2026-03-09: コントローラー開発を主目的とする構成へ更新
