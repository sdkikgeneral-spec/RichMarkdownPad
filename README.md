# RichMarkdownPad

Language: Japanese | [English](docs/README_en.md)

WPFでMarkdownを編集し、リアルタイムプレビューを確認できるリッチテキストボックス・コントローラーを開発するプロジェクトです。

## Project Status
- フェーズ: 仕様策定完了、実装準備中
- 現在の確定事項:
  - Framework: .NET 10
  - UI: WPF（MVVM）
  - Markdown renderer: Markdig
  - Preview engine: WebView2

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
1. コントローラー`UserControl`作成（2ペイン）
2. ViewModelとホスト通知コマンド実装
3. Markdigレンダラー実装
4. WebView2連携
5. ホスト連携と例外処理
6. テスト整備

## Notes
- このREADMEは初版です。実装の進行に合わせて更新します。
