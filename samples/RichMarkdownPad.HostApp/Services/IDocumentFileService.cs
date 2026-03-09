namespace RichMarkdownPad.HostApp.Services;

public interface IDocumentFileService
{
    Task<string> LoadTextAsync(string filePath, CancellationToken cancellationToken = default);

    Task SaveTextAsync(string filePath, string content, CancellationToken cancellationToken = default);
}
