namespace AppSecDotnetDemo.Services;

public class ExecutionResult
{
    public bool Success { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}
