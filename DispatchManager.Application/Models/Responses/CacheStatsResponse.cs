/// <summary>
/// Respuesta con estadísticas de caché
/// </summary>
public class CacheStatsResponse
{
    public int TotalKeys { get; set; }
    public string TotalMemoryUsage { get; set; } = string.Empty;
    public double HitRate { get; set; }
    public double MissRate { get; set; }
    public List<CacheTagInfo> Tags { get; set; } = new();
}