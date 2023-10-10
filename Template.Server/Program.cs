using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.FeatureManagement;

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

using Smart.Data.Accessor;

using Template.Accessor;

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

// Log
builder.Logging.ClearProviders();
builder.Host
    .UseSerilog((hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
    });

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpLogging(options =>
    {
        //options.LoggingFields = HttpLoggingFields.All | HttpLoggingFields.RequestQuery;
        options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |
                                HttpLoggingFields.RequestQuery |
                                HttpLoggingFields.ResponsePropertiesAndHeaders;
    });
}

// Add framework Services.
builder.Services.AddHttpContextAccessor();

// TODO setting

// Feature management
builder.Services.AddFeatureManagement();

// Route
builder.Services.Configure<RouteOptions>(options =>
{
    options.AppendTrailingSlash = true;
});

// XForward
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Do not restrict to local network/proxy
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
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
builder.Services.AddExceptionStatus();
builder.Services.AddTimeLogging(options =>
{
    options.Threshold = 10_000;
});

builder.Services
    .AddControllers(options =>
    {
        options.Conventions.Add(new LowercaseControllerModelConvention());
        options.Filters.AddExceptionLogging();
        options.Filters.AddExceptionStatus();
        options.Filters.AddTimeLogging();
        options.ModelBinderProviders.Insert(0, new AccountModelBinderProvider());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        options.JsonSerializerOptions.Converters.Add(new Template.Components.Json.DateTimeConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// Swagger
if (!builder.Environment.IsProduction())
{
    builder.Services.AddSwaggerGen();
}

// Error handler
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions.Add("nodeId", Environment.MachineName);
    };
});

// TODO SignalR

// Compress
//builder.Services.AddRequestDecompression();
//builder.Services.AddResponseCompression(options =>
//{
//    // Default false (for CRIME and BREACH attacks)
//    options.EnableForHttps = true;
//    options.Providers.Add<GzipCompressionProvider>();
//    options.MimeTypes = new[] { MediaTypeNames.Application.Json };
//});
//builder.Services.Configure<GzipCompressionProviderOptions>(options =>
//{
//    options.Level = CompressionLevel.Fastest;
//});

// Health
builder.Services.AddHealthChecks();

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

// Log
if (app.Environment.IsDevelopment())
{
    // Serilog
    app.UseSerilogRequestLogging(options =>
    {
        options.IncludeQueryInRequestPath = true;
    });

    // HTTP log
    app.UseWhen(
        c => c.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
        b => b.UseHttpLogging());
}

// Forwarded headers
app.UseForwardedHeaders();

// Error handler
app.UseWhen(
    c => c.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
    b => b.UseExceptionHandler());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// HTTPS redirection
app.UseHttpsRedirection();

// Static files
app.UseStaticFiles();

// Routing
app.UseRouting();

// Swagger
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Metrics
app.UseHttpMetrics();

// Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

// Compress
//app.UseWhen(
//    c => c.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
//    b =>
//    {
//        b.UseResponseCompression();
//        b.UseRequestDecompression();
//    });

// Blazor
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// API
app.MapControllers();

// TODO SignalR

// Metrics
app.MapMetrics();

// Health
app.MapHealthChecks("/health");

// Run
app.Run();
