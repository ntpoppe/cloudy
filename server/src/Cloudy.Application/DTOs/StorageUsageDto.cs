namespace Cloudy.Application.DTOs;

public class StorageUsageDto
{
    public long UsedBytes { get; set; }
    public long MaxBytes { get; set; }
    public double UsagePercentage { get; set; }
    public string UsedDisplay => FormatBytes(UsedBytes);
    public string MaxDisplay => FormatBytes(MaxBytes);
    public string AvailableDisplay => FormatBytes(MaxBytes - UsedBytes);
    
    private static string FormatBytes(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB" };
        int unitIndex = 0;
        double size = bytes;
        
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }
        
        return $"{size:F1} {units[unitIndex]}";
    }
}
