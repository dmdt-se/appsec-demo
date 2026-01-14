namespace AppSecDotnetDemo.Configuration;

public class DemoSettings
{
    public string AppName { get; set; } = "AppSec .NET Demo";
    public bool ShowAdminMenu { get; set; } = false;
    public bool AutoSeedDatabase { get; set; } = true;
    public string? AssembliesPath { get; set; }
    public string? PathBase { get; set; }
}
