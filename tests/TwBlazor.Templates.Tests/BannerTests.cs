using TwBlazor.Templates.Cli;
using Xunit;

namespace TwBlazor.Templates.Tests;

public sealed class BannerTests
{
    [Fact]
    public void Banner_WriteHeader_ContainsVersionAndRule()
    {
        var writer = new StringWriter();

        Banner.WriteHeader(writer);

        var text = writer.ToString();
        Assert.Contains("█", text);
        Assert.Contains($"v{CliApplication.Version}", text);
        Assert.Contains(new string('=', 16), text);
        Assert.Contains("https://twblazor.com/get-started", text);
    }

    [Fact]
    public void Banner_WriteBreakpoint_WritesARuleOfEqualsSigns()
    {
        var writer = new StringWriter();

        Banner.WriteBreakpoint(writer);

        var line = writer.ToString().Trim();
        Assert.NotEmpty(line);
        Assert.All(line, character => Assert.Equal('=', character));
    }
}
