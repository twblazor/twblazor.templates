[![twblazor: Tailwind CSS components for Blazor](https://raw.githubusercontent.com/twblazor/twblazor/main/images/readme-banner.png)](https://twblazor.com/)

# twblazor.templates

Create a Blazor Web App with [twblazor](https://twblazor.com) and Tailwind CSS in one command.

The templates share one set of pages and layouts across Server, WebAssembly and Auto interactivity, so the app you get is the app you keep: no sample counter, no weather page, just a clean starting point.

## Templates

| Dashboard | Sidebar | Navbar |
| --- | --- | --- |
| [![Dashboard template: sidebar, top bar, KPI cards and charts](images/templates/dashboard.png)](images/templates/dashboard.png) | [![Sidebar template: grouped navigation and a welcome page](images/templates/sidebar.png)](images/templates/sidebar.png) | [![Navbar template: top navigation and a welcome page](images/templates/navbar.png)](images/templates/navbar.png) |

- **dashboard**: a sidebar, a top bar, KPI cards, revenue/traffic/order charts, recent orders and activity.
- **sidebar**: grouped sidebar navigation, a welcome and resources page, and a settings form.
- **navbar**: a top navigation bar with the same welcome and settings pages.

Every layout includes `TwToastProvider` and `TwDialogProvider`, light and dark mode classes, and a 404 page.

## Install

```bash
dotnet tool install -g twblazor.templates
```

This is a .NET tool, so install it with `dotnet tool install`, not `dotnet new install`.

## Create an app

Once the tool is installed, `twblazor new` is the only command you need:

```bash
twblazor new MyApp
```

To install the tool and create an app in one PowerShell line:

```powershell
dotnet tool install -g twblazor.templates; twblazor new MyApp -i wasm -t dashboard
```

`twblazor install` only registers the templates with `dotnet new`; it does not create an app.

Choose the render mode and layout:

```bash
twblazor new MyApp -i wasm -t dashboard
twblazor new MyApp -i auto -t sidebar
twblazor new MyApp -i server -t navbar
```

| Option | Values | Default |
| --- | --- | --- |
| `-i`, `--interactivity` | `server`, `wasm` / `webassembly`, `auto` | `server` |
| `-t`, `--template` | `dashboard`, `sidebar`, `navbar` | `dashboard` |
| `-o`, `--output` | Folder to create the app in | `./MyApp` |
| `--templates` | Template folder or package id | Installed templates |
| `--no-restore` | Skip `dotnet restore` | |
| `--no-npm` | Skip `npm install` and the Tailwind CSS build | |
| `--force` | Write into a folder that is not empty | |

The command installs the templates, creates the solution, restores NuGet packages and builds the Tailwind stylesheet. It then prints the two commands needed to run the app.

### Render modes

| Mode | Projects | Where the UI lives |
| --- | --- | --- |
| `server` | `MyApp` | `MyApp/` |
| `wasm` | `MyApp` and `MyApp.Client` | `MyApp.Client/` |
| `auto` | `MyApp` and `MyApp.Client` | `MyApp.Client/` |

The pages, layouts and `Theme.cs` move to the `.Client` project for WebAssembly and Auto, because that is where the interactive components run.

## Contributing

The preview app, build instructions, screenshot setup, theme sync and release process are documented in [CONTRIBUTING.md](CONTRIBUTING.md).

## License

MIT
