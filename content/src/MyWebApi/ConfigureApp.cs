using Serilog;
using Scalar.AspNetCore;
using MyWebApi.Authentication.Extensions;

namespace MyWebApi;

public static class ConfigureApp
{
    public static async Task Configure(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(endpointPrefix: "/");
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();
        await app.EnsureDatabaseCreated();
        await app.SeedIdentityData();
    }

    private static async Task EnsureDatabaseCreated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        var mainDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await mainDb.Database.MigrateAsync();

        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await identityDb.Database.MigrateAsync();
    }
}