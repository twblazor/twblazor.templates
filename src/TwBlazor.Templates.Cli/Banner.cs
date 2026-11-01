namespace TwBlazor.Templates.Cli;

/// <summary>
/// Console colours and the purple twblazor banner used by the CLI.
/// </summary>
internal static class Banner
{
    private const string Purple = "\x1b[1;38;2;168;85;247m";
    private const string PurpleDim = "\x1b[38;2;147;51;234m";
    private const string PurpleLight = "\x1b[38;2;192;132;252m";
    private const string Green = "\x1b[32m";
    private const string Red = "\x1b[31m";
    private const string Reset = "\x1b[0m";

    private const string Url = "https://twblazor.com/get-started";

    private static readonly string[] Logo =
    [
        "░▀█▀░█░█░█▀▄░█░░░█▀█░▀▀█░█▀█░█▀▄░░░▀█▀░█▀▀░█▄█░█▀█░█░░░█▀█░▀█▀░█▀▀░█▀▀",
        "░░█░░█▄█░█▀▄░█░░░█▀█░▄▀░░█░█░█▀▄░░░░█░░█▀▀░█░█░█▀▀░█░░░█▀█░░█░░█▀▀░▀▀█",
        "░░▀░░▀░▀░▀▀░░▀▀▀░▀░▀░▀▀▀░▀▀▀░▀░▀░░░░▀░░▀▀▀░▀░▀░▀░░░▀▀▀░▀░▀░░▀░░▀▀▀░▀▀▀"
    ];

    private static string VersionLine => $"v{CliApplication.Version}";

    private static int LogoWidth => Logo.Max(line => line.Length);

    private static int RuleWidth => Math.Max(Math.Max(LogoWidth, VersionLine.Length), Url.Length) + 6;

    /// <summary>
    /// Prints the banner: a rule, the logo, the version line, the URL, and a closing rule.
    /// </summary>
    /// <param name="output">The writer the banner is written to.</param>
    internal static void WriteHeader(TextWriter output)
    {
        WriteBreakpoint(output);
        foreach (var line in Logo)
        {
            output.WriteLine(Paint(Center(line, RuleWidth)));
        }
        output.WriteLine(PaintMuted(Center(VersionLine, RuleWidth)));
        output.WriteLine(PaintMuted(Center(Url, RuleWidth)));
        WriteBreakpoint(output);
    }

    /// <summary>
    /// Prints a single rule line of equals signs.
    /// </summary>
    /// <param name="output">The writer the rule is written to.</param>
    internal static void WriteBreakpoint(TextWriter output) =>
        output.WriteLine(PaintMuted(new string('=', RuleWidth)));

    /// <summary>Paints text in the bright purple brand colour.</summary>
    internal static string Paint(string text) => WithColor(text, Purple);

    /// <summary>Paints text in the dim purple brand colour.</summary>
    internal static string PaintMuted(string text) => WithColor(text, PurpleDim);

    /// <summary>Paints text in a lighter purple, used for secondary text such as menu option labels.</summary>
    internal static string PaintLight(string text) => WithColor(text, PurpleLight);

    /// <summary>Paints text green (success).</summary>
    internal static string PaintSuccess(string text) => WithColor(text, Green);

    /// <summary>Paints text red (failure).</summary>
    internal static string PaintError(string text) => WithColor(text, Red);

    private static string Center(string text, int width)
    {
        if (text.Length >= width)
        {
            return text;
        }

        var padding = (width - text.Length) / 2;
        return new string(' ', padding) + text;
    }

    private static string WithColor(string text, string color) =>
        Console.IsOutputRedirected ? text : $"{color}{text}{Reset}";
}
