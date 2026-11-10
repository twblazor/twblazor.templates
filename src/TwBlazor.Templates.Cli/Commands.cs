using System.Text.Json;

namespace TwBlazor.Templates.Cli;

/// <summary>
/// Installs the templates bundled with the tool.
/// </summary>
internal static class InstallCommand
{
    /// <summary>
    /// Runs <c>twblazor install</c>.
    /// </summary>
    /// <param name="options">The parsed options.</param>
    /// <param name="output">The writer used for progress messages.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The process exit code.</returns>
    internal static async Task<int> ExecuteAsync(InstallOptions options, TextWriter output, CancellationToken cancellationToken)
    {
        Banner.WriteHeader(output);

        var source = TemplateSource.Resolve(options.Templates);

        var steps = new StepRunner(output, animate: !Console.IsOutputRedirected && !options.Verbose);

        var exitCode = await steps.RunAsync(
            "Installing twblazor templates",
            token => ProcessRunner.RunAsync("dotnet", ["new", "install", source, "--force"], Directory.GetCurrentDirectory(), options.Verbose, token),
            cancellationToken);

        if (exitCode != 0)
        {
            throw new CliException("The templates could not be installed. Run 'twblazor install -v' to see the details.");
        }

        Banner.WriteBreakpoint(output);
        output.WriteLine("Templates installed. Create an app with: twblazor new MyApp");

        return 0;
    }
}

/// <summary>
/// Creates a new app from the twblazor template.
/// </summary>
internal static class NewCommand
{
    /// <summary>
    /// Runs <c>twblazor new</c>.
    /// </summary>
    /// <param name="options">The parsed options.</param>
    /// <param name="output">The writer used for progress messages.</param>
    /// <param name="error">The writer used for warnings.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The process exit code.</returns>
    internal static async Task<int> ExecuteAsync(NewOptions options, TextWriter output, TextWriter error, CancellationToken cancellationToken)
    {
        Banner.WriteHeader(output);
        output.WriteLine();

        ArgumentNullException.ThrowIfNull(options.Name);

        var projectName = options.Name;
        var outputPath = Path.GetFullPath(options.OutputPath ?? Path.Combine(Directory.GetCurrentDirectory(), projectName));

        if (Directory.Exists(outputPath) && Directory.EnumerateFileSystemEntries(outputPath).Any() && !options.Force)
        {
            throw new CliException($"'{outputPath}' is not empty. Choose another folder or pass --force.");
        }

        var cssBuild = ResolveCssBuildMethod(options, interactive: !Console.IsInputRedirected);

        if (cssBuild is null)
        {
            cssBuild = PromptForCssBuildMethod(output, Console.In);
            output.WriteLine();
        }

        var steps = new StepRunner(output, animate: !Console.IsOutputRedirected && !options.Verbose);

        var templateSource = TemplateSource.Resolve(options.Templates);

        var installExitCode = await steps.RunAsync(
            "Installing twblazor templates",
            token => ProcessRunner.RunAsync("dotnet", ["new", "install", templateSource, "--force"], Directory.GetCurrentDirectory(), options.Verbose, token),
            cancellationToken);

        if (installExitCode != 0)
        {
            throw new CliException("The templates could not be installed. Run 'twblazor new -v' to see the details.");
        }

        var arguments = BuildTemplateArguments(options, projectName, outputPath);

        var createExitCode = await steps.RunAsync(
            $"Creating {projectName} ({options.Layout.ToTemplateValue()}, {options.Interactivity.ToTemplateValue()}, {options.Framework.ToTemplateValue()})",
            token => ProcessRunner.RunAsync("dotnet", arguments, Directory.GetCurrentDirectory(), options.Verbose, token),
            cancellationToken);

        if (createExitCode != 0)
        {
            return createExitCode;
        }

        if (!options.NoRestore)
        {
            var solutionPath = Path.Combine(outputPath, $"{projectName}.slnx");

            var restoreExitCode = await steps.RunAsync(
                "Restoring packages",
                token => ProcessRunner.RunAsync("dotnet", ["restore", solutionPath], outputPath, options.Verbose, token),
                cancellationToken);

            if (restoreExitCode != 0)
            {
                error.WriteLine("warning: dotnet restore failed. Run 'dotnet restore' in the project after fixing the error above.");
            }
        }

        if (cssBuild == TailwindBuild.Cli)
        {
            await BuildTailwindWithCliAsync(outputPath, projectName, options.Verbose, steps, error, cancellationToken);
        }
        else if (cssBuild == TailwindBuild.Npm)
        {
            await BuildTailwindWithNpmAsync(outputPath, options.Verbose, steps, error, cancellationToken);
        }

        output.WriteLine();
        Banner.WriteBreakpoint(output);
        output.WriteLine(Banner.Paint($"Created {projectName} in {outputPath}"));
        output.WriteLine(Banner.PaintMuted($"  cd \"{outputPath}\""));
        output.WriteLine(Banner.PaintMuted($"  dotnet run --project {projectName}"));
        Banner.WriteBreakpoint(output);

        return 0;
    }

    private static async Task BuildTailwindWithNpmAsync(
        string outputPath,
        bool verbose,
        StepRunner steps,
        TextWriter error,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(Path.Combine(outputPath, "package.json")))
        {
            error.WriteLine("warning: package.json was not found, so the Tailwind CSS build was skipped.");
            return;
        }

        var installExitCode = await steps.RunAsync(
            "Installing Tailwind CSS",
            token => RunNpmAsync(outputPath, ["install"], verbose, token),
            cancellationToken);

        if (installExitCode != 0)
        {
            error.WriteLine("warning: npm install failed. Run 'npm install' in the project after fixing the error above.");
            return;
        }

        var buildExitCode = await steps.RunAsync(
            "Building Tailwind CSS",
            token => RunNpmAsync(outputPath, ["run", "build:css"], verbose, token),
            cancellationToken);

        if (buildExitCode != 0)
        {
            error.WriteLine("warning: the Tailwind CSS build failed. Run 'npm run build:css' after fixing the error above.");
            return;
        }

        RemoveNpmArtifacts(outputPath, error);
    }

    private static async Task BuildTailwindWithCliAsync(
        string outputPath,
        string projectName,
        bool verbose,
        StepRunner steps,
        TextWriter error,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(Path.Combine(outputPath, "package.json")))
        {
            error.WriteLine("warning: package.json was not found, so the Tailwind CSS build was skipped.");
            return;
        }

        var version = ReadTailwindCliVersion(outputPath);

        var exitCode = await steps.RunAsync(
            "Building Tailwind CSS",
            token => RunNpxAsync(outputPath, BuildCliArguments(projectName, version), verbose, token),
            cancellationToken);

        if (exitCode != 0)
        {
            error.WriteLine($"warning: the Tailwind CSS build failed. Run 'npx @tailwindcss/cli -i ./{projectName}/wwwroot/css/input.css -o ./{projectName}/wwwroot/css/output.css --minify' after fixing the error above.");
        }
    }

    /// <summary>
    /// Deletes the <c>node_modules</c> folder and <c>package-lock.json</c> that the one-off Tailwind
    /// build leaves behind, so the scaffolded project stays clean. Failures only warn, so a locked
    /// folder never turns a successful scaffold into a failure.
    /// </summary>
    /// <param name="outputPath">The folder the app was created in.</param>
    /// <param name="error">The writer used for warnings.</param>
    internal static void RemoveNpmArtifacts(string outputPath, TextWriter error)
    {
        var nodeModules = Path.Combine(outputPath, "node_modules");
        var packageLock = Path.Combine(outputPath, "package-lock.json");

        try
        {
            if (Directory.Exists(nodeModules))
            {
                Directory.Delete(nodeModules, recursive: true);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            error.WriteLine($"warning: could not delete '{nodeModules}' ({exception.Message}). You can remove it yourself.");
        }

        try
        {
            if (File.Exists(packageLock))
            {
                File.Delete(packageLock);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            error.WriteLine($"warning: could not delete '{packageLock}' ({exception.Message}). You can remove it yourself.");
        }
    }

    /// <summary>
    /// Maps a menu answer to a stylesheet build method.
    /// </summary>
    /// <param name="input">The answer the user typed.</param>
    /// <returns>The build method, or <see langword="null"/> when the answer is not recognised.</returns>
    internal static TailwindBuild? ParseCssBuildChoice(string? input) =>
        input?.Trim().ToLowerInvariant() switch
        {
            "1" or "cli" or "tailwind" => TailwindBuild.Cli,
            "2" or "npm" => TailwindBuild.Npm,
            "3" or "skip" or "none" => TailwindBuild.Skip,
            _ => null
        };

    /// <summary>
    /// Resolves how to build the stylesheet from the options, without prompting.
    /// </summary>
    /// <param name="options">The parsed options.</param>
    /// <param name="interactive">Whether the session can prompt the user.</param>
    /// <returns>The build method, or <see langword="null"/> when the caller should prompt.</returns>
    internal static TailwindBuild? ResolveCssBuildMethod(NewOptions options, bool interactive)
    {
        if (options.NoNpm)
        {
            return TailwindBuild.Skip;
        }

        if (options.Tailwind is { } tailwind)
        {
            return tailwind;
        }

        return interactive ? null : TailwindBuild.Cli;
    }

    /// <summary>
    /// Builds the <c>dotnet new twblazor</c> arguments for the chosen options.
    /// </summary>
    /// <param name="options">The parsed options.</param>
    /// <param name="projectName">The project and solution name.</param>
    /// <param name="outputPath">The folder to create the app in.</param>
    /// <returns>The arguments to pass to <c>dotnet</c>.</returns>
    internal static List<string> BuildTemplateArguments(NewOptions options, string projectName, string outputPath)
    {
        var arguments = new List<string>
        {
            "new",
            "twblazor",
            "--name",
            projectName,
            "--output",
            outputPath,
            "--interactivity",
            options.Interactivity.ToTemplateValue(),
            "--layout",
            options.Layout.ToTemplateValue(),
            "--framework",
            options.Framework.ToTemplateValue()
        };

        if (options.Force)
        {
            arguments.Add("--force");
        }

        return arguments;
    }

    /// <summary>
    /// Reads the pinned Tailwind CSS CLI version from the generated <c>package.json</c>.
    /// </summary>
    /// <param name="outputPath">The folder the app was created in.</param>
    /// <returns>The version without its range prefix, or <see langword="null"/> when it cannot be read.</returns>
    internal static string? ReadTailwindCliVersion(string outputPath)
    {
        var packageJson = Path.Combine(outputPath, "package.json");

        if (!File.Exists(packageJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(packageJson));

            if (!document.RootElement.TryGetProperty("devDependencies", out var devDependencies) ||
                !devDependencies.TryGetProperty("@tailwindcss/cli", out var version) ||
                version.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var value = version.GetString();

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var stripped = value.Trim().TrimStart('^', '~', '=', 'v');

            return string.IsNullOrWhiteSpace(stripped) ? null : stripped;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Builds the <c>npx</c> package specifier for the Tailwind CSS CLI.
    /// </summary>
    /// <param name="version">The pinned CLI version, or <see langword="null"/> for the latest.</param>
    /// <returns>The package specifier passed to <c>npx</c>.</returns>
    internal static string ResolveCliPackage(string? version) =>
        string.IsNullOrWhiteSpace(version) ? "@tailwindcss/cli" : $"@tailwindcss/cli@{version}";

    /// <summary>
    /// Builds the <c>npx</c> arguments that generate the stylesheet with the Tailwind CSS CLI.
    /// </summary>
    /// <param name="projectName">The generated project name.</param>
    /// <param name="version">The pinned CLI version, or <see langword="null"/> for the latest.</param>
    /// <returns>The arguments passed to <c>npx</c>.</returns>
    internal static IReadOnlyList<string> BuildCliArguments(string projectName, string? version) =>
    [
        "--yes",
        ResolveCliPackage(version),
        "-i",
        $"./{projectName}/wwwroot/css/input.css",
        "-o",
        $"./{projectName}/wwwroot/css/output.css",
        "--minify"
    ];

    /// <summary>
    /// Prompts the user to choose how to build the stylesheet.
    /// </summary>
    /// <param name="output">The writer used for the menu.</param>
    /// <param name="input">The reader used for the user's answer.</param>
    /// <returns>The chosen build method.</returns>
    internal static TailwindBuild PromptForCssBuildMethod(TextWriter output, TextReader input)
    {
        output.WriteLine(Banner.Paint("How should the Tailwind CSS be built?"));
        output.WriteLine();
        output.WriteLine($"  {Banner.Paint("1.")} {Banner.PaintLight("tailwind")}");
        output.WriteLine($"  {Banner.Paint("2.")} {Banner.PaintLight("npm")}");
        output.WriteLine($"  {Banner.Paint("3.")} {Banner.PaintLight("skip")}");
        output.WriteLine();

        while (true)
        {
            output.Write("Enter 1, 2 or 3 (default 1): ");
            var line = input.ReadLine();

            if (line is null || string.IsNullOrWhiteSpace(line))
            {
                return TailwindBuild.Cli;
            }

            if (ParseCssBuildChoice(line) is { } choice)
            {
                return choice;
            }

            output.WriteLine("Please enter 1, 2 or 3.");
        }
    }

    private static Task<int> RunNpmAsync(string workingDirectory, IReadOnlyList<string> arguments, bool verbose, CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ProcessRunner.RunAsync("npm", arguments, workingDirectory, verbose, cancellationToken);
        }

        var npmArguments = new List<string> { "/c", "npm" };
        npmArguments.AddRange(arguments);

        return ProcessRunner.RunAsync("cmd.exe", npmArguments, workingDirectory, verbose, cancellationToken);
    }

    private static Task<int> RunNpxAsync(string workingDirectory, IReadOnlyList<string> arguments, bool verbose, CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ProcessRunner.RunAsync("npx", arguments, workingDirectory, verbose, cancellationToken);
        }

        var npxArguments = new List<string> { "/c", "npx" };
        npxArguments.AddRange(arguments);

        return ProcessRunner.RunAsync("cmd.exe", npxArguments, workingDirectory, verbose, cancellationToken);
    }
}
