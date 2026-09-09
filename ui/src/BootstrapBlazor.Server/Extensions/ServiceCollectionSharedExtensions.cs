using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Blazored.LocalStorage;
using BootstrapBlazor.Server.Http;
using BootstrapBlazor.Server.Identity;
using BootstrapBlazor.Server.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionSharedExtensions
{
    public static IServiceCollection AddBootstrapBlazorServices(this IServiceCollection services, Action<BootstrapBlazorOptions>? configureOptions = null)
    {
        services.AddSingleton<WeatherForecastService>();
        services.AddSingleton<PackageVersionService>();
        services.AddSingleton<CodeSnippetService>();
        services.AddSingleton(typeof(IDataService<>), typeof(TableDemoDataService<>));
        services.AddSingleton<MockDataTableDynamicService>();
        services.AddSingleton<MenuService>();
        services.AddScoped<FanControllerDataService>();

        services.AddScoped<IRoleManagerService, RoleManagerService>();
        services.AddTransient<IUserManagerService, UserManagerService>();
        services.AddScoped<ITotpApiService, TotpApiService>();
        services.AddScoped<IWebAuthnApiService, WebAuthnApiService>();
        services.AddScoped<IYubikeyApiService, YubikeyApiService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IFileManagerService, FileManagerService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();
        services.AddScoped<DownloadFileService>();
        services.AddTransient<JsService>();
        services.AddScoped<IUserPermissionService, UserPermissionService>();
        services.AddScoped<IMenuLayoutClientService, MenuLayoutClientService>();
        services.AddScoped<IMenuLayoutProvider, MenuLayoutProvider>();
        services.AddScoped<IUrlAuthorizationService, UrlAuthorizationService>();
        services.AddBlazoredLocalStorage();
        services.AddAuthorizationCore();
        services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
        services.AddScoped<RequestClientUnauthorizedNotifier>();
        services.AddScoped<RequestClientCircuitGate>();
        services.AddScoped<CircuitHandler, RequestClientCircuitHandler>();

        services.AddBootstrapBlazorTotpService();
        services.AddOptionsMonitor<WebsiteOptions>();
        services.AddCascadingAuthenticationState();
        services.AddBootstrapBlazor(configureOptions);
        services.AddBootstrapBlazorHtml2PdfService();
        services.AddBootstrapBlazorTableExportService();
        services.AddBootstrapHolidayService();

        return services;
    }
}
