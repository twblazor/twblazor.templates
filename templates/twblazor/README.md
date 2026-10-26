# TwBlazorApp

A Blazor app styled with [twblazor](https://twblazor.com) and Tailwind CSS. The projects reference the `twblazor` 3.0.1-preview.2 NuGet package.

## Run it

```bash
npm install
dotnet run --project TwBlazorApp
```

The first build creates `wwwroot/css/output.css`, so the app has no styling until `npm install` and a build have run.

## Tailwind CSS

Run the CSS build after changing classes or pulling changes:

```bash
npm run build:css
```

While you work on markup, leave the watch build running in a second terminal so new classes show up straight away:

```bash
npm run watch:css
```

Tailwind CSS is also rebuilt every time the app builds.

Open `TwBlazorApp.slnx` in Visual Studio to work on it there. The checked-in `.vsconfig` asks for the ASP.NET and web development workload, which is all the app needs.

## Where things are

- `TwBlazorApp/Pages` and `TwBlazorApp.Client/Pages`: your pages. Add a `.razor` file with an `@page` route and it appears in the app. With WebAssembly or Auto interactivity the pages live in the `.Client` project.
- `TwBlazorApp/Layout/MainLayout.razor` and `TwBlazorApp.Client/Layout/MainLayout.razor`: the layout around every page. Pick a different layout by generating the app again with `-t dashboard`, `-t sidebar`, `-t navbar` or `-t blank`.
- `AppNavigation.cs`: the sidebar and top bar links. Add a page's route here so it shows up in the navigation. The blank layout does not include it.
- `Theme.cs`: every twblazor class, in one file. To change colours, rounding, shadows or the components included, generate a new theme at [twblazor.com/theme](https://twblazor.com/theme) and replace the whole file.
- `TwBlazorApp/wwwroot/css/input.css`: the Tailwind CSS entry point.

`Theme.cs` is in the host project for Server interactivity and in the `.Client` project for WebAssembly or Auto interactivity, next to the components that use it.

Every component and its parameters are listed at [twblazor.com](https://twblazor.com).
