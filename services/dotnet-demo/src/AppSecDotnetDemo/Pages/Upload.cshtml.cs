using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using AppSecDotnetDemo.Configuration;
using AppSecDotnetDemo.Data;
using AppSecDotnetDemo.Services;

namespace AppSecDotnetDemo.Pages;

public class UploadModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly DemoSettings _demoSettings;

    public UploadModel(AppDbContext context, IWebHostEnvironment env, IOptions<DemoSettings> demoSettings)
    {
        _context = context;
        _env = env;
        _demoSettings = demoSettings.Value ?? new DemoSettings();
    }

    [BindProperty]
    public IFormFile? AssemblyFile { get; set; }

    [BindProperty]
    public string? AssemblyName { get; set; }

    public string? Message { get; set; }
    public bool IsError { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (AssemblyFile == null || AssemblyFile.Length == 0)
        {
            Message = "Please select a file to upload.";
            IsError = true;
            return Page();
        }

        if (!AssemblyFile.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            Message = "Only .dll files are allowed.";
            IsError = true;
            return Page();
        }

        // Save to assemblies directory
        var assembliesPath = AssemblyPathHelper.Resolve(_env.ContentRootPath, _demoSettings.AssembliesPath);
        Directory.CreateDirectory(assembliesPath);

        var filePath = Path.Combine(assembliesPath, AssemblyFile.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await AssemblyFile.CopyToAsync(stream);
        }

        var assemblyInfo = new AssemblyInfo
        {
            Name = AssemblyName ?? Path.GetFileNameWithoutExtension(AssemblyFile.FileName),
            FileName = AssemblyFile.FileName,
            FilePath = filePath,
            UploadedAt = DateTime.UtcNow
        };

        _context.Assemblies.Add(assemblyInfo);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Index");
    }
}
