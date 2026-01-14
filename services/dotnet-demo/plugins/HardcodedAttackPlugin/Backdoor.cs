using System.Diagnostics;
using System.Text;

namespace HardcodedAttackPlugin;

/// <summary>
/// SECURITY DEMONSTRATION ONLY - This plugin contains simulated attack patterns
/// designed to trigger Dynatrace runtime application security detections.
/// DO NOT use in production environments.
/// </summary>
public class Backdoor
{
    public string Run()
    {
        var results = new StringBuilder();
        results.AppendLine("=== Malicious Plugin Execution Started ===");
        results.AppendLine();

        // Attack Pattern 1: Process Creation - Triggers Dynatrace
        results.AppendLine("[1] Shell Execution Attack:");
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"whoami && hostname\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            results.AppendLine($"    Command output: {output.Trim()}");
        }
        catch (Exception ex)
        {
            results.AppendLine($"    Shell exec failed (expected on Windows): {ex.Message}");
        }

        // Attack Pattern 2: Sensitive File Access - Triggers Dynatrace
        results.AppendLine();
        results.AppendLine("[2] Sensitive File Access Attack:");
        try
        {
            var passwdContent = File.ReadAllText("/etc/passwd");
            var lineCount = passwdContent.Split('\n').Length;
            results.AppendLine($"    Read /etc/passwd: {lineCount} lines");
            results.AppendLine($"    First line: {passwdContent.Split('\n')[0]}");
        }
        catch (Exception ex)
        {
            results.AppendLine($"    File access failed (expected on Windows): {ex.Message}");
        }

        // Attack Pattern 3: Outbound Network Connection - Triggers Dynatrace
        results.AppendLine();
        results.AppendLine("[3] Outbound Network Attack:");
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = client.GetAsync("http://example.com").Result;
            results.AppendLine($"    Network call to example.com: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            results.AppendLine($"    Network call failed: {ex.Message}");
        }

        results.AppendLine();
        results.AppendLine("=== Malicious Plugin Execution Complete ===");
        results.AppendLine();
        results.AppendLine("Dynatrace should now show security alerts for:");
        results.AppendLine("  - Unknown assembly loaded via reflection");
        results.AppendLine("  - Process creation from web application context");
        results.AppendLine("  - Sensitive file access (/etc/passwd)");
        results.AppendLine("  - Outbound network connection");

        return results.ToString();
    }
}
