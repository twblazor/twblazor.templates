using TwBlazor.Templates.Cli;
using Xunit;

namespace TwBlazor.Templates.Tests;

public sealed class TailwindBuildTests
{
    [Theory]
    [InlineData("1", nameof(TailwindBuild.Cli))]
    [InlineData("cli", nameof(TailwindBuild.Cli))]
    [InlineData("CLI", nameof(TailwindBuild.Cli))]
    [InlineData("2", nameof(TailwindBuild.Npm))]
    [InlineData("npm", nameof(TailwindBuild.Npm))]
    [InlineData("3", nameof(TailwindBuild.Skip))]
    [InlineData("skip", nameof(TailwindBuild.Skip))]
    public void ParseCssBuildChoice_MapsAllInputs(string input, string expected)
    {
        Assert.Equal(expected, NewCommand.ParseCssBuildChoice(input)?.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("4")]
    [InlineData("y")]
    [InlineData("--no-npm")]
    public void ParseCssBuildChoice_InvalidReturnsNull(string? input)
    {
        Assert.Null(NewCommand.ParseCssBuildChoice(input));
    }

    [Fact]
    public void ResolveCssBuildMethod_NoNpmWins()
    {
        var options = new NewOptions { NoNpm = true, Tailwind = TailwindBuild.Cli };

        Assert.Equal(TailwindBuild.Skip, NewCommand.ResolveCssBuildMethod(options, interactive: false));
    }

    [Fact]
    public void ResolveCssBuildMethod_FlagBypassesMenu()
    {
        var options = new NewOptions { Tailwind = TailwindBuild.Npm };

        Assert.Equal(TailwindBuild.Npm, NewCommand.ResolveCssBuildMethod(options, interactive: true));
    }

    [Fact]
    public void ResolveCssBuildMethod_InteractiveNeedsPrompt()
    {
        var options = new NewOptions();

        Assert.Null(NewCommand.ResolveCssBuildMethod(options, interactive: true));
    }

    [Fact]
    public void ResolveCssBuildMethod_NonInteractiveDefaultsToCli()
    {
        var options = new NewOptions();

        Assert.Equal(TailwindBuild.Cli, NewCommand.ResolveCssBuildMethod(options, interactive: false));
    }

    [Theory]
    [InlineData("^4.3.3")]
    [InlineData("~4.3.3")]
    [InlineData("=4.3.3")]
    [InlineData("v4.3.3")]
    [InlineData("4.3.3")]
    public void ReadTailwindCliVersion_StripsPrefix(string range)
    {
        using var temp = new TemporaryDirectory();
        WritePackageJson(temp.Path, $$$"""{"devDependencies": {"@tailwindcss/cli": "{{{range}}}"}}""");

        Assert.Equal("4.3.3", NewCommand.ReadTailwindCliVersion(temp.Path));
    }

    [Fact]
    public void ReadTailwindCliVersion_MissingFileOrKeyReturnsNull()
    {
        using var noFile = new TemporaryDirectory();
        using var noDevDependencies = new TemporaryDirectory();
        using var noCliKey = new TemporaryDirectory();

        WritePackageJson(noDevDependencies.Path, """{"dependencies": {"@tailwindcss/cli": "4.3.3"}}""");
        WritePackageJson(noCliKey.Path, """{"devDependencies": {"tailwindcss": "4.3.3"}}""");

        Assert.Null(NewCommand.ReadTailwindCliVersion(noFile.Path));
        Assert.Null(NewCommand.ReadTailwindCliVersion(noDevDependencies.Path));
        Assert.Null(NewCommand.ReadTailwindCliVersion(noCliKey.Path));
    }

    [Theory]
    [InlineData("""{"devDependencies": {"@tailwindcss/cli": 4}}""")]
    [InlineData("""{"devDependencies": {"@tailwindcss/cli": ""}}""")]
    [InlineData("""{"devDependencies": {"@tailwindcss/cli": "   "}}""")]
    [InlineData("""{"devDependencies": [}""")]
    public void ReadTailwindCliVersion_NonStringOrUnreadableReturnsNull(string contents)
    {
        using var temp = new TemporaryDirectory();
        WritePackageJson(temp.Path, contents);

        Assert.Null(NewCommand.ReadTailwindCliVersion(temp.Path));
    }

    [Fact]
    public void ResolveCliPackage_PinsVersion()
    {
        Assert.Equal("@tailwindcss/cli@4.3.3", NewCommand.ResolveCliPackage("4.3.3"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveCliPackage_UnpinnedWhenNullOrBlank(string? version)
    {
        Assert.Equal("@tailwindcss/cli", NewCommand.ResolveCliPackage(version));
    }

    [Fact]
    public void BuildCliArguments_IncludesYesPinnedPackageAndPaths()
    {
        var expected = new[]
        {
            "--yes",
            "@tailwindcss/cli@4.3.3",
            "-i",
            "./MyApp/wwwroot/css/input.css",
            "-o",
            "./MyApp/wwwroot/css/output.css",
            "--minify"
        };

        Assert.Equal(expected, NewCommand.BuildCliArguments("MyApp", "4.3.3"));
    }

    [Fact]
    public void BuildCliArguments_UnpinnedWhenVersionNull()
    {
        Assert.Equal("@tailwindcss/cli", NewCommand.BuildCliArguments("MyApp", null)[1]);
    }

    [Theory]
    [InlineData("1\n", nameof(TailwindBuild.Cli))]
    [InlineData("cli\n", nameof(TailwindBuild.Cli))]
    [InlineData("2\n", nameof(TailwindBuild.Npm))]
    [InlineData("3\n", nameof(TailwindBuild.Skip))]
    public void PromptForCssBuildMethod_ReturnsChosenMethod(string input, string expected)
    {
        using var reader = new StringReader(input);
        using var writer = new StringWriter();

        Assert.Equal(expected, NewCommand.PromptForCssBuildMethod(writer, reader).ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("   \n")]
    public void PromptForCssBuildMethod_DefaultsToCliOnEmptyInput(string input)
    {
        using var reader = new StringReader(input);
        using var writer = new StringWriter();

        Assert.Equal(TailwindBuild.Cli, NewCommand.PromptForCssBuildMethod(writer, reader));
    }

    [Fact]
    public void PromptForCssBuildMethod_RepromptsUntilValid()
    {
        using var reader = new StringReader("4\n2\n");
        using var writer = new StringWriter();

        Assert.Equal(TailwindBuild.Npm, NewCommand.PromptForCssBuildMethod(writer, reader));
        Assert.Contains("Please enter 1, 2 or 3.", writer.ToString());
    }

    [Fact]
    public void ReadTailwindCliVersion_EmptyAfterStripReturnsNull()
    {
        using var caretOnly = new TemporaryDirectory();
        using var vOnly = new TemporaryDirectory();

        WritePackageJson(caretOnly.Path, """{"devDependencies": {"@tailwindcss/cli": "^"}}""");
        WritePackageJson(vOnly.Path, """{"devDependencies": {"@tailwindcss/cli": "v"}}""");

        Assert.Null(NewCommand.ReadTailwindCliVersion(caretOnly.Path));
        Assert.Null(NewCommand.ReadTailwindCliVersion(vOnly.Path));
    }

    [Fact]
    public async Task ProcessRunner_ReturnsNonZeroWhenExecutableMissing()
    {
        using var temp = new TemporaryDirectory();

        var exitCode = await ProcessRunner.RunAsync(
            "twblazor-missing-tool-xyz",
            Array.Empty<string>(),
            temp.Path,
            false,
            CancellationToken.None);

        Assert.NotEqual(0, exitCode);
    }

    private static void WritePackageJson(string directory, string contents) =>
        File.WriteAllText(Path.Combine(directory, "package.json"), contents);

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = Directory.CreateTempSubdirectory("twblazor-tests-").FullName;
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // Best-effort cleanup: a failed delete must not fail the test run.
            }
        }
    }
}
