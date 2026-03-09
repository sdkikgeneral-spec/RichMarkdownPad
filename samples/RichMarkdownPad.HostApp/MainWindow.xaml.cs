using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using RichMarkdownPad.HostApp.Services;

namespace RichMarkdownPad.HostApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string? _currentFilePath;
    private readonly IDocumentFileService _documentFileService;

    public MainWindow()
    {
        _documentFileService = new DocumentFileService();
        InitializeComponent();
        EditorControl.DirtyStateChanged += EditorControl_OnDirtyStateChanged;
        EditorControl.DocumentText = "# RichMarkdownPad\n\nType markdown on the left. Preview is rendered on the right.";
        UpdateUiState("App Started");
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
        if (!EditorControl.IsDirty)
        {
            base.OnClosing(e);
            return;
        }

        var result = MessageBox.Show(
            this,
            "You have unsaved changes. Save before exit?",
            "Unsaved Changes",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Cancel)
        {
            e.Cancel = true;
            return;
        }

        if (result == MessageBoxResult.Yes)
        {
            var saveSucceeded = await SaveToFileAsync();
            if (!saveSucceeded)
            {
                e.Cancel = true;
                return;
            }
        }

        base.OnClosing(e);
    }

    private async void EditorControl_OnOpenRequested(object sender, EventArgs e)
    {
        if (EditorControl.IsDirty)
        {
            var result = MessageBox.Show(
                this,
                "Current changes are not saved. Continue opening another file?",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Markdown files (*.md;*.markdown)|*.md;*.markdown|Text files (*.txt)|*.txt|All files (*.*)|*.*",
            CheckFileExists = true,
            Title = "Open Markdown File"
        };

        if (dialog.ShowDialog(this) != true)
        {
            UpdateUiState("Open Canceled");
            return;
        }

        try
        {
            var text = await _documentFileService.LoadTextAsync(dialog.FileName);
            _currentFilePath = dialog.FileName;
            EditorControl.DocumentText = text;
            UpdateUiState($"Opened: {Path.GetFileName(dialog.FileName)}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to open file.\n{ex.Message}", "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
            UpdateUiState("Open Failed");
        }
    }

    private async void EditorControl_OnSaveRequested(object sender, EventArgs e)
    {
        await SaveToFileAsync();
    }

    private void EditorControl_OnDirtyStateChanged(object? sender, bool isDirty)
    {
        UpdateUiState("Edited");
    }

    private async Task<bool> SaveToFileAsync()
    {
        var targetPath = _currentFilePath;
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Markdown files (*.md)|*.md|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".md",
                AddExtension = true,
                Title = "Save Markdown File"
            };

            if (dialog.ShowDialog(this) != true)
            {
                UpdateUiState("Save Canceled");
                return false;
            }

            targetPath = dialog.FileName;
        }

        try
        {
            await _documentFileService.SaveTextAsync(targetPath, EditorControl.DocumentText);
            _currentFilePath = targetPath;
            EditorControl.MarkDocumentSaved();
            UpdateUiState($"Saved: {Path.GetFileName(targetPath)}");
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to save file.\n{ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            UpdateUiState("Save Failed");
            return false;
        }
    }

    private void UpdateUiState(string lastAction)
    {
        var fileName = string.IsNullOrWhiteSpace(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath);
        var dirtyMarker = EditorControl.IsDirty ? "*" : string.Empty;

        Title = $"{dirtyMarker}{fileName} - RichMarkdownPad Host Sample";
        FileStatusText.Text = $"File: {(_currentFilePath ?? "Untitled")}";
        DirtyStatusText.Text = $"Dirty: {EditorControl.IsDirty}";
        LastActionStatusText.Text = $"Last Action: {lastAction}";
    }
}