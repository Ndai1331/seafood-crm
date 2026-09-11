

using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using BootstrapBlazor.Server.Identity;
using BootstrapBlazor.Server.Http;
using BootstrapBlazor.Server.Services;

namespace BootstrapBlazor.Server.Components.Layout;
/// <summary>
///
/// </summary>
public sealed partial class PageLayout : IDisposable
{
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    [NotNull]
    private Modal? Modal { get; set; }
    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }
    [Inject]
    [NotNull]
    private IUserManagerService? UerManagerService { get; set; }
    
    [Inject]
    [NotNull]
    private IUrlAuthorizationService? AuthorizationService { get; set; }

    [Inject]
    [NotNull]
    private IUserPermissionService? UserPermissionService { get; set; }

    [Inject]
    [NotNull]
    private RequestClientUnauthorizedNotifier? UnauthorizedNotifier { get; set; }

    /// <summary>Top-bar "Soạn thông báo" button (permission-gated, was ADMIN-hardcoded).</summary>
    private bool CanComposeNotification { get; set; }

    /// <summary>Floating AI chat widget (permission-gated, was ADMIN-hardcoded).</summary>
    private bool CanUseAiChat { get; set; }

    /// <summary>Non-null while a read-only impersonation session is active — drives the warning banner.</summary>
    public string? ImpersonatedUserName { get; set; }

    private async Task ExitImpersonationAsync()
    {
        await UerManagerService.StopImpersonationAsync();
        NavigationManager.NavigateTo("/", forceLoad: true);
    }
    
    private string SelectedCulture { get; set; } = CultureInfo.CurrentUICulture.Name;
    private string? Theme { get; set; }

    private string? LayoutClassString => CssBuilder.Default("layout-demo")
        .AddClass(Theme)
        .Build();

    private List<MenuItem>? Menus { get; set; } = new List<MenuItem>();

    /// <summary>
    /// </summary>
    public bool IsFixedHeader { get; set; } = true;

    /// <summary>
    /// </summary>
    public bool IsFixedFooter { get; set; } = true;

    /// <summary>
    /// </summary>
    public bool IsFixedTabHeader { get; set; } = true;

    /// <summary>
    /// </summary>
    public bool IsFullSide { get; set; } = false;

    /// <summary>
    /// </summary>
    public bool ShowFooter { get; set; } = true;
    public string UserName { get; set; } = "Admin";
    public int CurrentUserId { get; set; } = 1;
    
    /// <summary>
    /// Current user role - accessible by child components
    /// </summary>
    public string Role { get; set; } = "";
    private bool _isPageReady;

    /// <summary>
    /// Current page title for top menu
    /// </summary>
    public string CurrentPageTitle { get; set; } = "";

    /// <summary>
    /// Current user name for top menu
    /// </summary>
    public string CurrentUserName => UserName;

    /// <summary>
    /// Current user avatar for top menu
    /// </summary>
    public string CurrentUserAvatar => "/images/avatars/150-1.jpg";

    /// <summary>
    /// </summary>
    public bool UseTabSet { get; set; } = false;

    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; }
    [Inject, NotNull]
    private IServiceProvider? ServiceProvider { get; set; }
    
    [Inject]
    private IJSRuntime? JSRuntime { get; set; }

    public NewUserPasswordDto NewPassword { get; set; } = new NewUserPasswordDto();
    
    /// <summary>
    /// Track if location changed event is subscribed
    /// </summary>
    private bool _isLocationChangedSubscribed = false;
    private bool _isAuthStateChangedSubscribed = false;
    
    /// <summary>
    /// Current route ID for CSS targeting
    /// </summary>
    private string CurrentRouteId { get; set; } = "page-default";
    
    /// <summary>
    /// Generate route ID from current route path
    /// Examples:
    /// /report-seo-performance -> page-report-seo-performance
    /// /report-seo-performance-by-pic -> page-report-seo-performance-by-pic
    /// /seo-payment-ticket/edit/123 -> page-seo-payment-ticket-edit
    /// /user-manager/create -> page-user-manager-create
    /// </summary>
    private string GetRouteId()
    {
        var currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        var normalizedUrl = currentUrl.Split('?')[0].Split('#')[0].Trim();
        
        if (string.IsNullOrEmpty(normalizedUrl) || normalizedUrl == "/")
        {
            return "page-default";
        }
        
        // Convert route to CSS-friendly ID
        var routeId = normalizedUrl.TrimStart('/');
        
        // Remove route parameters (e.g., /edit/{Id} -> /edit)
        // Remove numeric IDs and GUIDs at the end
        routeId = System.Text.RegularExpressions.Regex.Replace(routeId, @"/\d+$", "");
        routeId = System.Text.RegularExpressions.Regex.Replace(routeId, @"/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Replace slashes with dashes
        routeId = routeId.Replace("/", "-");
        
        // Remove special characters and convert to lowercase
        routeId = System.Text.RegularExpressions.Regex.Replace(routeId, @"[^a-zA-Z0-9\-]", "-");
        routeId = routeId.ToLowerInvariant();
        
        // Remove consecutive dashes
        routeId = System.Text.RegularExpressions.Regex.Replace(routeId, @"-+", "-");
        routeId = routeId.Trim('-');
        
        return $"page-{routeId}";
    }
    
    /// <summary>
    /// </summary>
    /// <returns></returns>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Initialize route ID
        CurrentRouteId = GetRouteId();
        
        // Subscribe to location changed event to check authorization on navigation
        if (!_isLocationChangedSubscribed)
        {
            NavigationManager.LocationChanged += OnLocationChanged;
            _isLocationChangedSubscribed = true;
        }

        if (!_isAuthStateChangedSubscribed)
        {
            var provider = ServiceProvider.GetService<AuthenticationStateProvider>();
            if (provider != null)
            {
                provider.AuthenticationStateChanged += OnAuthenticationStateChanged;
                UnauthorizedNotifier.Detected += OnUnauthorizedDetected;
                _isAuthStateChangedSubscribed = true;
            }
        }
    }
    
    /// <summary>
    /// Handle location changed event to check authorization
    /// </summary>
    private async void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        // Update route ID when location changes
        CurrentRouteId = GetRouteId();
        
        // Check authorization when location changes
        await CheckAuthorizationAsync();
        
        // Ensure menus are loaded if they're empty (for direct URL navigation)
        if ((Menus == null || !Menus.Any()) && !string.IsNullOrEmpty(Role))
        {
            await LoadMenusAsync();
        }
        
        // Force UI update after location change to ensure menu is visible
        await InvokeAsync(() => StateHasChanged());
        
        // Sidebar state has a single JavaScript owner. Re-sync after Blazor has
        // rendered the new active NavLink; do not run delayed restore races here.
        if (JSRuntime != null)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("seafoodSidebar.handleRouteChange");
            }
            catch
            {
                // Optional client-side helpers must not block navigation.
            }
        }
    }
    
    /// <summary>
    /// Check authorization for current URL
    /// </summary>
    private async Task CheckAuthorizationAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(Role) || AuthorizationService == null)
            {
                return;
            }
            
            var currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            var normalizedUrl = currentUrl.Split('?')[0].Split('#')[0].Trim();
            
            // Skip check for login, unauthorized, and root pages
            if (!string.IsNullOrEmpty(normalizedUrl) &&
                normalizedUrl != "login" &&
                normalizedUrl != "unauthorized" &&
                normalizedUrl != "")
            {
                if (!await AuthorizationService.IsUrlAllowedAsync(normalizedUrl))
                {
                    NavigationManager.NavigateTo("/unauthorized");
                }
            }
        }
        catch
        {
            // Ignore errors during authorization check
        }
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _ = InvokeAsync(async () =>
        {
            var authState = await task;
            if (!(authState.User.Identity?.IsAuthenticated ?? false))
            {
                await HandleSessionExpiredRedirectAsync();
            }
        });
    }

    private void OnUnauthorizedDetected()
    {
        _ = InvokeAsync(async () =>
        {
            UerManagerService?.Logout();
            await HandleSessionExpiredRedirectAsync();
        });
    }

    private Task HandleSessionExpiredRedirectAsync()
    {
        var currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        var normalizedUrl = currentUrl.Split('?')[0].Split('#')[0].Trim().ToLowerInvariant();
        if (!IsPublicRoute(normalizedUrl))
        {
            NavigationManager.NavigateTo("/login", true);
        }

        return Task.CompletedTask;
    }

    private static bool IsPublicRoute(string normalizedUrl)
    {
        return string.IsNullOrEmpty(normalizedUrl)
            || normalizedUrl == "login"
            || normalizedUrl == "unauthorized"
            || normalizedUrl == "privacy"
            || normalizedUrl == "terms";
    }
    
    /// <summary>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {

        await base.OnInitializedAsync();

        await Task.Delay(10);

        // Clear menus before loading to ensure fresh state
        Menus?.Clear();
        Menus = new List<MenuItem>();
       
        AuthenticationState auth;
        try
        {
            auth = await AuthState;
        }
        catch
        {
            _isPageReady = true;
            NavigationManager.NavigateTo("/login");
            return;
        }
        if (auth.User.Identity?.IsAuthenticated != true)
        {
            _isPageReady = true;
            NavigationManager.NavigateTo("/login");
            return;
        }

        UserName = auth.User.Identity.Name!;
        NewPassword.UserName = UserName;

        try
        {
            var provider = ServiceProvider.GetService<AuthenticationStateProvider>();
            Role = await ((ApiAuthenticationStateProvider)provider!).GetUserRolesAsync();
            var currentUserId = await ((ApiAuthenticationStateProvider)provider!).GetCurrentUserId();
            if (!int.TryParse(currentUserId, out var parsedUserId))
                throw new InvalidOperationException("Authenticated user ID is missing or invalid.");

            CurrentUserId = parsedUserId;
            
            // Check authorization for current URL
            var currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            var normalizedUrl = currentUrl.Split('?')[0].Split('#')[0].Trim();
            
            // Skip check for login, unauthorized, and root pages
            if (!string.IsNullOrEmpty(normalizedUrl) && 
                normalizedUrl != "login" && 
                normalizedUrl != "unauthorized" &&
                normalizedUrl != "")
            {
                if (AuthorizationService != null && !await AuthorizationService.IsUrlAllowedAsync(normalizedUrl))
                {
                    _isPageReady = true;
                    NavigationManager.NavigateTo("/unauthorized");
                    return;
                }
            }

            // Top-bar flags + impersonation banner: MUST be set before the root-page
            // redirect below, which returns early. Otherwise a fresh login / a
            // NavigateTo("/", forceLoad:true) (e.g. right after starting/stopping
            // impersonation) always lands on "/" first and bails out before ever
            // reaching this code, so the banner (and Soạn thông báo/AI Chat) never
            // appear until the user manually reloads a non-root page.
            CanComposeNotification = await UserPermissionService.HasAsync("notification.notification-admin");
            CanUseAiChat = await UserPermissionService.HasAsync("ai-chat.agent");

            if (ServiceProvider.GetService<AuthenticationStateProvider>() is ApiAuthenticationStateProvider impProvider)
            {
                ImpersonatedUserName = await impProvider.GetImpersonatedUserNameAsync();
            }

            // If user is on root page, redirect to first allowed page
            if (normalizedUrl == "" && AuthorizationService != null)
            {
                var defaultPage = await AuthorizationService.GetDefaultPageAsync();
                if (!string.IsNullOrEmpty(defaultPage))
                {
                    _isPageReady = true;
                    NavigationManager.NavigateTo($"/{defaultPage}");
                    return;
                }
            }

            // Load menus based on user permissions
            await LoadMenusAsync();
            _isPageReady = true;
        }
        catch (Exception)
        {
            _isPageReady = true;
            NavigationManager.NavigateTo("/login");
        }

        // Force UI update after menu is loaded
        StateHasChanged();
    }
    
    /// <summary>
    /// Called when parameters are set or updated
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        
        // Ensure menus are loaded if they're empty (for direct URL navigation)
        if ((Menus == null || !Menus.Any()) && !string.IsNullOrEmpty(Role))
        {
            await LoadMenusAsync();
            StateHasChanged();
        }
    }
    
    /// <summary>
    /// Load menus from PageRegistry filtered by the current user's permissions.
    /// Single source of truth — legacy hardcoded role menus removed.
    /// </summary>
    private async Task LoadMenusAsync()
    {
        if (string.IsNullOrEmpty(Role))
        {
            return;
        }

        Menus = await AuthorizationService.BuildMenusAsync();
    }

    private static string GetSidebarMenuId(MenuItem menu, bool mobile)
    {
        var seed = SidebarMenuNavigation.ResolveDirectUrl(menu)
            ?? menu.Items?.FirstOrDefault()?.Url
            ?? menu.Text;
        var normalized = System.Text.RegularExpressions.Regex.Replace(seed ?? menu.Text, @"[^a-zA-Z0-9]+", "-")
            .Trim('-')
            .ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized)) normalized = "menu";
        return $"sidebar-{(mobile ? "mobile-" : string.Empty)}{normalized}";
    }

    private bool IsTopMenuActive(MenuItem menu)
    {
        var currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri)
            .Split('?')[0]
            .Split('#')[0]
            .Trim('/');

        var directUrl = SidebarMenuNavigation.ResolveDirectUrl(menu);
        if (IsTopMenuRouteMatch(currentUrl, directUrl))
        {
            return true;
        }

        return menu.Items?.Any(item => IsTopMenuRouteMatch(currentUrl, item.Url)) == true;
    }

    private static bool IsTopMenuRouteMatch(string currentUrl, string? menuUrl)
    {
        if (string.IsNullOrWhiteSpace(menuUrl))
        {
            return false;
        }

        var normalizedMenuUrl = menuUrl.TrimStart('/').Split('?')[0].Split('#')[0].Trim('/');
        return currentUrl.Equals(normalizedMenuUrl, StringComparison.OrdinalIgnoreCase)
            || currentUrl.StartsWith(normalizedMenuUrl + "/", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetTopMenuId(MenuItem menu)
    {
        var seed = SidebarMenuNavigation.ResolveDirectUrl(menu)
            ?? menu.Items?.FirstOrDefault()?.Url
            ?? menu.Text;
        var normalized = System.Text.RegularExpressions.Regex.Replace(seed ?? menu.Text, @"[^a-zA-Z0-9]+", "-")
            .Trim('-')
            .ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized)) normalized = "menu";
        return $"top-menu-{normalized}";
    }

    /// <summary>
    /// Called after component has rendered
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && JSRuntime != null)
        {
            try
            {
                var clientUserAgent = await JSRuntime.InvokeAsync<string>("seafoodAudit.getUserAgent");
                RequestClient.SetClientUserAgent(clientUserAgent);
            }
            catch
            {
                // Device detection remains available from the API request headers when JS is unavailable.
            }
        }
        
        // Initialize the idempotent sidebar controller after render.
        if (Menus != null && Menus.Any() && JSRuntime != null)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("seafoodSidebar.init");
            }
            catch
            {
                // Ignore JS errors
            }
        }
    }
    
    private Task SetLang(string cultureName)
    {
        if (SelectedCulture != cultureName)
        {
            var uri = new Uri(NavigationManager.Uri).GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped);
            var query = $"?culture={Uri.EscapeDataString(cultureName)}&redirectUri={Uri.EscapeDataString(uri)}";
            NavigationManager.NavigateTo("/Culture/SetCulture" + query, forceLoad: true);
        }

        return Task.CompletedTask;
    }
    
    private async Task ShowModal()
    {
        await Modal.Show();
    }

    /// <summary>
    /// Show change password modal
    /// </summary>
    private async Task ShowChangePasswordModal()
    {
        await Modal.Show();
    }

    /// <summary>
    /// Handle logout action
    /// </summary>
    private Task OnLogout()
    {
        return Logout();
    }

    private async Task<bool> OnChangePasswordAsync()
    {
        var res = await UerManagerService!.SetNewPasswordAsync(NewPassword);
        if (res){
            await Toast.Success("Success", "Đổi mật khẩu thành công");
            return true;
        }
        else{
            await Toast.Error("Error", "Đổi mật khẩu thất bại");
            return false;
        }

    }
    private Task Logout()
    {
        UerManagerService!.Logout();
        NavigationManager.NavigateTo("/login");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_isLocationChangedSubscribed)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
            _isLocationChangedSubscribed = false;
        }

        if (_isAuthStateChangedSubscribed)
        {
            var provider = ServiceProvider.GetService<AuthenticationStateProvider>();
            if (provider != null)
            {
                provider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
            }

            UnauthorizedNotifier.Detected -= OnUnauthorizedDetected;
            _isAuthStateChangedSubscribed = false;
        }
    }
}
