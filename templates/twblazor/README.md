# TwBlazorApp

A Blazor app styled with [twblazor](https://twblazor.com) and Tailwind CSS. The projects reference the `twblazor` 3.4.0 NuGet package.

## Run it

```bash
dotnet run --project TwBlazorApp
```

`twblazor new` builds the stylesheet so the app is styled on first run. If you created the app with `dotnet new` or skipped the build, run the Tailwind build below first.

## Tailwind CSS

The app ships a ready-built `wwwroot/css/output.css`, so rebuild it only after changing classes, `Theme.cs` or pulling changes.

With the Tailwind CSS CLI, no install is needed:

```bash
npx @tailwindcss/cli -i ./TwBlazorApp/wwwroot/css/input.css -o ./TwBlazorApp/wwwroot/css/output.css --minify
```

While you work on markup, leave the watch build running in a second terminal so new classes show up straight away:

```bash
npx @tailwindcss/cli -i ./TwBlazorApp/wwwroot/css/input.css -o ./TwBlazorApp/wwwroot/css/output.css --watch
```

Prefer project-local tooling? Run `npm install` once to restore the `package.json` dev dependencies, then `npm run build:css` or `npm run watch:css`.

Open `TwBlazorApp.slnx` in Visual Studio to work on it there. The checked-in `.vsconfig` asks for the ASP.NET and web development workload, which is all the app needs.

## Where things are

- `TwBlazorApp/Pages` and `TwBlazorApp.Client/Pages`: your pages. Add a `.razor` file with an `@page` route and it appears in the app. With WebAssembly or Auto interactivity the pages live in the `.Client` project.
- `TwBlazorApp/Layout/MainLayout.razor` and `TwBlazorApp.Client/Layout/MainLayout.razor`: the layout around every page. Pick a different layout by generating the app again with `-t dashboard`, `-t sidebar`, `-t navbar` or `-t blank`.
- `AppNavigation.cs`: the sidebar and top bar links. Add a page's route here so it shows up in the navigation. The blank layout does not include it.
- `Theme.cs`: every twblazor class, in one file. To change colours, rounding, shadows or the components included, generate a new theme at [twblazor.com/theme](https://twblazor.com/theme) and replace the whole file.
- `TwBlazorApp/wwwroot/css/input.css`: the Tailwind CSS entry point.

`Theme.cs` is in the host project for Server interactivity and in the `.Client` project for WebAssembly or Auto interactivity, next to the components that use it.

Every component and its parameters are listed at [twblazor.com](https://twblazor.com).
