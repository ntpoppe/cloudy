namespace Cloudy.Infrastructure.Settings;

public class MinioSettings
{
    public string Endpoint { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Bucket { get; set; } = "cloudy";
    public bool UseSSL { get; set; } = false;
}