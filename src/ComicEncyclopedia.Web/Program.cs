using ComicEncyclopedia.Business.Helpers;
using ComicEncyclopedia.Business.Services;
using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Data.Context;
using ComicEncyclopedia.Data.Entities;
using ComicEncyclopedia.Data.Repositories;
using ComicEncyclopedia.Data.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 200_000_000;
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200_000_000;
    options.ValueLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

var dbProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=ComicEncyclopedia.db";

var healthChecksBuilder = builder.Services.AddHealthChecks();

if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
    || dbProvider.Equals("AzureSql", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
    healthChecksBuilder.AddSqlServer(connectionString, name: "database");
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
    healthChecksBuilder.AddSqlite(connectionString, name: "database");
}

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

builder.Services.AddSingleton<ITextSanitizer, TextSanitizer>();
builder.Services.AddSingleton<IComicFilter, ComicFilter>();
builder.Services.AddSingleton<IComicSorter, ComicSorter>();
builder.Services.AddSingleton<IComicGrouper, ComicGrouper>();

builder.Services.AddSingleton<ICsvParser, CsvParser>();
builder.Services.AddSingleton<IComicRepository, ComicRepository>();

builder.Services.AddScoped<IComicService, ComicService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISearchListService, SearchListService>();
builder.Services.AddScoped<IFlagService, FlagService>();
builder.Services.AddHttpClient<IDatasetUpdateService, DatasetUpdateService>();

builder.Services.AddHttpClient<IBookCoverService, BookCoverService>();

// send email
builder.Services.AddHttpClient<ISendGridEmailService, SendGridEmailService>();
builder.Services.AddHttpClient<IMailjetEmailService, MailjetEmailService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        context.Database.EnsureCreated();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "Staff", "Public" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (await userManager.FindByEmailAsync("admin@fantasybzaar.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@fantasybzaar.com",
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await userManager.FindByEmailAsync("staff@fantasybzaar.com") == null)
        {
            var staff = new ApplicationUser
            {
                UserName = "staff",
                Email = "staff@fantasybzaar.com",
                FirstName = "Staff",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            await userManager.CreateAsync(staff, "Staff123!");
            await userManager.AddToRoleAsync(staff, "Staff");
        }

        Console.WriteLine("Database initialized successfully!");
    }
    catch (Exception ex)
    {
        // return error
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
