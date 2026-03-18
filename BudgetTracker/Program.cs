using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.Repositories.BillRepository;
using BudgetTracker.Repositories.CategoryRepository;
using BudgetTracker.Repositories.IncomeRepository;
using BudgetTracker.Repositories.MonthlyTotalRepository;
using BudgetTracker.Repositories.UserRepository;
using BudgetTracker.Services.Bill;
using BudgetTracker.Services.Category;
using BudgetTracker.Services.EmailSender;
using BudgetTracker.Services.History;
using BudgetTracker.Services.Income;
using BudgetTracker.Services.User;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

// Add globalization settings for Spanish (Argentina)
var culture = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");
builder.Services.AddDbContext<BudgettrackerdbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        //Enables the retry strategy for transient failures
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null
        );
    }));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<BudgettrackerdbContext>()
    .AddDefaultTokenProviders();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
});

// Configure token lifespan for email confirmation and password reset
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(2);
});

// Configure authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    // Specify where to redirect un-authenticated users
    options.LoginPath = "/Home/Login";
    options.LogoutPath = "/Home/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";

    // Specify the name of the auth cookie
    options.Cookie.Name = "userLoged";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
});

// Configure DataProtection for Render.com
if (!builder.Environment.IsDevelopment())
{
    // For Render.com, we need to store DataProtection keys in a persistent directory
    // Render uses volumes for persistent storage
    var keyPath = Environment.GetEnvironmentVariable("DATA_PROTECTION_PATH") 
        ?? "/tmp/data_protection_keys";
    
    if (!Directory.Exists(keyPath))
    {
        Directory.CreateDirectory(keyPath);
    }
    
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyPath));
}

// Register Repositories
builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IMonthlyTotalRepository, MonthlyTotalRepository>();

// Register Services
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();

// Set the default culture
var defaultCulture = new CultureInfo("es-AR");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
