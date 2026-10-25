[![twblazor: Tailwind CSS components for Blazor](https://raw.githubusercontent.com/twblazor/twblazor/main/images/readme-banner.png)](https://twblazor.com/)

# twblazor.templates

Create a Blazor Web App with [twblazor](https://twblazor.com) and Tailwind CSS in one command.

The templates share one set of pages and layouts across Server, WebAssembly and Auto interactivity, so the app you get is the app you keep: no sample counter, no weather page, just a clean starting point.

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

### Layouts

- **dashboard**: a sidebar, a top bar, KPI cards, a revenue goal, recent orders and activity.
- **sidebar**: a sidebar-driven layout with a welcome page and a settings form.
- **navbar**: a top navigation bar with the same pages.

Every layout includes `TwToastProvider` and `TwDialogProvider`, light and dark mode classes, and a 404 page.

### Render modes

| Mode | Projects | Where the UI lives |
| --- | --- | --- |
| `server` | `MyApp` | `MyApp/` |
| `wasm` | `MyApp` and `MyApp.Client` | `MyApp.Client/` |
| `auto` | `MyApp` and `MyApp.Client` | `MyApp.Client/` |

The pages, layouts and `Theme.cs` move to the `.Client` project for WebAssembly and Auto, because that is where the interactive components run.

## Preview the templates

The solution includes a Server preview app that links the template's shared `ui` files, so edits to the templates show up immediately:

```bash
cd preview/TwBlazor.Templates.Preview
npm install
npm run build:css
dotnet run
```

The first two commands are needed once. After that the project rebuilds the stylesheet on every build when `node_modules` exists. `wwwroot/css/output.css` is generated and ignored.

The preview routes are:

- `/` - dashboard layout
- `/sidebar` - sidebar layout
- `/navbar` - navbar layout
- `/settings` - settings page

The preview project uses the published `twblazor` package and is Server-only. To generate a complete app in Server, WebAssembly or Auto mode, use `preview.ps1`:

```powershell
.\preview.ps1 -Interactivity server -Layout dashboard
.\preview.ps1 -Interactivity wasm -Layout dashboard
```

The script writes the app to `artifacts/preview/<name>`, restores packages, builds Tailwind CSS and starts it with the HTTP launch profile.

| Option | Effect |
| --- | --- |
| `-Interactivity server\|wasm\|auto` | Render mode. Default: `server`. |
| `-Layout dashboard\|sidebar\|navbar` | Template layout. Default: `dashboard`. |
| `-Open` | Open the generated `.slnx` in Visual Studio instead of running it. |
| `-NoRun` | Generate the app without starting it. |
| `-NoRestore` | Skip `dotnet restore`. |
| `-NoNpm` | Skip `npm install` and the Tailwind build. |

The repository solution contains the CLI, the tests and the preview app. Visual Studio's "install extra components" prompt is about the workloads it infers, not the `.slnx` format. The checked-in `.vsconfig` asks for the ASP.NET and web development workload, which is what the solution and the generated apps need. Generated apps also include the `.vsconfig` and the web projects, so they should not ask for the desktop workload.

Running a WebAssembly preview in Debug does not need the WebAssembly AOT tools. If you later publish with AOT, install them with `dotnet workload install wasm-tools`.

## How the templates are organised

```
templates/twblazor/
  .template.config/     dotnet new symbols and option names
  host/                 Program.cs, App.razor, wwwroot for every app
  client/               Program.cs and imports for the .Client project
  ui/                   pages, layouts, navigation and Theme.cs shared by all modes
```

`template.json` copies `ui/` into the host project for Server and into the `.Client` project for WebAssembly and Auto, renames the chosen layout and home page, and drops the layouts that were not chosen.

## Repository layout

```
src/TwBlazor.Templates.Cli/   the twblazor command and packable tool
templates/twblazor/           the dotnet new template
preview/                      the Server preview app linked to the template UI
tests/                        tests for the parser, template and locators
scripts/                      release version helpers
.github/workflows/            CI, PR checks and NuGet publishing
preview.ps1                   generates and runs a preview app
.vsconfig                     the Visual Studio workload the solution needs
global.json                   pins the .NET 10 SDK
```

## Release flow

Releases follow the same develop -> main model as the main twblazor repository.

- Pull requests to `develop` run **PR Checks (develop)**.
- Pull requests to `main` must come from `develop` and run **PR Checks (main)**.
- Every PR to `main` carries exactly one release label. **Release Bump Label** creates the labels and fails if the PR has none, more than one, or `release:none` mixed with a bump.
- On merge to `main`, **Publish Release** turns the label into a version, waits for the `release` environment approval, creates the tag, builds, tests, packs, publishes to GitHub Packages and NuGet.org, creates a GitHub Release and fast-forwards `develop`.

### Release labels

| Label | Result |
| --- | --- |
| `release:major` | breaking change, e.g. `1.2.3` -> `2.0.0` |
| `release:minor` | backwards compatible change, e.g. `1.2.3` -> `1.3.0` |
| `release:patch` | fix, e.g. `1.2.3` -> `1.2.4`; applied automatically to new PRs |
| `release:none` | merge without releasing |

Versions live only in Git tags. MinVer derives the package version from the tag, so nothing in the repository stores or bumps a version.

The first release starts from no tag, so it is based on `0.0.0`. Apply `release:major` to make it `1.0.0`, or `release:patch` for `0.0.1`.

### Approve releases

Create an environment named `release` in the repository:

**Settings > Environments > New environment > release**

Add yourself as a **Required reviewer**. The `release` job in `publish.yml` then waits for your approval before it tags or publishes anything. Develop previews do not use this environment and do not need approval.

This repository's `release` environment is already configured with the owner as the required reviewer, and only `main` may deploy to it. Edit those rules under **Settings > Environments > release**.

### Develop previews

Every push to `develop` runs **Publish Develop Preview**. It computes `X.Y.Z-preview.N` from the newest release tag and the commit height, packs the tool, publishes it to GitHub Packages and uploads the `.nupkg` as an artifact. NuGet.org only receives stable releases from `publish.yml`, so develop never claims a stable NuGet version.

### NuGet trusted publishing

Publishing uses NuGet.org **trusted publishing** (GitHub OIDC), the same model as the main twblazor release. No NuGet API key is stored in GitHub.

In the `twblazor` organization on nuget.org, open **Trusted publishing** and add a policy:

- Repository owner: `twblazor`
- Repository: `twblazor.templates`
- Workflow file: `publish.yml`
- Environment: `release`

The policy for `twblazor/twblazor` does not cover this repository. Add a separate policy for `twblazor/twblazor.templates`.

In GitHub, make sure the `NUGET_ORG_USERNAME` secret is available to this repository. If it is an organization secret, grant this repository access; otherwise add it as a repository secret. Its value is the NuGet.org username that owns the policy, for example `twblazor`.

### Branch protection

Make these checks required:

- `develop`: **PR Checks (develop)**
- `main`: **PR Checks (main)** and **Release Bump Label**

If `main` or its tags are protected so the workflow's `GITHUB_TOKEN` cannot push the release tag or fast-forward `develop`, replace the token in those steps with a GitHub App token, as the main twblazor release does.

### Install a release

```bash
dotnet tool install -g twblazor.templates
twblazor new MyApp -i wasm -t dashboard
```

### Manual release

Run **Actions > Publish Release > Run workflow** from `main` and choose `major`, `minor` or `patch`. The same `release` environment approval applies.

## Work on it

```bash
dotnet build TwBlazor.Templates.slnx
dotnet test TwBlazor.Templates.slnx
dotnet pack src/TwBlazor.Templates.Cli -c Release -o artifacts
```

Run the CLI from source against the templates in this repository:

```bash
dotnet run --project src/TwBlazor.Templates.Cli -- new MyApp --templates templates/twblazor
```

## License

MIT
