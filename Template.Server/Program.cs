using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting.WindowsServices;

using MudBlazor.Services;

using PdfSharpCore.Fonts;

using Prometheus;

using Serilog;

using Smart.AspNetCore;
using Smart.AspNetCore.ApplicationModels;
using Smart.Data.Accessor.Extensions.DependencyInjection;
using Smart.Data;

using Template.Components.Reports;
using Template.Components.Security;
using Template.Components.Storage;
using Template.Server.Application;

using Smart.Data.Accessor;

using Template.Accessor;
using Template.Services;

using Smart.AspNetCore.Filters;

#pragma warning disable CA1852

// System
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

//--------------------------------------------------------------------------------
// Configure builder
//--------------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
});

// Service
builder.Host
    .UseWindowsService()
    .UseSystemd();

// Add framework Services.
builder.Services.AddHttpContextAccessor();

// Log
builder.Logging.ClearProviders();
builder.Host
    .UseSerilog((hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
    });

// Route
builder.Services.Configure<RouteOptions>(options =>
{
    options.AppendTrailingSlash = true;
});

// Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMudServices(config =>
{
    // Snackbar
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
});

builder.Services.AddSingleton<IErrorBoundaryLogger, ErrorBoundaryLogger>();

// API
builder.Services.AddExceptionLogging();
builder.Services.AddTimeLogging(options =>
{
    options.Threshold = 10_000;
});
builder.Services.AddSingleton<ExceptionStatusFilter>();

builder.Services
    .AddControllers(options =>
    {
        options.Filters.AddExceptionLogging();
        options.Filters.AddTimeLogging();
        options.Filters.AddService<ExceptionStatusFilter>();
        options.Conventions.Add(new LowercaseControllerModelConvention());
        options.ModelBinderProviders.Insert(0, new AccountModelBinderProvider());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        options.JsonSerializerOptions.Converters.Add(new Template.Components.Json.DateTimeConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// Health
builder.Services.AddHealthChecks();

if (!builder.Environment.IsProduction())
{
    // Swagger
    builder.Services.AddSwaggerGen();
}

// Add Authentication component.
builder.Services.Configure<CookieAuthenticationSetting>(builder.Configuration.GetSection("Authentication"));
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
builder.Services.AddScoped(static p => (ILoginProvider)p.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<LoginManager>();

// Validation
ValidatorOptions.Global
    .UseDisplayName()
    .UseCustomLocalizeMessage();
ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

// PDF
GlobalFontSettings.FontResolver = new FontResolver(Directory.GetCurrentDirectory(), FontNames.Gothic, new Dictionary<string, string>
{
    { FontNames.Gothic, "ipaexg.ttf" }
});

// HTTP
builder.Services.AddHttpClient();

// Data
var connectionStringBuilder = new SqliteConnectionStringBuilder
{
    DataSource = "Data.db",
    Pooling = true,
    Cache = SqliteCacheMode.Shared
};
var connectionString = connectionStringBuilder.ConnectionString;
builder.Services.AddSingleton<IDbProvider>(new DelegateDbProvider(() => new SqliteConnection(connectionString)));
builder.Services.AddSingleton<IDialect>(new DelegateDialect(
    static ex => ex is SqliteException { SqliteErrorCode: 1555 },
    static x => Regex.Replace(x, "[%_]", "[$0]")));
builder.Services.AddDataAccessor();

// Mapper
builder.Services.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(c =>
{
    c.AddProfile<MappingProfile>();
})));

// Security
builder.Services.AddSingleton<DefaultPasswordProviderOptions>();
builder.Services.AddSingleton<IPasswordProvider, DefaultPasswordProvider>();

// Storage
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<FileStorageOptions>>().Value);
builder.Services.AddSingleton<IStorage, FileStorage>();

// Service
builder.Services.AddSingleton<DataService>();

//--------------------------------------------------------------------------------
// Configure the HTTP request pipeline
//--------------------------------------------------------------------------------
var app = builder.Build();

// Prepare
if (!File.Exists(connectionStringBuilder.DataSource))
{
    var accessor = app.Services.GetRequiredService<IAccessorResolver<IDataAccessor>>().Accessor;
    accessor.Create();
}

// Serilog
if (!app.Environment.IsProduction())
{
    app.UseSerilogRequestLogging(options =>
    {
        options.IncludeQueryInRequestPath = true;
    });
}

// Forwarded headers
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// HTTPS redirection
app.UseHttpsRedirection();

// Health
app.UseHealthChecks("/health");

// Metrics
app.UseHttpMetrics();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Static files
app.UseStaticFiles();

// Routing
app.UseRouting();

// Authentication
app.UseAuthentication();
app.UseAuthorization();

// API
app.MapControllers();

// Metrics
app.MapMetrics();

// Blazor
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Run
app.Run();
