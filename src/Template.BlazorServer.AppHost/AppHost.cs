// ReSharper disable StringLiteralTypo
var builder = DistributedApplication.CreateBuilder(args);

builder.AddDotnetProject("blazorserver", "../Template.BlazorServer.Host/Template.BlazorServer.Host.csproj")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
