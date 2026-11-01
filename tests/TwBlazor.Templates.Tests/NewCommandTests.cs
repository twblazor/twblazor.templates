using TwBlazor.Templates.Cli;
using Xunit;

namespace TwBlazor.Templates.Tests;

public sealed class NewCommandTests
{
    [Fact]
    public void RemoveNpmArtifacts_RemovesNodeModulesAndPackageLock()
    {
        using var temp = new TemporaryDirectory();

        var nodeModules = Path.Combine(temp.Path, "node_modules");
        var nestedPackage = Path.Combine(nodeModules, "some-pkg", "index.js");
        var packageLock = Path.Combine(temp.Path, "package-lock.json");
        var packageJson = Path.Combine(temp.Path, "package.json");
        var outputCss = Path.Combine(temp.Path, "TwBlazorApp", "wwwroot", "css", "output.css");

        Directory.CreateDirectory(Path.GetDirectoryName(nestedPackage)!);
        File.WriteAllText(nestedPackage, "console.log('test');");
        File.WriteAllText(packageLock, "{}");
        File.WriteAllText(packageJson, "{}");
        Directory.CreateDirectory(Path.GetDirectoryName(outputCss)!);
        File.WriteAllText(outputCss, "/* generated */");

        NewCommand.RemoveNpmArtifacts(temp.Path, TextWriter.Null);

        Assert.False(Directory.Exists(nodeModules), "node_modules should be deleted.");
        Assert.False(File.Exists(packageLock), "package-lock.json should be deleted.");
        Assert.True(File.Exists(packageJson), "package.json must be preserved.");
        Assert.True(File.Exists(outputCss), "The generated stylesheet must be preserved.");
    }

    [Fact]
    public void RemoveNpmArtifacts_DoesNothingWhenArtifactsAreAbsent()
    {
        using var temp = new TemporaryDirectory();
        var writer = new StringWriter();

        NewCommand.RemoveNpmArtifacts(temp.Path, writer);

        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void RemoveNpmArtifacts_WarnsWhenANodeModulesFileIsLocked()
    {
        using var temp = new TemporaryDirectory();

        if (!OperatingSystem.IsWindows())
        {
            // On Unix an open file can still be unlinked, so the deletion succeeds.
            return;
        }

        var nodeModules = Path.Combine(temp.Path, "node_modules");
        var nestedPackage = Path.Combine(nodeModules, "some-pkg", "index.js");
        var writer = new StringWriter();

        Directory.CreateDirectory(Path.GetDirectoryName(nestedPackage)!);
        File.WriteAllText(nestedPackage, "console.log('test');");

        FileStream? lockedFile = null;

        try
        {
            lockedFile = new FileStream(nestedPackage, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            NewCommand.RemoveNpmArtifacts(temp.Path, writer);
        }
        finally
        {
            lockedFile?.Dispose();
        }

        Assert.Contains("could not delete", writer.ToString());
        Assert.True(Directory.Exists(nodeModules), "node_modules should still exist when a file inside it is locked.");
    }

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
