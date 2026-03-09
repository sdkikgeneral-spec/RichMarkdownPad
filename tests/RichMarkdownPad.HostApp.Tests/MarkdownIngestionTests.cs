using RichMarkdownPad.Controller.Services;
using RichMarkdownPad.HostApp.Services;
using Xunit.Abstractions;

namespace RichMarkdownPad.HostApp.Tests;

public sealed class MarkdownIngestionTests
{
    private readonly ITestOutputHelper _output;
    private readonly DocumentFileService _fileService = new();
    private readonly MarkdigMarkdownRenderer _renderer = new();

    public MarkdownIngestionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task AllMarkdownSamples_CanBeLoadedAndRendered()
    {
        var samplesDirectory = Path.Combine(AppContext.BaseDirectory, "TestData", "MarkdownSamples");
        Assert.True(Directory.Exists(samplesDirectory), $"Samples directory not found: {samplesDirectory}");

        var markdownFiles = Directory.GetFiles(samplesDirectory, "*.md", SearchOption.TopDirectoryOnly);
        Assert.NotEmpty(markdownFiles);

        foreach (var markdownFile in markdownFiles)
        {
            var markdown = await _fileService.LoadTextAsync(markdownFile);
            Assert.False(string.IsNullOrWhiteSpace(markdown));

            _output.WriteLine($"[INGEST] file={Path.GetFileName(markdownFile)}");
            _output.WriteLine("[MARKDOWN-BEGIN]");
            _output.WriteLine(markdown);
            _output.WriteLine("[MARKDOWN-END]");

            var html = _renderer.Render(markdown);
            Assert.False(string.IsNullOrWhiteSpace(html));
            Assert.Contains("<", html);

            _output.WriteLine($"[RENDERED] file={Path.GetFileName(markdownFile)} htmlLength={html.Length}");
        }
    }

    [Theory]
    [InlineData("basic.md", "<h1")]
    [InlineData("code.md", "<code")]
    [InlineData("table.md", "<table")]
    [InlineData("mixed-ja.md", "<blockquote")]
    [InlineData("edge.md", "<hr")]
    [InlineData("prompt-system.md", "<input")]
    [InlineData("prompt-agent.md", "<h3")]
    [InlineData("prompt-spec.md", "<table")]
    public async Task SpecificSamples_RenderExpectedHtmlMarkers(string fileName, string expectedMarker)
    {
        var samplesDirectory = Path.Combine(AppContext.BaseDirectory, "TestData", "MarkdownSamples");
        var fullPath = Path.Combine(samplesDirectory, fileName);

        var markdown = await _fileService.LoadTextAsync(fullPath);
        var html = _renderer.Render(markdown);

        _output.WriteLine($"[CHECK] file={fileName} marker={expectedMarker}");
        _output.WriteLine("[MARKDOWN-BEGIN]");
        _output.WriteLine(markdown);
        _output.WriteLine("[MARKDOWN-END]");

        Assert.Contains(expectedMarker, html, StringComparison.OrdinalIgnoreCase);
    }
}
