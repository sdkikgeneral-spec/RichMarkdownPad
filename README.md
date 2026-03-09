# RichMarkdownPad

Language: Japanese | [English](docs/README_en.md)

WPFでMarkdownを編集し、リアルタイムプレビューを確認できるリッチテキストボックス・コントローラーを開発するプロジェクトです。

## Project Status
- フェーズ: MVP実装進行中（コア機能とテスト基盤を実装済み）
- 現在の確定事項:
  - Framework: .NET 10
  - UI: WPF（MVVM）
  - Markdown renderer: Markdig
  - Preview engine: WebView2

## Implemented
- `MarkdownEditorControl`（2ペイン + `GridSplitter`）
- Markdig + WebView2 によるライブプレビュー（デバウンス更新）
- Dirty状態通知、Open/Save要求イベント
- サンプルホストでの実ファイルI/O（Open/Saveダイアログ、未保存確認）
- xUnitテスト（ファイルサービス、複雑Markdown流し込み、詳細ログ出力）

## Goals (MVP)
- ホスト画面に埋め込み可能な`UserControl`を提供
- 左右2ペイン（左: エディタ、右: プレビュー）
- 入力変更に対するライブプレビュー（デバウンス更新）
- 編集状態（Dirty）と保存要求イベントをホストへ通知

## Docs
- 要求仕様: `docs/specsheet.md`
- 実装仕様: `docs/implementation-spec.md`

## Planned Architecture
- MVVMベース
- サービス分離
  - `IMarkdownRenderer`（Markdown -> HTML）
  - `IEditorHostBridge`（ホストとの連携）
- WebView2の`NavigateToString`でプレビュー反映

## Development Plan
1. コントローラー公開APIの整理（依存プロパティ、イベント契約）
2. テスト拡充（UI連携、回帰ケース、負荷ケース）
3. 配布形態の整備（NuGet化検討）
4. ドキュメント/サンプルの拡充

## Notes
- テスト実行例: `dotnet test RichMarkdownPad.slnx`
- 詳細ログ付き実行例: `dotnet test tests/RichMarkdownPad.HostApp.Tests/RichMarkdownPad.HostApp.Tests.csproj --logger "console;verbosity=detailed"`
