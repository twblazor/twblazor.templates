namespace TwBlazor.Templates.Cli;

/// <summary>
/// Resolves where the templates are installed from.
/// </summary>
internal static class TemplateSource
{
    /// <summary>
    /// The NuGet package that carries the templates.
    /// </summary>
    internal const string PackageId = "twblazor.templates";

    /// <summary>
    /// Resolves an explicit template source, or falls back to the templates bundled with the tool.
    /// </summary>
    /// <param name="requested">The value passed with <c>--templates</c>, if any.</param>
    /// <returns>A template folder path or a package id.</returns>
    /// <exception cref="CliException">Thrown when the requested folder does not exist.</exception>
    internal static string Resolve(string? requested)
    {
        if (string.IsNullOrWhiteSpace(requested))
        {
            return TemplateLocator.FindLocal() ?? PackageId;
        }

        if (Directory.Exists(requested))
        {
            var directory = Path.GetFullPath(requested);

            if (!TemplateLocator.IsTemplateDirectory(directory))
            {
                throw new CliException($"'{directory}' does not contain .template.config/template.json.");
            }

            return directory;
        }

        if (LooksLikePath(requested))
        {
            throw new CliException($"Template folder '{requested}' was not found.");
        }

        return requested.Trim();
    }

    private static bool LooksLikePath(string value) =>
        value.StartsWith('.') ||
        value.Contains(Path.DirectorySeparatorChar) ||
        value.Contains(Path.AltDirectorySeparatorChar) ||
        value.Contains('/') ||
        value.Contains('\\');
}

/// <summary>
/// Finds the template folder in a source checkout or in the installed tool.
/// </summary>
internal static class TemplateLocator
{
    /// <summary>
    /// Finds a local template folder, or <see langword="null"/> when the tool should use the package.
    /// </summary>
    /// <returns>The template folder, or <see langword="null"/>.</returns>
    internal static string? FindLocal()
    {
        var configured = Environment.GetEnvironmentVariable("TWLBLAZOR_TEMPLATES_PATH");

        if (IsTemplateDirectory(configured))
        {
            return Path.GetFullPath(configured!);
        }

        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            foreach (var directory in Ancestors(start))
            {
                var candidate = Path.Combine(directory, "templates", "twblazor");

                if (IsTemplateDirectory(candidate))
                {
                    return candidate;
                }
            }
        }

        var bundled = Path.Combine(AppContext.BaseDirectory, "templates", "twblazor");

        return IsTemplateDirectory(bundled) ? bundled : null;
    }

    /// <summary>
    /// Checks whether a folder is a dotnet new template.
    /// </summary>
    /// <param name="path">The folder to check.</param>
    /// <returns><see langword="true"/> when the folder holds a template.json.</returns>
    internal static bool IsTemplateDirectory(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        File.Exists(Path.Combine(path, ".template.config", "template.json"));

    private static IEnumerable<string> Ancestors(string start)
    {
        var directory = new DirectoryInfo(start);

        while (directory is not null)
        {
            yield return directory.FullName;
            directory = directory.Parent;
        }
    }
}
