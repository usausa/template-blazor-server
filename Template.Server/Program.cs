using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Hosting.WindowsServices;

using MudBlazor;
using MudBlazor.Services;

using PdfSharpCore.Fonts;

using Serilog;

using Smart.AspNetCore;
using Smart.AspNetCore.ApplicationModels;
using Smart.AspNetCore.Components;

using Template.Components.Json;
using Template.Components.Reports;
using Template.Server.Components.Authentication;

#pragma warning disable CA1812

//--------------------------------------------------------------------------------
// Configure builder
//--------------------------------------------------------------------------------
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

// Configure builder
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
builder.Host
    .ConfigureLogging((_, logging) =>
    {
        logging.ClearProviders();
    })
    .UseSerilog((hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
    });

// Route
builder.Services.Configure<RouteOptions>(options =>
{
    options.AppendTrailingSlash = true;
});

// Filter
builder.Services.AddTimeLogging(options =>
{
    options.Threshold = 5000;
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
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// API
builder.Services
    .AddControllers(options =>
    {
        options.Filters.AddTimeLogging();
        options.Conventions.Add(new LowercaseControllerModelConvention());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
    });

// Swagger
builder.Services.AddSwaggerGen();

// Add Authentication component.
builder.Services.Configure<CookieAuthenticationSetting>(builder.Configuration.GetSection("Authentication"));
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<CookieAuthenticationStateProvider>());

// PDF
GlobalFontSettings.FontResolver = new FontResolver(Directory.GetCurrentDirectory(), FontNames.Gothic, new Dictionary<string, string>
{
    { FontNames.Gothic, "ipaexg.ttf" }
});

//--------------------------------------------------------------------------------
// Configure the HTTP request pipeline
//--------------------------------------------------------------------------------
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// HTTPS redirection
app.UseHttpsRedirection();

// Static files
app.UseStaticFiles();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Authentication
app.UseAuthentication();
app.UseAuthorization();

// Routing
app.UseRouting();

// API
app.MapControllers();

// Blazor
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Run
app.Run();
