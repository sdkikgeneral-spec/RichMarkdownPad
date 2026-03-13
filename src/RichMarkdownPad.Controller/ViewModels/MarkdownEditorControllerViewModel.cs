using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RichMarkdownPad.Controller.Commands;
using RichMarkdownPad.Controller.Services;

namespace RichMarkdownPad.Controller.ViewModels;

public sealed class MarkdownEditorControllerViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IMarkdownRenderer _markdownRenderer;
    private CancellationTokenSource? _previewCts;
    private string _documentText = string.Empty;
    private string _htmlPreview;
    private bool _isDirty;

    public MarkdownEditorControllerViewModel(IMarkdownRenderer markdownRenderer)
    {
        _markdownRenderer = markdownRenderer;
        _htmlPreview = BuildHtmlDocument(string.Empty);

        RequestOpenCommand = new RelayCommand(() => OpenRequested?.Invoke(this, EventArgs.Empty));
        RequestSaveCommand = new RelayCommand(() => SaveRequested?.Invoke(this, EventArgs.Empty));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? OpenRequested;

    public event EventHandler? SaveRequested;

    public ICommand RequestOpenCommand { get; }

    public ICommand RequestSaveCommand { get; }

    public int PreviewDebounceMilliseconds { get; set; } = 200;

    public string DocumentText
    {
        get => _documentText;
        set
        {
            if (_documentText == value)
            {
                return;
            }

            _documentText = value;
            IsDirty = true;
            OnPropertyChanged();
            _ = UpdatePreviewAsync();
        }
    }

    public string HtmlPreview
    {
        get => _htmlPreview;
        private set
        {
            if (_htmlPreview == value)
            {
                return;
            }

            _htmlPreview = value;
            OnPropertyChanged();
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (_isDirty == value)
            {
                return;
            }

            _isDirty = value;
            OnPropertyChanged();
        }
    }

    public void LoadText(string text)
    {
        _documentText = text ?? string.Empty;
        OnPropertyChanged(nameof(DocumentText));
        IsDirty = false;
        _ = UpdatePreviewAsync();
    }

    public void MarkSaved()
    {
        IsDirty = false;
    }

    public void Dispose()
    {
        _previewCts?.Cancel();
        _previewCts?.Dispose();
    }

    private async Task UpdatePreviewAsync()
    {
        _previewCts?.Cancel();
        _previewCts?.Dispose();
        _previewCts = new CancellationTokenSource();
        var token = _previewCts.Token;

        try
        {
            await Task.Delay(PreviewDebounceMilliseconds, token);

            var text = _documentText;
            var htmlBody = await Task.Run(() => _markdownRenderer.Render(text), token);
            HtmlPreview = BuildHtmlDocument(htmlBody);
        }
        catch (OperationCanceledException)
        {
            // Debounce cancellation is expected while the user is typing.
        }
        catch (Exception ex)
        {
            HtmlPreview = BuildErrorHtml(ex.Message);
        }
    }

    private static string BuildHtmlDocument(string body)
    {
                const string template = """
<!doctype html>
<html>
<head>
  <meta charset=\"utf-8\" />
  <style>
    body {
      font-family: Segoe UI, sans-serif;
      margin: 1rem;
      line-height: 1.5;
      color: #222;
    }
    pre {
      background: #f5f5f5;
      padding: 0.75rem;
      border-radius: 6px;
      overflow-x: auto;
    }
    code {
      background: #f5f5f5;
      padding: 0.15rem 0.35rem;
      border-radius: 4px;
    }
  </style>
</head>
<body>
__BODY__
</body>
</html>
""";

    return template.Replace("__BODY__", body, StringComparison.Ordinal);
    }

    private static string BuildErrorHtml(string message)
    {
        var escapedMessage = System.Net.WebUtility.HtmlEncode(message);
        return BuildHtmlDocument($"<h3>Preview Error</h3><p>{escapedMessage}</p>");
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
