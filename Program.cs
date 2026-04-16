using Microsoft.EntityFrameworkCore;
using E_Invoice_system.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();

// Required for session to work reliably
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Bridge for Razor Pages and other services that inject ApplicationDbContext directly
builder.Services.AddScoped<ApplicationDbContext>(p => 
    p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

builder.Services.AddScoped<E_Invoice_system.Services.CurrencyService>();


builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression(); 
app.UseStaticFiles();

string imagesDir = @"D:\netcore\E-Invoice_system\bin\Debug\images";
if (!System.IO.Directory.Exists(imagesDir)) System.IO.Directory.CreateDirectory(imagesDir);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imagesDir),
    RequestPath = "/product-images"
});

string logoDir = @"D:\netcore\E-Invoice_system\bin\Debug\Logo";
if (!System.IO.Directory.Exists(logoDir)) System.IO.Directory.CreateDirectory(logoDir);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(logoDir),
    RequestPath = "/store-logo"
});
app.UseRouting();


app.UseSession();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<E_Invoice_system.Data.ApplicationDbContext>();
    try { 
   
    } catch (Exception ex) { 
        System.Console.WriteLine("DB Patch Error: " + ex.Message); 
    }
}

app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();

app.Run();

