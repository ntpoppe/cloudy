namespace Cloudy.Domain;

public static class Common
{
    public static string GenerateStorageKey(string name)
    {
        return $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}-{name}";
    }
}