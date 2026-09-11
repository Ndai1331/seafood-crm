using BootstrapBlazor.Server.Components;
using BootstrapBlazor.Server.Http;
using BootstrapBlazor.Server.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// dotnet run can use a non-Development environment when no launch profile is
// selected. Load the generated Razor/package assets in that case as well;
// otherwise MapStaticAssets falls back to wwwroot and CSS bundles return 500.
if (!builder.Environment.IsDevelopment())
{
    StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(2);
});

builder.Services.AddHttpClient();
RequestClient.Initialize(builder.Configuration);

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<BootstrapBlazor.Server.Services.Seafood.SeafoodApiService>();
builder.Services.AddDistributedMemoryCache();

var keyRingPath = builder.Configuration["DataProtection:KeyRingPath"]
                  ?? Path.Combine(builder.Environment.ContentRootPath, "dp-keys");
Directory.CreateDirectory(keyRingPath);
builder.Services.AddDataProtection()
    .SetApplicationName("seafood-crm-ui")
    .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));

builder.Services.AddBootstrapBlazorServerService();

var app = builder.Build();

var option = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (option != null)
{
    app.UseRequestLocalization(option.Value);
}

app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseUploaderStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
        ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddYears(1).ToString("R"));
    }
});

app.UseAntiforgery();
app.UseBootstrapBlazor();
app.MapStaticAssets();
app.MapDefaultControllerRoute();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
