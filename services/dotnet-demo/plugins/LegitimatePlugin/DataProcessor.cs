namespace LegitimatePlugin;

public class DataProcessor
{
    public string Process()
    {
        // Simulate normal business data processing
        var data = new List<string> { "customer_001", "customer_002", "customer_003" };
        var processed = new List<string>();

        foreach (var item in data)
        {
            // Simple transformation - uppercase and add timestamp
            processed.Add($"{item.ToUpper()}_{DateTime.UtcNow:yyyyMMdd}");
        }

        var summary = $"Processed {processed.Count} records successfully.\n";
        summary += $"Results: {string.Join(", ", processed)}\n";
        summary += $"Completed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        return summary;
    }
}
