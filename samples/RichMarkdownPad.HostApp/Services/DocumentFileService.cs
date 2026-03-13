using System.IO;
using System.Text;

namespace RichMarkdownPad.HostApp.Services;

public sealed class DocumentFileService : IDocumentFileService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public async Task<string> LoadTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"File is too large to open ({fileInfo.Length / 1024 / 1024} MB). Maximum supported size is {MaxFileSizeBytes / 1024 / 1024} MB.");

        return await File.ReadAllTextAsync(filePath, Utf8WithoutBom, cancellationToken);
    }

    public Task SaveTextAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        return File.WriteAllTextAsync(filePath, content ?? string.Empty, Utf8WithoutBom, cancellationToken);
    }
}
