using System.IO;
using System.Text;

namespace RichMarkdownPad.HostApp.Services;

public sealed class DocumentFileService : IDocumentFileService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public Task<string> LoadTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        return File.ReadAllTextAsync(filePath, cancellationToken);
    }

    public Task SaveTextAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        return File.WriteAllTextAsync(filePath, content ?? string.Empty, Utf8WithoutBom, cancellationToken);
    }
}
