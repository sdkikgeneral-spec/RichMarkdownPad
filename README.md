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
- ホスト側で実装可能なファイルI/O連携（Open/Save、未保存確認）
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

## 実装ガイド（アプリ組み込み）

### 1. 前提環境
- .NET SDK 10
- Windows（WPF / WebView2 実行環境）

### 2. 基本構成
- コントローラー本体: `src/RichMarkdownPad.Controller/`
- 主要UI: `src/RichMarkdownPad.Controller/Controls/MarkdownEditorControl.xaml`
- 状態/ロジック: `src/RichMarkdownPad.Controller/ViewModels/MarkdownEditorControllerViewModel.cs`
- Markdown変換: `src/RichMarkdownPad.Controller/Services/MarkdigMarkdownRenderer.cs`

### 3. 組み込み時の責務分担
- コントローラー側責務:
  - Markdown編集UI
  - ライブプレビュー更新（デバウンス）
  - `IsDirty`管理
  - Open/Save要求イベント通知
- ホストアプリ側責務:
  - ファイルダイアログ表示
  - ファイル読み書き（エンコーディング方針含む）
  - 終了時の未保存確認
  - 必要に応じた状態表示（ファイル名、最終操作など）

### 4. 実装時の推奨フロー
1. `MarkdownEditorControl`をホスト画面へ配置
2. `OpenRequested` / `SaveRequested` イベントをホストでハンドル
3. 読み込み時は `DocumentText` を設定
4. 保存成功時は `MarkDocumentSaved()` を呼び出し
5. `DirtyStateChanged` でホストUI状態を更新

### 5. 品質確認
```bash
dotnet build RichMarkdownPad.slnx
dotnet test RichMarkdownPad.slnx
```

Markdown投入ログを確認する場合:
```bash
dotnet test tests/RichMarkdownPad.HostApp.Tests/RichMarkdownPad.HostApp.Tests.csproj --logger "console;verbosity=detailed"
```

## 公開API

主にホストアプリが利用する公開APIは `MarkdownEditorControl` です。

対象クラス:
- `src/RichMarkdownPad.Controller/Controls/MarkdownEditorControl.xaml.cs`

### イベント
- `OpenRequested`
  - ユーザーがOpen操作を要求したときに発火
  - ホスト側でファイル選択と読み込みを実装
- `SaveRequested`
  - ユーザーがSave操作を要求したときに発火
  - ホスト側で保存処理を実装
- `DirtyStateChanged` (`EventHandler<bool>`)
  - `IsDirty` の変化を通知
  - 引数の `bool` が現在のDirty状態

### プロパティ
- `DocumentText: string`
  - エディタ内容の取得/設定
  - ファイル読込時はホスト側でこの値を設定
- `IsDirty: bool`（読み取り専用）
  - 未保存変更の有無

### メソッド
- `MarkDocumentSaved()`
  - ホスト側の保存成功後に呼び出し、Dirty状態をリセット

### 最小利用パターン
1. `MarkdownEditorControl` を画面に配置
2. `OpenRequested` / `SaveRequested` を購読
3. Open成功後に `DocumentText` へ読込内容を設定
4. Save成功後に `MarkDocumentSaved()` を呼び出し
5. `DirtyStateChanged` または `IsDirty` で画面状態を更新

注記:
- ViewModel（`MarkdownEditorControllerViewModel`）の公開メンバーは現時点では内部実装寄りです。
- アプリ組み込み時は `MarkdownEditorControl` 経由の利用を推奨します。

## Notes
- テスト実行例: `dotnet test RichMarkdownPad.slnx`
- 詳細ログ付き実行例: `dotnet test tests/RichMarkdownPad.HostApp.Tests/RichMarkdownPad.HostApp.Tests.csproj --logger "console;verbosity=detailed"`
