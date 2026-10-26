using System.Reflection;

namespace TwBlazor.Templates.Cli;

/// <summary>
/// Entry point for the <c>twblazor</c> command line tool.
/// </summary>
internal static class CliApplication
{
    private const string NewCommandName = "new";
    private const string InstallCommandName = "install";

    /// <summary>
    /// Gets the informational version of the tool.
    /// </summary>
    internal static string Version =>
        typeof(CliApplication).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";

    /// <summary>
    /// Runs the command described by <paramref name="args"/>.
    /// </summary>
    /// <param name="args">The raw command line arguments.</param>
    /// <param name="cancellationToken">A token that cancels the running command.</param>
    /// <returns>The process exit code.</returns>
    internal static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        try
        {
            if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
            {
                WriteHelp();
                return 0;
            }

            if (args[0] is "-v" or "--version" or "version")
            {
                Console.WriteLine(Version);
                return 0;
            }

            switch (args[0])
            {
                case NewCommandName:
                {
                    var options = CliParser.ParseNew(args[1..]);

                    if (options.ShowHelp)
                    {
                        WriteNewHelp();
                        return 0;
                    }

                    return await NewCommand.ExecuteAsync(options, Console.Out, Console.Error, cancellationToken);
                }

                case InstallCommandName:
                {
                    var options = CliParser.ParseInstall(args[1..]);

                    if (options.ShowHelp)
                    {
                        WriteInstallHelp();
                        return 0;
                    }

                    return await InstallCommand.ExecuteAsync(options, Console.Out, cancellationToken);
                }

                default:
                    throw new CliException($"Unknown command '{args[0]}'. Run 'twblazor --help'.");
            }
        }
        catch (CliException exception)
        {
            Console.Error.WriteLine($"error: {exception.Message}");
            return 1;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Cancelled.");
            return 130;
        }
    }

    private static void WriteHelp()
    {
        Console.WriteLine(
            """
            twblazor - scaffold a Blazor app with twblazor

            Usage:
              twblazor new <name> [options]     Create an app from a template
              twblazor install [options]        Install or update the templates only
              twblazor --version                Print the tool version
              twblazor --help                   Show this help

            Run 'twblazor new --help' for the template options.
            """);
    }

    private static void WriteNewHelp()
    {
        Console.WriteLine(
            """
            Usage:
              twblazor new <name> [options]

            Arguments:
              <name>                     Project and solution name, for example MyApp.

            Options:
              -i, --interactivity <mode>  Server, Wasm or Auto. Default: Server.
              -t, --template <layout>     dashboard, sidebar, navbar or blank. Default: dashboard.
              -o, --output <path>         Folder to create the app in. Default: ./<name>.
                  --templates <path|id>   Template folder or package id. Default: installed templates.
                  --no-restore            Skip dotnet restore.
                  --no-npm                Skip npm install and the Tailwind CSS build.
                  --force                 Write into a folder that is not empty.
              -h, --help                  Show this help.
            """);
    }

    private static void WriteInstallHelp()
    {
        Console.WriteLine(
            """
            Usage:
              twblazor install [options]

            Options:
                  --templates <path|id>  Template folder or package id. Default: installed templates.
              -h, --help                 Show this help.
            """);
    }
}
