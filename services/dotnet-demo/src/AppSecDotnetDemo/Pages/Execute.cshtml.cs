using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppSecDotnetDemo.Data;
using AppSecDotnetDemo.Services;

namespace AppSecDotnetDemo.Pages;

public class ExecuteModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly DllExecutorService _executor;

    public ExecuteModel(AppDbContext context, DllExecutorService executor)
    {
        _context = context;
        _executor = executor;
    }

    public List<SelectListItem> AssemblyOptions { get; set; } = new();

    [BindProperty]
    public int SelectedAssemblyId { get; set; }

    [BindProperty]
    public string TypeName { get; set; } = string.Empty;

    [BindProperty]
    public string MethodName { get; set; } = string.Empty;

    [BindProperty]
    public string? MethodParameter { get; set; }

    public ExecutionResult? Result { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAssemblies();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAssemblies();

        var assembly = await _context.Assemblies.FindAsync(SelectedAssemblyId);
        if (assembly == null)
        {
            Result = new ExecutionResult
            {
                Success = false,
                Error = "Assembly not found"
            };
            return Page();
        }

        Result = _executor.ExecuteMethod(assembly.FilePath, TypeName, MethodName, MethodParameter);
        return Page();
    }

    private async Task LoadAssemblies()
    {
        var assemblies = await _context.Assemblies.ToListAsync();
        AssemblyOptions = assemblies.Select(a => new SelectListItem
        {
            Value = a.Id.ToString(),
            Text = $"{a.Name} ({a.FileName})"
        }).ToList();
    }
}
