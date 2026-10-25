using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TwBlazor;
using TwBlazorApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTwBlazor(_ => { }, Theme.CreateDefaultTheme);

await builder.Build().RunAsync();
