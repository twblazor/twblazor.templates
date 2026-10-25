using System.Text.RegularExpressions;

namespace TwBlazor.Templates.Cli;

/// <summary>
/// Parses the command line arguments for the tool.
/// </summary>
internal static partial class CliParser
{
    /// <summary>
    /// Parses the arguments for <c>twblazor new</c>.
    /// </summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <returns>The parsed options.</returns>
    /// <exception cref="CliException">Thrown when an option or a value is not valid.</exception>
    internal static NewOptions ParseNew(IReadOnlyList<string> args)
    {
        var options = new NewOptions();
        var positionals = new List<string>();

        for (var index = 0; index < args.Count; index++)
        {
            var (name, inlineValue) = SplitOption(args[index]);

            switch (name)
            {
                case "-i":
                case "--interactivity":
                    options.Interactivity = ParseInteractivity(ReadValue(args, ref index, inlineValue, name));
                    break;

                case "-t":
                case "--template":
                    options.Layout = ParseLayout(ReadValue(args, ref index, inlineValue, name));
                    break;

                case "-o":
                case "--output":
                    options.OutputPath = ReadValue(args, ref index, inlineValue, name);
                    break;

                case "--templates":
                    options.Templates = ReadValue(args, ref index, inlineValue, name);
                    break;

                case "--no-restore":
                    EnsureNoValue(inlineValue, name);
                    options.NoRestore = true;
                    break;

                case "--no-npm":
                    EnsureNoValue(inlineValue, name);
                    options.NoNpm = true;
                    break;

                case "--force":
                    EnsureNoValue(inlineValue, name);
                    options.Force = true;
                    break;

                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    return options;

                default:
                    if (name.StartsWith('-'))
                    {
                        throw new CliException($"Unknown option '{name}'. Run 'twblazor new --help'.");
                    }

                    positionals.Add(name);
                    break;
            }
        }

        if (positionals.Count == 0)
        {
            throw new CliException("A project name is required. For example: twblazor new MyApp");
        }

        if (positionals.Count > 1)
        {
            throw new CliException($"Unexpected argument '{positionals[1]}'. A project name is the only positional argument.");
        }

        var projectName = positionals[0];

        if (!ProjectNameRegex().IsMatch(projectName))
        {
            throw new CliException($"'{projectName}' is not a valid project name. Use letters, digits, dots and underscores, starting with a letter.");
        }

        options.Name = projectName;
        return options;
    }

    /// <summary>
    /// Parses the arguments for <c>twblazor install</c>.
    /// </summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <returns>The parsed options.</returns>
    /// <exception cref="CliException">Thrown when an option or a value is not valid.</exception>
    internal static InstallOptions ParseInstall(IReadOnlyList<string> args)
    {
        var options = new InstallOptions();

        for (var index = 0; index < args.Count; index++)
        {
            var (name, inlineValue) = SplitOption(args[index]);

            switch (name)
            {
                case "--templates":
                    options.Templates = ReadValue(args, ref index, inlineValue, name);
                    break;

                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    return options;

                default:
                    throw new CliException($"Unknown option '{name}'. Run 'twblazor install --help'.");
            }
        }

        return options;
    }

    /// <summary>
    /// Maps the accepted render mode names and aliases to <see cref="Interactivity"/>.
    /// </summary>
    /// <param name="value">The value passed on the command line.</param>
    /// <returns>The matching render mode.</returns>
    /// <exception cref="CliException">Thrown when the value is not a render mode.</exception>
    internal static Interactivity ParseInteractivity(string value) =>
        value.ToLowerInvariant() switch
        {
            "s" or "server" => Interactivity.Server,
            "w" or "wasm" or "webassembly" => Interactivity.WebAssembly,
            "a" or "auto" => Interactivity.Auto,
            _ => throw new CliException($"'{value}' is not an interactive render mode. Use server, wasm or auto.")
        };

    /// <summary>
    /// Maps the accepted layout names to <see cref="LayoutTemplate"/>.
    /// </summary>
    /// <param name="value">The value passed on the command line.</param>
    /// <returns>The matching layout.</returns>
    /// <exception cref="CliException">Thrown when the value is not a layout.</exception>
    internal static LayoutTemplate ParseLayout(string value) =>
        value.ToLowerInvariant() switch
        {
            "dashboard" => LayoutTemplate.Dashboard,
            "sidebar" => LayoutTemplate.Sidebar,
            "navbar" => LayoutTemplate.Navbar,
            _ => throw new CliException($"'{value}' is not a template. Use dashboard, sidebar or navbar.")
        };

    /// <summary>
    /// Gets the value <c>dotnet new</c> expects for a render mode.
    /// </summary>
    /// <param name="interactivity">The render mode.</param>
    /// <returns>The template choice name.</returns>
    internal static string ToTemplateValue(this Interactivity interactivity) =>
        interactivity switch
        {
            Interactivity.Server => "Server",
            Interactivity.WebAssembly => "WebAssembly",
            Interactivity.Auto => "Auto",
            _ => throw new ArgumentOutOfRangeException(nameof(interactivity))
        };

    /// <summary>
    /// Gets the value <c>dotnet new</c> expects for a layout.
    /// </summary>
    /// <param name="layout">The layout.</param>
    /// <returns>The template choice name.</returns>
    internal static string ToTemplateValue(this LayoutTemplate layout) =>
        layout switch
        {
            LayoutTemplate.Dashboard => "dashboard",
            LayoutTemplate.Sidebar => "sidebar",
            LayoutTemplate.Navbar => "navbar",
            _ => throw new ArgumentOutOfRangeException(nameof(layout))
        };

    private static (string Name, string? InlineValue) SplitOption(string argument)
    {
        var separator = argument.IndexOf('=');

        return separator < 0
            ? (argument, null)
            : (argument[..separator], argument[(separator + 1)..]);
    }

    private static string ReadValue(IReadOnlyList<string> args, ref int index, string? inlineValue, string option)
    {
        if (inlineValue is not null)
        {
            if (inlineValue.Length == 0)
            {
                throw new CliException($"Option '{option}' needs a value.");
            }

            return inlineValue;
        }

        if (index + 1 >= args.Count)
        {
            throw new CliException($"Option '{option}' needs a value.");
        }

        return args[++index];
    }

    private static void EnsureNoValue(string? inlineValue, string option)
    {
        if (inlineValue is not null)
        {
            throw new CliException($"Option '{option}' does not take a value.");
        }
    }

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9._]*$", RegexOptions.CultureInvariant)]
    private static partial Regex ProjectNameRegex();
}
