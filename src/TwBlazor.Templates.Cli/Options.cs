namespace TwBlazor.Templates.Cli;

/// <summary>
/// The interactive render mode chosen for the generated app.
/// </summary>
internal enum Interactivity
{
    /// <summary>Components run on the server over a SignalR connection.</summary>
    Server,

    /// <summary>Components run in the browser with WebAssembly.</summary>
    WebAssembly,

    /// <summary>Components run on the server, then in the browser once the WebAssembly files are cached.</summary>
    Auto
}

/// <summary>
/// The layout shipped with the generated app.
/// </summary>
internal enum LayoutTemplate
{
    /// <summary>A sidebar and top bar, with a dashboard home page.</summary>
    Dashboard,

    /// <summary>A sidebar for navigation.</summary>
    Sidebar,

    /// <summary>A top navigation bar.</summary>
    Navbar,

    /// <summary>A centered blank home page with a link to the docs.</summary>
    Blank
}

/// <summary>
/// The options accepted by <c>twblazor new</c>.
/// </summary>
internal sealed class NewOptions
{
    /// <summary>Gets or sets the project and solution name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the interactive render mode.</summary>
    public Interactivity Interactivity { get; set; } = Interactivity.Server;

    /// <summary>Gets or sets the layout template.</summary>
    public LayoutTemplate Layout { get; set; } = LayoutTemplate.Dashboard;

    /// <summary>Gets or sets the folder to create the app in.</summary>
    public string? OutputPath { get; set; }

    /// <summary>Gets or sets the template folder or package id.</summary>
    public string? Templates { get; set; }

    /// <summary>Gets or sets whether dotnet restore is skipped.</summary>
    public bool NoRestore { get; set; }

    /// <summary>Gets or sets whether npm install and the Tailwind build are skipped.</summary>
    public bool NoNpm { get; set; }

    /// <summary>Gets or sets whether an existing non-empty folder is reused.</summary>
    public bool Force { get; set; }

    /// <summary>Gets or sets whether help was requested instead of a command to run.</summary>
    public bool ShowHelp { get; set; }
}

/// <summary>
/// The options accepted by <c>twblazor install</c>.
/// </summary>
internal sealed class InstallOptions
{
    /// <summary>Gets or sets the template folder or package id.</summary>
    public string? Templates { get; set; }

    /// <summary>Gets or sets whether help was requested instead of a command to run.</summary>
    public bool ShowHelp { get; set; }
}

/// <summary>
/// An error that should be shown to the user without a stack trace.
/// </summary>
internal sealed class CliException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CliException"/> class.
    /// </summary>
    /// <param name="message">The message shown to the user.</param>
    public CliException(string message)
        : base(message)
    {
    }
}
