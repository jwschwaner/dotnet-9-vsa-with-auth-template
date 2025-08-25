global using MyWebApi.Common.Api;
global using MyWebApi.Common.Api.Extensions;
global using MyWebApi.Common.Api.Requests;
global using MyWebApi.Common.Api.Results;
global using MyWebApi.Data;
global using MyWebApi.Data.Types;
global using FluentValidation;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.EntityFrameworkCore;
global using System.Security.Claims;
global using Microsoft.AspNetCore.Identity;
global using MyWebApi.Authentication.Data;
global using MyWebApi.Authentication.Models;
using MyWebApi;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServices();
    var app = builder.Build();
    await app.Configure();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}