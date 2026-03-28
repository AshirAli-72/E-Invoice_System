using Microsoft.EntityFrameworkCore;
using E_Invoice_system.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// ✅ ADD SESSION
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // timeout control
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ RESPONSE COMPRESSION
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression(); // ✅ BEFORE StaticFiles
app.UseStaticFiles();

string imagesDir = @"D:\netcore\E-Invoice_system\bin\Debug\images";
if (!System.IO.Directory.Exists(imagesDir))
{
    System.IO.Directory.CreateDirectory(imagesDir);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imagesDir),
    RequestPath = "/product-images"
});
app.UseRouting();

// ✅ USE SESSION (Routing ke baad, Authorization se pehle)
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();

app.Run();

