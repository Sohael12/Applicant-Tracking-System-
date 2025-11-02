using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stageproject_ATS_AP2025Q2.Components;
using Stageproject_ATS_AP2025Q2.Data;
using Stageproject_ATS_AP2025Q2.Models;
using Stageproject_ATS_AP2025Q2.Services;
using Stageproject_ATS_AP2025Q2.Interfaces;
using Stageproject_ATS_AP2025Q2.Repositories;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Syncfusion.Blazor;
using Microsoft.AspNetCore.Identity.UI.Services;
using IEmailSender = Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;

var builder = WebApplication.CreateBuilder(args);

// ------------------ DATABASE ------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 42))));


// ------------------ IDENTITY ------------------
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ------------------ COOKIE AUTHENTICATION ------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Path = "/";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);        
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.SlidingExpiration = true;
});

// ------------------ PASSWORD & SECURITY RULES ------------------
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password rules
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout rules
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User rules
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

// ------------------ SERVICES ------------------
builder.Services.AddHttpClient("LocalApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7282");
});
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});
builder.Services.AddScoped<IVacancyRepository, VacancyRepository>();
builder.Services.AddScoped<InterviewNoteService>();
builder.Services.AddScoped<StatusHistoryService>();
builder.Services.AddScoped<VacancyService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IStateNotifier, StateNotifier>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<UserRoleService>();
builder.Services.AddScoped<MeetingService>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>(); 
builder.Services.AddScoped<VacancyService>();
builder.Services.AddScoped<VacancyNotificationService>();
builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped<TemplateService>();
builder.Services.AddScoped<MeetingService>();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddScoped<GraphTokenTester>();
builder.Services.AddSingleton<GraphEmailService>();


// Built-in provider for Identity cookies
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<AppUser>>();
builder.Services.AddCascadingAuthenticationState();
// ------------------ Edittool ------------------
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF1cX2hIf0x3QHxbf1x1ZFJMYVpbRHdPMyBoS35Rc0RjWHled3VTRmRaVUV1VEFc");

// ------------------ UI ------------------
builder.Services.AddSyncfusionBlazor();
builder.Services.AddBlazorise(options => { options.Immediate = true; })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

// ------------------ BLAZOR ------------------
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseCookiePolicy();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseCors();
app.MapRazorPages();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
