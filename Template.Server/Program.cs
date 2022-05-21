using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using Microsoft.AspNetCore.Components.Authorization;

using MudBlazor;
using MudBlazor.Services;

using Serilog;

using Template.Server.Components.Authentication;

#pragma warning disable CA1812

//--------------------------------------------------------------------------------
// Configure builder
//--------------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

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

// API TODO
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        //options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
    });

// Swagger
builder.Services.AddSwaggerGen();

// Add Authentication component.
builder.Services.Configure<CookieAuthenticationSetting>(builder.Configuration.GetSection("Authentication"));
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<CookieAuthenticationStateProvider>());

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
