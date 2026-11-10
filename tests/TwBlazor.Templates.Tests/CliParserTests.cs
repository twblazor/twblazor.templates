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

    [Theory]
    [InlineData("net11.0", nameof(TargetFramework.Net11))]
    [InlineData("NET11", nameof(TargetFramework.Net11))]
    [InlineData("11", nameof(TargetFramework.Net11))]
    [InlineData("net10.0", nameof(TargetFramework.Net10))]
    [InlineData("net10", nameof(TargetFramework.Net10))]
    [InlineData("10.0", nameof(TargetFramework.Net10))]
    public void ParseFramework_MapsNames(string value, string expected)
    {
        Assert.Equal(expected, CliParser.ParseFramework(value).ToString());
    }

    [Fact]
    public void ParseFramework_RejectsUnsupportedFrameworks()
    {
        var exception = Assert.Throws<CliException>(() => CliParser.ParseFramework("net9.0"));

        Assert.Contains("net11.0 or net10.0", exception.Message);
    }

    [Theory]
    [InlineData("-f", "net10.0")]
    [InlineData("--framework", "net10.0")]
    public void ParseNew_ReadsTheFramework(string option, string value)
    {
        var options = CliParser.ParseNew(["MyApp", option, value]);

        Assert.Equal(TargetFramework.Net10, options.Framework);
    }

    [Fact]
    public void ParseNew_DefaultsToNet11()
    {
        Assert.Equal(TargetFramework.Net11, CliParser.ParseNew(["MyApp"]).Framework);
    }

    [Theory]
    [InlineData(nameof(TargetFramework.Net11), "net11.0")]
    [InlineData(nameof(TargetFramework.Net10), "net10.0")]
    public void ToTemplateValue_GivesTheTargetFrameworkMoniker(string framework, string expected)
    {
        Assert.Equal(expected, Enum.Parse<TargetFramework>(framework).ToTemplateValue());
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

    [Theory]
    [InlineData("cli", nameof(TailwindBuild.Cli))]
    [InlineData("Cli", nameof(TailwindBuild.Cli))]
    [InlineData("npm", nameof(TailwindBuild.Npm))]
    [InlineData("Npm", nameof(TailwindBuild.Npm))]
    public void ParseTailwind_MapsNames(string value, string expected)
    {
        Assert.Equal(expected, CliParser.ParseTailwind(value).ToString());
    }

    [Theory]
    [InlineData("skip")]
    [InlineData("none")]
    public void ParseTailwind_RejectsSkipAndUnknown(string value)
    {
        var exception = Assert.Throws<CliException>(() => CliParser.ParseTailwind(value));

        Assert.Contains("--no-npm", exception.Message);
    }

    [Fact]
    public void ParseNew_ReadsTailwind()
    {
        var spaced = CliParser.ParseNew(["MyApp", "--tailwind", "npm"]);
        var inline = CliParser.ParseNew(["MyApp", "--tailwind=cli"]);
        var unspecified = CliParser.ParseNew(["MyApp"]);

        Assert.Equal(TailwindBuild.Npm, spaced.Tailwind);
        Assert.Equal(TailwindBuild.Cli, inline.Tailwind);
        Assert.Null(unspecified.Tailwind);
    }

    [Fact]
    public void ParseNew_RejectsInvalidTailwindValue()
    {
        Assert.Throws<CliException>(() => CliParser.ParseNew(["MyApp", "--tailwind", "skip"]));
    }

    [Theory]
    [InlineData("-v")]
    [InlineData("--verbose")]
    public void ParseNew_ReadsVerbose(string flag)
    {
        var options = CliParser.ParseNew(["MyApp", flag]);

        Assert.True(options.Verbose);
    }

    [Fact]
    public void ParseNew_VerboseIsFalseByDefault()
    {
        var options = CliParser.ParseNew(["MyApp"]);

        Assert.False(options.Verbose);
    }

    [Theory]
    [InlineData("-v")]
    [InlineData("--verbose")]
    public void ParseInstall_ReadsVerbose(string flag)
    {
        var options = CliParser.ParseInstall([flag]);

        Assert.True(options.Verbose);
    }

    [Fact]
    public void ParseInstall_VerboseIsFalseByDefault()
    {
        var options = CliParser.ParseInstall([]);

        Assert.False(options.Verbose);
    }
}
