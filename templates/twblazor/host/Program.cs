using TwBlazor;
using TwBlazorApp;
#if (HasClient)
using TwBlazorApp.Client;
#endif

var builder = WebApplication.CreateBuilder(args);

#if (UseServer)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
#elif (UseWebAssembly)
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
#else
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
#endif

builder.Services.AddTwBlazor(_ => { }, Theme.CreateDefaultTheme);

var app = builder.Build();

#if (HasClient)
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}
#else
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
#endif

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
#if (UseServer)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
#elif (UseWebAssembly)
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Routes).Assembly);
#else
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Routes).Assembly);
#endif

await app.RunAsync();
