using Microsoft.AspNetCore.Components.Authorization;

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
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// HTTPS redirection
app.UseHttpsRedirection();

// Static files
app.UseStaticFiles();

// Authentication
app.UseAuthentication();
app.UseAuthorization();

// Routing
app.UseRouting();

// Blazor
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Run
app.Run();
