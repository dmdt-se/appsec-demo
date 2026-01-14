namespace AppSecDotnetDemo.Data;

public class AssemblyInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
