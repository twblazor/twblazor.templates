using TwBlazor.Enums;
using TwBlazor.Models;

#if (HasClient)
namespace TwBlazorApp.Client;
#else
namespace TwBlazorApp;
#endif

/// <summary>
/// The links shown in the app's navigation. Add a page to the project, then add its route here.
/// </summary>
public static class AppNavigation
{
    /// <summary>
    /// Gets the links shown in the sidebar layouts.
    /// </summary>
    public static List<NavigationItem> Sidebar { get; } =
    [
        new()
        {
            Label = "Workspace",
            Icon = Icon.Folder,
            Collapsed = false,
            NavigationItems =
            [
                new() { Label = "Overview", Href = "/", Icon = Icon.Grid_1X2_Fill },
                new() { Label = "Settings", Href = "/settings", Icon = Icon.Gear }
            ]
        },
        new()
        {
            Label = "Resources",
            Icon = Icon.Journal_Text,
            Collapsed = false,
            NavigationItems =
            [
                new() { Label = "Documentation", Href = "https://twblazor.com", Icon = Icon.Book },
                new() { Label = "GitHub", Href = "https://github.com/twblazor/twblazor", Icon = Icon.Github },
                new() { Label = "API reference", Href = "https://twblazor.github.io/twblazor/", Icon = Icon.Journal_Code },
                new() { Label = "Theme builder", Href = "https://twblazor.com/theme", Icon = Icon.Brush }
            ]
        }
    ];

    /// <summary>
    /// Gets the links shown in the top navigation layout.
    /// </summary>
    public static List<NavigationItem> Top { get; } =
    [
        new() { Label = "Home", Href = "/", Icon = Icon.House_Door_Fill },
        new() { Label = "Docs", Href = "https://twblazor.com", Icon = Icon.Book },
        new() { Label = "Settings", Href = "/settings", Icon = Icon.Gear }
    ];
}
