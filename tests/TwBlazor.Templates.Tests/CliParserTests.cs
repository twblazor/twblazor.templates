using TwBlazor.Templates.Cli;
using Xunit;

namespace TwBlazor.Templates.Tests;

public sealed class CliParserTests
{
    [Theory]
    [InlineData("server", nameof(Interactivity.Server))]
    [InlineData("Server", nameof(Interactivity.Server))]
    [InlineData("s", nameof(Interactivity.Server))]
    [InlineData("wasm", nameof(Interactivity.WebAssembly))]
    [InlineData("WebAssembly", nameof(Interactivity.WebAssembly))]
    [InlineData("w", nameof(Interactivity.WebAssembly))]
    [InlineData("auto", nameof(Interactivity.Auto))]
    [InlineData("a", nameof(Interactivity.Auto))]
    public void ParseInteractivity_MapsAliases(string value, string expected)
    {
        Assert.Equal(expected, CliParser.ParseInteractivity(value).ToString());
    }

    [Fact]
    public void ParseInteractivity_RejectsUnknownMode()
    {
        var exception = Assert.Throws<CliException>(() => CliParser.ParseInteractivity("static"));

        Assert.Contains("server, wasm or auto", exception.Message);
    }

    [Theory]
    [InlineData("dashboard", nameof(LayoutTemplate.Dashboard))]
    [InlineData("Sidebar", nameof(LayoutTemplate.Sidebar))]
    [InlineData("navbar", nameof(LayoutTemplate.Navbar))]
    [InlineData("blank", nameof(LayoutTemplate.Blank))]
    public void ParseLayout_MapsNames(string value, string expected)
    {
        Assert.Equal(expected, CliParser.ParseLayout(value).ToString());
    }

    [Fact]
    public void ParseNew_ReadsEveryOption()
    {
        var options = CliParser.ParseNew(
        [
            "MyApp",
            "-i", "wasm",
            "-t", "sidebar",
            "-o", "apps/MyApp",
            "--templates", @".\templates\twblazor",
            "--no-restore",
            "--no-npm",
            "--force"
        ]);

        Assert.Equal("MyApp", options.Name);
        Assert.Equal(Interactivity.WebAssembly, options.Interactivity);
        Assert.Equal(LayoutTemplate.Sidebar, options.Layout);
        Assert.Equal("apps/MyApp", options.OutputPath);
        Assert.Equal(@".\templates\twblazor", options.Templates);
        Assert.True(options.NoRestore);
        Assert.True(options.NoNpm);
        Assert.True(options.Force);
    }

    [Fact]
    public void ParseNew_SupportsInlineValues()
    {
        var options = CliParser.ParseNew(["MyApp", "--interactivity=auto", "--template=navbar"]);

        Assert.Equal(Interactivity.Auto, options.Interactivity);
        Assert.Equal(LayoutTemplate.Navbar, options.Layout);
    }

    [Fact]
    public void ParseNew_DefaultsToServerAndDashboard()
    {
        var options = CliParser.ParseNew(["MyApp"]);

        Assert.Equal(Interactivity.Server, options.Interactivity);
        Assert.Equal(LayoutTemplate.Dashboard, options.Layout);
    }

    [Fact]
    public void ParseNew_RequiresAName()
    {
        var exception = Assert.Throws<CliException>(() => CliParser.ParseNew(["--no-npm"]));

        Assert.Contains("project name is required", exception.Message);
    }

    [Fact]
    public void ParseNew_RejectsAnUnknownOption()
    {
        var exception = Assert.Throws<CliException>(() => CliParser.ParseNew(["MyApp", "--wat"]));

        Assert.Contains("Unknown option", exception.Message);
    }

    [Fact]
    public void ParseNew_RejectsAnInvalidName()
    {
        var exception = Assert.Throws<CliException>(() => CliParser.ParseNew(["my-app"]));

        Assert.Contains("not a valid project name", exception.Message);
    }

    [Fact]
    public void ParseNew_RejectsAMissingValue()
    {
        var exception = Assert.Throws<CliException>(() => CliParser.ParseNew(["MyApp", "--template"]));

        Assert.Contains("needs a value", exception.Message);
    }

    [Fact]
    public void ParseNew_AllowsHelpWithoutAName()
    {
        var options = CliParser.ParseNew(["--help"]);

        Assert.True(options.ShowHelp);
    }

    [Fact]
    public void ParseInstall_ReadsTheTemplateSource()
    {
        var options = CliParser.ParseInstall(["--templates", "TwBlazor.Templates"]);

        Assert.Equal("TwBlazor.Templates", options.Templates);
    }

    [Fact]
    public void ParseInstall_RejectsAPositionalArgument()
    {
        var exception = Assert.Throws<CliException>(() => CliParser.ParseInstall(["Extra"]));

        Assert.Contains("Unknown option", exception.Message);
    }
}
