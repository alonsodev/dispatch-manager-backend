/// <summary>
/// Información de tag de caché
/// </summary>
public class CacheTagInfo
{
    public string Tag { get; set; } = string.Empty;
    public int KeyCount { get; set; }
    public DateTime LastAccessed { get; set; }
}