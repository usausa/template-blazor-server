// ReSharper disable StringLiteralTypo
var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Template_BlazorServer_Host>("blazorserver")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
