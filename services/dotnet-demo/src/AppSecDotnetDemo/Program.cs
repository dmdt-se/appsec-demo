using Microsoft.EntityFrameworkCore;
using AppSecDotnetDemo.Configuration;
using AppSecDotnetDemo.Data;
using AppSecDotnetDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure DemoSettings
builder.Services.Configure<DemoSettings>(builder.Configuration.GetSection("DemoSettings"));
var demoSettings = builder.Configuration.GetSection("DemoSettings").Get<DemoSettings>() ?? new DemoSettings();

var assembliesPath = AssemblyPathHelper.Resolve(builder.Environment.ContentRootPath, demoSettings.AssembliesPath);
Directory.CreateDirectory(assembliesPath);
var databasePath = Path.Combine(assembliesPath, "appsec-demo.db");

// Add DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

// Add DllExecutorService
builder.Services.AddScoped<DllExecutorService>();

var app = builder.Build();

if (!string.IsNullOrWhiteSpace(demoSettings.PathBase))
{
    app.UsePathBase(demoSettings.PathBase);
}

// Ensure database is created and seed plugins
if (demoSettings.AutoSeedDatabase)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Seed demo plugins if not already present
    var legitimatePluginFileName = "LegitimatePlugin.dll";
    var legitimatePluginPath = Path.GetFullPath(Path.Combine(assembliesPath, legitimatePluginFileName));
    if (!db.Assemblies.Any(a => a.FileName == legitimatePluginFileName) && File.Exists(legitimatePluginPath))
    {
        var legitimatePlugin = new AssemblyInfo
        {
            Name = "LegitimatePlugin",
            FileName = legitimatePluginFileName,
            FilePath = legitimatePluginPath,
            UploadedAt = DateTime.UtcNow
        };
        db.Assemblies.Add(legitimatePlugin);
        db.SaveChanges();
        Console.WriteLine($"Seeded assembly: {legitimatePlugin.Name}");
    }

    // Seed TaintedInputPlugin
    var taintedPluginFileName = "TaintedInputPlugin.dll";
    var taintedPluginPath = Path.GetFullPath(Path.Combine(assembliesPath, taintedPluginFileName));
    if (!db.Assemblies.Any(a => a.FileName == taintedPluginFileName) && File.Exists(taintedPluginPath))
    {
        var taintedPlugin = new AssemblyInfo
        {
            Name = "TaintedInputPlugin",
            FileName = taintedPluginFileName,
            FilePath = taintedPluginPath,
            UploadedAt = DateTime.UtcNow
        };
        db.Assemblies.Add(taintedPlugin);
        db.SaveChanges();
        Console.WriteLine($"Seeded assembly: {taintedPlugin.Name}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
