using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using RichMarkdownPad.Controller.Services;
using RichMarkdownPad.Controller.ViewModels;

namespace RichMarkdownPad.Controller.Controls;

public partial class MarkdownEditorControl : UserControl
{
    private readonly MarkdownEditorControllerViewModel _viewModel;
    private bool _webViewReady;

    public MarkdownEditorControl()
    {
        InitializeComponent();

        _viewModel = new MarkdownEditorControllerViewModel(new MarkdigMarkdownRenderer());
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.OpenRequested += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);
        _viewModel.SaveRequested += (_, _) => SaveRequested?.Invoke(this, EventArgs.Empty);

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
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel.Dispose();
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
            _webViewReady = true;
        }
        catch
        {
            _webViewReady = false;
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
