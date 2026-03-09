using System.Text;
using RichMarkdownPad.HostApp.Services;

namespace RichMarkdownPad.HostApp.Tests;

public sealed class DocumentFileServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly DocumentFileService _service = new();

    public DocumentFileServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "RichMarkdownPad.HostApp.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task SaveTextAsync_And_LoadTextAsync_RoundTripsContent()
    {
        var filePath = Path.Combine(_testDirectory, "roundtrip.md");
        const string content = "# Title\n\n- item 1\n- item 2\n";

        await _service.SaveTextAsync(filePath, content);
        var loaded = await _service.LoadTextAsync(filePath);

        Assert.Equal(content, loaded);
    }

    [Fact]
    public async Task SaveTextAsync_WritesUtf8WithoutBom()
    {
        var filePath = Path.Combine(_testDirectory, "encoding.md");
        const string content = "こんにちは RichMarkdownPad";

        await _service.SaveTextAsync(filePath, content);
        var bytes = await File.ReadAllBytesAsync(filePath);

        var hasUtf8Bom = bytes.Length >= 3 &&
                         bytes[0] == 0xEF &&
                         bytes[1] == 0xBB &&
                         bytes[2] == 0xBF;

        Assert.False(hasUtf8Bom);
        Assert.Equal(content, Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public async Task LoadTextAsync_WithBlankPath_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.LoadTextAsync(" "));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}
