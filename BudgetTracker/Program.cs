using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.Repositories.BillRepository;
using BudgetTracker.Repositories.CategoryRepository;
using BudgetTracker.Repositories.IncomeRepository;
using BudgetTracker.Repositories.MonthlyTotalRepository;
using BudgetTracker.Repositories.UserRepository;
using BudgetTracker.Services.Bill;
using BudgetTracker.Services.Category;
using BudgetTracker.Services.History;
using BudgetTracker.Services.Income;
using BudgetTracker.Services.User;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

// Identity hashing services
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

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

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        // Specify where to redirect un-authenticated users
                        options.LoginPath = "/";

                        // Specify the name of the auth cookie.
                        options.Cookie.Name = "userLoged";
                    });

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
