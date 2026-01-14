using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AppSecDotnetDemo.Data;
using AppSecDotnetDemo.Services;

namespace AppSecDotnetDemo.Pages;

public class DemoModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly DllExecutorService _executor;

    public DemoModel(AppDbContext context, DllExecutorService executor)
    {
        _context = context;
        _executor = executor;
    }

    [BindProperty]
    public string? Scenario { get; set; }

    [BindProperty]
    public string? Parameter { get; set; }

    public ExecutionResult? Result { get; set; }
    public string? CurrentScenario { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        CurrentScenario = Scenario;

        // Find the appropriate assembly
        var assemblyName = Scenario switch
        {
            "baseline" => "LegitimatePlugin",
            "init-db" => "TaintedInputPlugin",
            "sql-safe" => "TaintedInputPlugin",
            "sql-attack" => "TaintedInputPlugin",
            "cmd-safe" => "TaintedInputPlugin",
            "cmd-attack" => "TaintedInputPlugin",
            _ => null
        };

        if (assemblyName == null)
        {
            Result = new ExecutionResult
            {
                Success = false,
                Error = "Unknown scenario"
            };
            return Page();
        }

        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Name == assemblyName);

        if (assembly == null)
        {
            Result = new ExecutionResult
            {
                Success = false,
                Error = $"Assembly '{assemblyName}' not found. Please ensure plugins are seeded."
            };
            return Page();
        }

        // Execute based on scenario - Parameter comes from form (user input) for taint tracking!
        var (typeName, methodName) = Scenario switch
        {
            "baseline" => ("LegitimatePlugin.DataProcessor", "Process"),
            "init-db" => ("TaintedInputPlugin.Attacks", "InitializeDatabase"),
            "sql-safe" => ("TaintedInputPlugin.Attacks", "SearchUsers"),
            "sql-attack" => ("TaintedInputPlugin.Attacks", "SearchUsers"),
            "cmd-safe" => ("TaintedInputPlugin.Attacks", "ExecuteCommand"),
            "cmd-attack" => ("TaintedInputPlugin.Attacks", "ExecuteCommand"),
            _ => ((string?)null, (string?)null)
        };

        if (typeName == null || methodName == null)
        {
            Result = new ExecutionResult
            {
                Success = false,
                Error = "Invalid scenario configuration"
            };
            return Page();
        }

        // Use Parameter from form - this is user input that Dynatrace can track!
        Result = _executor.ExecuteMethod(assembly.FilePath, typeName, methodName, Parameter);
        return Page();
    }
}
