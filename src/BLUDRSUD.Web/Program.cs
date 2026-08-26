using BLUDRSUD.Infrastructure;
using BLUDRSUD.Infrastructure.Authorization;
using BLUDRSUD.Infrastructure.Identity;
using BLUDRSUD.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.RateLimiting;

// --- Bootstrap Serilog before the host runs (spec section 1: Serilog) ---
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting BLUD-RSUD host.");

    var builder = WebApplication.CreateBuilder(args);

    // Re-read Serilog configuration now that builder exists.
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();

    // Serilog as the logging provider.
    builder.Host.UseSerilog();

    // Infrastructure: DbContext (Pomelo MySQL) — spec section 1 & 24.
    builder.Services.AddBludInfrastructure(builder.Configuration);

    // ASP.NET Core Identity (spec section 21 & 38) — registered in Web host where ASP.NET Core shared framework is available.
    builder.Services
        .AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.SignIn.RequireConfirmedEmail = false; // demo; enable for production
        })
        .AddEntityFrameworkStores<BludRsudDbContext>()
        .AddDefaultTokenProviders();

    // Cookie-based application auth (separate from Identity cookies configured below).
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS in prod via env
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

    // Authorization: permission-based policies (spec section 21 — Policy + Permission, not Role-only).
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("dashboard:view", p => p.RequireClaim("permission", Permissions.DashboardView));

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            if (HttpMethods.IsPost(context.Request.Method) &&
                context.Request.Path.Equals("/Account/Login", StringComparison.OrdinalIgnoreCase))
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            }

            return RateLimitPartition.GetNoLimiter("excluded");
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // MVC + Razor Pages.
    builder.Services.AddRazorPages()
        .AddMvcOptions(o => o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Nilai tidak boleh kosong."));
    builder.Services.AddControllers();

    // OpenAPI / Swagger (spec section 1).
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // HSTS & HTTPS in production (spec section 38).
    builder.Services.AddHsts(o => o.Preload = true);

    var app = builder.Build();

    // --- Pipeline ---
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseSerilogRequestLogging();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapRazorPages();
    app.MapControllers();

    // --- Migrate + seed on startup (spec section 32). Idempotent & guarded by config flag. ---
    using (var scope = app.Services.CreateScope())
    {
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<BludRsudDbContext>();
        var um = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var rm = sp.GetRequiredService<RoleManager<ApplicationRole>>();

        await db.Database.MigrateAsync();

        if (builder.Configuration.GetValue<bool>("Seed:ApplyOnStartup"))
        {
            try
            {
                await DbSeeder.SeedAsync(db, um, rm, sp.GetRequiredService<ILoggerFactory>());
            }
            catch (Exception seedEx)
            {
                Log.Warning(seedEx, "Startup seeding failed. The app will continue without seed data. Check the database schema and seed configuration.");
            }
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
