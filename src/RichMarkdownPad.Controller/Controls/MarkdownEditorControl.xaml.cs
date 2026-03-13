using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RichMarkdownPad.Controller.Services;
using RichMarkdownPad.Controller.ViewModels;

namespace RichMarkdownPad.Controller.Controls;

public partial class MarkdownEditorControl : UserControl
{
    private readonly MarkdownEditorControllerViewModel _viewModel;
    private bool _webViewReady;
    private ScrollViewer? _editorScrollViewer;
    private int _lastLineCount;

    public MarkdownEditorControl()
    {
        InitializeComponent();

        _viewModel = new MarkdownEditorControllerViewModel(new MarkdigMarkdownRenderer());
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.OpenRequested += OnViewModelOpenRequested;
        _viewModel.SaveRequested += OnViewModelSaveRequested;

        DataContext = _viewModel;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public event EventHandler? OpenRequested;

    public event EventHandler? SaveRequested;

    public event EventHandler<bool>? DirtyStateChanged;

    public string DocumentText
    {
        get => _viewModel.DocumentText;
        set => _viewModel.LoadText(value);
    }

    public bool IsDirty => _viewModel.IsDirty;

    public void MarkDocumentSaved()
    {
        _viewModel.MarkSaved();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await EnsureWebViewReadyAsync();
        await UpdatePreviewAsync(_viewModel.HtmlPreview);
        InitLineNumbers();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_editorScrollViewer != null)
            _editorScrollViewer.ScrollChanged -= OnEditorScrollChanged;
        Editor.TextChanged -= OnEditorTextChanged;
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel.OpenRequested -= OnViewModelOpenRequested;
        _viewModel.SaveRequested -= OnViewModelSaveRequested;
        _viewModel.Dispose();
    }

    private void OnViewModelOpenRequested(object? sender, EventArgs e)
        => OpenRequested?.Invoke(this, EventArgs.Empty);

    private void OnViewModelSaveRequested(object? sender, EventArgs e)
        => SaveRequested?.Invoke(this, EventArgs.Empty);

    private void InitLineNumbers()
    {
        _editorScrollViewer = FindScrollViewer(Editor);
        if (_editorScrollViewer != null)
            _editorScrollViewer.ScrollChanged += OnEditorScrollChanged;
        Editor.TextChanged += OnEditorTextChanged;
        UpdateLineNumbers();
    }

    private void OnEditorScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        LineNumberScroller.ScrollToVerticalOffset(e.VerticalOffset);
    }

    private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateLineNumbers();
    }

    private void UpdateLineNumbers()
    {
        var text = Editor.Text;
        var lineCount = 1;
        for (int i = 0; i < text.Length; i++)
            if (text[i] == '\n') lineCount++;

        if (lineCount == _lastLineCount)
            return;
        _lastLineCount = lineCount;

        var sb = new StringBuilder();
        for (int i = 1; i <= lineCount; i++)
        {
            if (i > 1) sb.Append('\n');
            sb.Append(i);
        }
        LineNumberBlock.Text = sb.ToString();
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject d, int depth = 0)
    {
        if (depth > 15) return null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
        {
            var child = VisualTreeHelper.GetChild(d, i);
            if (child is ScrollViewer sv) return sv;
            var result = FindScrollViewer(child, depth + 1);
            if (result != null) return result;
        }
        return null;
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MarkdownEditorControllerViewModel.HtmlPreview))
        {
            await UpdatePreviewAsync(_viewModel.HtmlPreview);
            return;
        }

        if (e.PropertyName == nameof(MarkdownEditorControllerViewModel.IsDirty))
        {
            DirtyStateChanged?.Invoke(this, _viewModel.IsDirty);
        }
    }

    private async Task EnsureWebViewReadyAsync()
    {
        if (_webViewReady)
        {
            return;
        }

        try
        {
            await PreviewBrowser.EnsureCoreWebView2Async();
            PreviewBrowser.CoreWebView2.Settings.IsScriptEnabled = false;
            _webViewReady = true;
        }
        catch (Exception ex)
        {
            _webViewReady = false;
            Debug.WriteLine($"[RichMarkdownPad] WebView2 initialization failed: {ex}");
        }
    }

    private async Task UpdatePreviewAsync(string html)
    {
        if (!_webViewReady)
        {
            await EnsureWebViewReadyAsync();
        }

        if (_webViewReady)
        {
            PreviewBrowser.NavigateToString(html);
        }
    }
}
