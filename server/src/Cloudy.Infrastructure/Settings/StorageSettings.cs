namespace Cloudy.Infrastructure.Settings;

public class StorageSettings
{
    public long MaxStorageBytes { get; set; } 
    public string MaxStorageDisplay => $"{MaxStorageBytes / (1024 * 1024)}MB";
}