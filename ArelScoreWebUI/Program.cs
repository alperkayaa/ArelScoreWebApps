using ArelScoreWebUI.Middleware;
using ArelScoreWebUI.Models;
using Microsoft.EntityFrameworkCore;

var webApplicationOptions = new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args,
};

var builder = WebApplication.CreateBuilder(webApplicationOptions);

// Razor Pages ve MVC Controller desteği
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Oturum için gerekli önbellek servisi (önce eklenmeli)
builder.Services.AddDistributedMemoryCache();

// Session servislerini ekle
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // 20 dakika işlem yapılmazsa session silinir
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// CORS (istersen ileride açarsın)
builder.Services.AddCors();

// Veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultLocalConnectionString")));

builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Orta katman ayarları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Session middleware burada etkinleştirilir

//app.UseAuthorization();
//app.UseAuthentication();
app.UseCors();

// Açılışta login değil anasayfa gelsin
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
