using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TwBlazor;
using TwBlazorApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTwBlazor(_ => { }, Theme.CreateTheme);

await builder.Build().RunAsync();
