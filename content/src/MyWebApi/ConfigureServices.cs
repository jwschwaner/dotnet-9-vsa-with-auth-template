using Serilog;
using MyWebApi.Authentication.Services;

namespace MyWebApi;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.AddSerilog();
        builder.AddScalarDocs();
        builder.AddDatabase();
    builder.Services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);
    builder.AddAuthentication();
    }

    private static void AddScalarDocs(this WebApplicationBuilder builder)
    {
        // Register Microsoft OpenAPI generation and Scalar transformers
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();
    }

    private static void AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });
    }

    private static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
        });
    }

    private static void AddAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Identity"));
        });

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedAccount = true;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/auth/login";
            options.LogoutPath = "/auth/logout";
            options.AccessDeniedPath = "/auth/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        builder.Services.AddAuthorization();

        // Register email service
        builder.Services.AddScoped<IEmailService, EmailService>();
    }
}