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
        var source = TemplateSource.Resolve(options.Templates);

        output.WriteLine($"Installing twblazor templates from {source}...");

        var exitCode = await ProcessRunner.RunAsync(
            "dotnet",
            ["new", "install", source, "--force"],
            Directory.GetCurrentDirectory(),
            cancellationToken);

        if (exitCode != 0)
        {
            throw new CliException("The templates could not be installed. See the output above for details.");
        }

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
        ArgumentNullException.ThrowIfNull(options.Name);

        var projectName = options.Name;
        var outputPath = Path.GetFullPath(options.OutputPath ?? Path.Combine(Directory.GetCurrentDirectory(), projectName));

        if (Directory.Exists(outputPath) && Directory.EnumerateFileSystemEntries(outputPath).Any() && !options.Force)
        {
            throw new CliException($"'{outputPath}' is not empty. Choose another folder or pass --force.");
        }

        var templateSource = TemplateSource.Resolve(options.Templates);

        output.WriteLine($"Installing twblazor templates from {templateSource}...");

        var installExitCode = await ProcessRunner.RunAsync(
            "dotnet",
            ["new", "install", templateSource, "--force"],
            Directory.GetCurrentDirectory(),
            cancellationToken);

        if (installExitCode != 0)
        {
            throw new CliException("The templates could not be installed. See the output above for details.");
        }

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
            options.Layout.ToTemplateValue()
        };

        if (options.Force)
        {
            arguments.Add("--force");
        }

        output.WriteLine($"Creating {projectName} ({options.Layout.ToTemplateValue()}, {options.Interactivity.ToTemplateValue()})...");

        var createExitCode = await ProcessRunner.RunAsync(
            "dotnet",
            arguments,
            Directory.GetCurrentDirectory(),
            cancellationToken);

        if (createExitCode != 0)
        {
            return createExitCode;
        }

        if (!options.NoRestore)
        {
            output.WriteLine("Restoring packages...");

            var solutionPath = Path.Combine(outputPath, $"{projectName}.slnx");
            var restoreExitCode = await ProcessRunner.RunAsync(
                "dotnet",
                ["restore", solutionPath],
                outputPath,
                cancellationToken);

            if (restoreExitCode != 0)
            {
                error.WriteLine("warning: dotnet restore failed. Run 'dotnet restore' in the project after fixing the error above.");
            }
        }

        if (!options.NoNpm)
        {
            await BuildTailwindAsync(outputPath, output, error, cancellationToken);
        }

        output.WriteLine();
        output.WriteLine($"Created {projectName} in {outputPath}");
        output.WriteLine($"  cd \"{outputPath}\"");
        output.WriteLine($"  dotnet run --project {projectName}");

        return 0;
    }

    private static async Task BuildTailwindAsync(
        string outputPath,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(Path.Combine(outputPath, "package.json")))
        {
            return;
        }

        output.WriteLine("Installing Tailwind CSS...");

        var installExitCode = await RunNpmAsync(outputPath, ["install"], cancellationToken);

        if (installExitCode != 0)
        {
            error.WriteLine("warning: npm install failed. Run 'npm install' in the project after fixing the error above.");
            return;
        }

        output.WriteLine("Building Tailwind CSS...");

        var buildExitCode = await RunNpmAsync(outputPath, ["run", "build:css"], cancellationToken);

        if (buildExitCode != 0)
        {
            error.WriteLine("warning: the Tailwind CSS build failed. Run 'npm run build:css' after fixing the error above.");
        }
    }

    private static Task<int> RunNpmAsync(string workingDirectory, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ProcessRunner.RunAsync("npm", arguments, workingDirectory, cancellationToken);
        }

        var npmArguments = new List<string> { "/c", "npm" };
        npmArguments.AddRange(arguments);

        return ProcessRunner.RunAsync("cmd.exe", npmArguments, workingDirectory, cancellationToken);
    }
}
