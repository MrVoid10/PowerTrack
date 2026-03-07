using Microsoft.EntityFrameworkCore;
using PowerTrack.Data;
using PowerTrack.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// DATABASE
// =====================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();

// =====================================================
// SESSION SUPPORT
// =====================================================

builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromMinutes(30);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});

// =====================================================
// DEPENDENCY INJECTION SERVICES
// =====================================================

// ⭐ Required for AuditService
builder.Services.AddHttpContextAccessor();

// ⭐ Custom Services
builder.Services.AddScoped<AuditService>();
//builder.Services.AddScoped<AdminAuditService>();

// =====================================================
// BUILD APP
// =====================================================

var app = builder.Build();

// =====================================================
// MIDDLEWARE PIPELINE
// =====================================================

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// =====================================================
// ROUTES
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();