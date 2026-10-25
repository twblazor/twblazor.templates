using System.Text.Json;
using TwBlazor.Templates.Cli;
using Xunit;

namespace TwBlazor.Templates.Tests;

public sealed class TemplatePackageTests
{
    [Theory]
    [InlineData("ui/Layout/DashboardLayout.razor")]
    [InlineData("ui/Layout/SidebarLayout.razor")]
    [InlineData("ui/Layout/NavbarLayout.razor")]
    [InlineData("ui/Pages/Dashboard.razor")]
    [InlineData("ui/Pages/Welcome.razor")]
    [InlineData("ui/Pages/Settings.razor")]
    [InlineData("ui/Pages/NotFound.razor")]
    [InlineData("ui/AppNavigation.cs")]
    [InlineData("ui/Theme.cs")]
    [InlineData("ui/Routes.razor")]
    [InlineData("host/App.razor")]
    [InlineData("host/Program.cs")]
    [InlineData("host/_Imports.razor")]
    [InlineData("host/wwwroot/css/input.css")]
    [InlineData("client/Program.cs")]
    [InlineData("client/_Imports.razor")]
    [InlineData(".vsconfig")]
    public void Template_ContainsEverySharedFile(string relativePath)
    {
        var templateDirectory = FindTemplateDirectory();

        Assert.True(File.Exists(Path.Combine(templateDirectory, relativePath)), $"{relativePath} is missing from the template.");
    }

    [Fact]
    public void Template_DeclaresTheDocumentedChoices()
    {
        var templateJson = File.ReadAllText(Path.Combine(FindTemplateDirectory(), ".template.config", "template.json"));
        using var document = JsonDocument.Parse(templateJson);

        var symbols = document.RootElement.GetProperty("symbols");
        var interactivity = symbols.GetProperty("interactivity").GetProperty("choices").EnumerateArray().Select(choice => choice.GetProperty("choice").GetString()).ToList();
        var layouts = symbols.GetProperty("layout").GetProperty("choices").EnumerateArray().Select(choice => choice.GetProperty("choice").GetString()).ToList();

        Assert.Equal(["Server", "WebAssembly", "Auto"], interactivity);
        Assert.Equal(["dashboard", "sidebar", "navbar"], layouts);
    }

    [Fact]
    public void Template_LaunchSettingsHaveNoWslProfile()
    {
        var templateDirectory = FindTemplateDirectory();
        var launchSettingsFiles = Directory.GetFiles(templateDirectory, "launchSettings.json", SearchOption.AllDirectories);

        Assert.NotEmpty(launchSettingsFiles);
        Assert.All(launchSettingsFiles, file =>
        {
            var json = File.ReadAllText(file);

            Assert.DoesNotContain("\"WSL\"", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"commandName\": \"WSL", json, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void Preview_LaunchSettingsHaveNoWslProfile()
    {
        var templateDirectory = FindTemplateDirectory();
        var repositoryRoot = Directory.GetParent(templateDirectory)!.Parent!.FullName;
        var launchSettings = Path.Combine(repositoryRoot, "preview", "TwBlazor.Templates.Preview", "Properties", "launchSettings.json");

        Assert.True(File.Exists(launchSettings), $"{launchSettings} is missing.");

        var json = File.ReadAllText(launchSettings);
        Assert.DoesNotContain("\"WSL\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"commandName\": \"WSL", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Template_UsesThePublishedTwBlazorPackage()
    {
        var templateDirectory = FindTemplateDirectory();
        var projects = new[]
        {
            File.ReadAllText(Path.Combine(templateDirectory, "host", "TwBlazorApp.csproj")),
            File.ReadAllText(Path.Combine(templateDirectory, "client", "TwBlazorApp.Client.csproj"))
        };

        Assert.All(projects, project =>
        {
            Assert.Contains("<PackageReference Include=\"twblazor\" Version=\"3.0.0\" />", project);
            Assert.DoesNotContain("TWLBLAZOR_PROJECT_PATH", project);
        });
    }

    [Fact]
    public void TemplateSource_IsFoundFromTheRepository()
    {
        var found = TemplateLocator.FindLocal();

        Assert.NotNull(found);
        Assert.Equal(Path.GetFullPath(FindTemplateDirectory()), Path.GetFullPath(found));
    }

    private static string FindTemplateDirectory()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);

            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, "templates", "twblazor");

                if (TemplateLocator.IsTemplateDirectory(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("templates/twblazor was not found.");
    }
}
