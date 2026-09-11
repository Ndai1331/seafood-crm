using Domain.Identity.Roles;
using Domain.Identity.Users;
using Fido2NetLib;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SqlServ4r.EntityFramework;
using System.Text;
using Application.Common;
using Application.Identity.Totp;
using Application.Seafood;
using Contract.Common;
using Contract.Identity.Totp;
using WebApi;
using WebApi.Middlewares;
using WebApi.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Host.UseAutofac();
builder.Services.AddApplication<MyModule>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, WebApi.Authorization.PermissionPolicyProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, WebApi.Authorization.PermissionAuthorizationHandler>();

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Seafood CRM API", Version = "v1" });
    c.CustomSchemaIds(type => type.FullName);
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddSwaggerGenNewtonsoftSupport();

builder.Services.AddDbContext<DreamContext>((_, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyApp"), b =>
    {
        b.MigrationsAssembly("SqlServ4r");
        b.EnableRetryOnFailure(5);
        b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<DreamContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options => options.AddPolicy("ApiCorsPolicy", opt =>
{
    opt.WithOrigins(builder.Configuration.GetSection("Cors").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(_ => true)
        .AllowCredentials();
}));

builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("Minio"));
builder.Services.Configure<TotpOptions>(builder.Configuration.GetSection("Totp"));
builder.Services.AddScoped<IEncryptionService, AesEncryptionService>();
builder.Services.AddScoped<ITotpService, TotpService>();
builder.Services.AddSingleton<MinioService>();
builder.Services.AddScoped<SeafoodDataSeeder>();
builder.Services.AddScoped<SeafoodCatalogService>();
builder.Services.AddScoped<InboundService>();
builder.Services.AddScoped<ProductionService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<FinanceService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<SeafoodDocumentService>();
builder.Services.AddScoped<SeafoodAllocationService>();
builder.Services.AddScoped<SeafoodPartnerService>();
builder.Services.AddScoped<SeafoodImportService>();
builder.Services.AddScoped<SeafoodRawLotService>();
builder.Services.AddScoped<SeafoodTraceabilityService>();

builder.Services.AddFido2(options =>
{
    options.ServerDomain = builder.Configuration["WebAuthn:ServerDomain"];
    options.ServerName = builder.Configuration["WebAuthn:ServerName"] ?? "Seafood CRM";
    options.Origins = builder.Configuration.GetSection("WebAuthn:Origins").Get<HashSet<string>>() ?? new HashSet<string>();
    options.Timeout = builder.Configuration.GetValue<uint?>("WebAuthn:TimeoutMs") ?? 45000;
    options.TimestampDriftTolerance = 300000;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };
    o.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            if (context.Principal?.FindFirst(Application.Identity.Common.TwoFactorTokens.TokenTypeClaim) != null)
            {
                context.Fail("Token is not a session token.");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 0;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
});

builder.Services.AddHttpClient();

var dataProtection = builder.Services.AddDataProtection().SetApplicationName("seafood-crm-api");
var dataProtectionKeyRingPath = builder.Configuration["DataProtection:KeyRingPath"];
var useDatabaseKeyRing = true;
if (!string.IsNullOrWhiteSpace(dataProtectionKeyRingPath))
{
    try
    {
        Directory.CreateDirectory(dataProtectionKeyRingPath);
        dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyRingPath));
        useDatabaseKeyRing = false;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[DataProtection] Cannot use key ring path '{dataProtectionKeyRingPath}' ({ex.Message}). Falling back to the database key ring.");
    }
}
if (useDatabaseKeyRing)
{
    dataProtection.PersistKeysToDbContext<DreamContext>();
}

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.Logger.LogInformation(
    "WebAuthn relying party: domain={ServerDomain} origins={Origins}",
    builder.Configuration["WebAuthn:ServerDomain"],
    string.Join(",", builder.Configuration.GetSection("WebAuthn:Origins").Get<string[]>() ?? []));

using (var seedScope = app.Services.CreateScope())
{
    try
    {
        var db = seedScope.ServiceProvider.GetRequiredService<DreamContext>();
        await db.Database.MigrateAsync();
        var permissionSeeder = seedScope.ServiceProvider.GetRequiredService<Application.Identity.RoleManager.PermissionSeeder>();
        await permissionSeeder.SeedSuperAdminAsync();
        await permissionSeeder.SeedIfEmptyAsync();
        var seafoodSeeder = seedScope.ServiceProvider.GetRequiredService<SeafoodDataSeeder>();
        await seafoodSeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Startup seed failed");
    }
}

if (builder.Configuration["Enable:Swagger"] == "True")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Seafood CRM API");
        c.RoutePrefix = "swagger";
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("ApiCorsPolicy");

var www = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(www)) Directory.CreateDirectory(www);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(www),
    RequestPath = "/StaticFiles"
});

// Keep audit middleware outside the error handler so failed requests are persisted
// with the final HTTP status code instead of the default 200.
app.UseMiddleware<AppHistoryMiddleware>();
app.UseMiddleware<GlobalErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
