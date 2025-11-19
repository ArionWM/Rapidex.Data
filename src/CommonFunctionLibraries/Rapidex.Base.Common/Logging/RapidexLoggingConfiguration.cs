namespace Rapidex;

/// <summary>
/// Loglama konfigürasyonu için temel sýnýf
/// </summary>
public class RapidexLoggingConfiguration
{
    /// <summary>
    /// Log dosyalarýnýn yazýlacaðý ana dizin
    /// </summary>
    public string LogDirectory { get; set; } = "Logs";

    /// <summary>
    /// Log dosyasý ön eki (örn: "MyApp" -> "MyApp-20240101.log")
    /// </summary>
    public string LogFilePrefix { get; set; } = "app";

    /// <summary>
    /// Varsayýlan minimum loglama seviyesi (0=Trace, 1=Debug, 2=Information, 3=Warning, 4=Error, 5=Critical, 6=None)
    /// </summary>
    public int DefaultMinimumLevel { get; set; } = 1; // Debug

    /// <summary>
    /// Error loglarý için ayrý dosya kullanýlsýn mý?
    /// </summary>
    public bool UseSeperateErrorLogFile { get; set; } = true;

    /// <summary>
    /// Warning loglarý için ayrý dosya kullanýlsýn mý?
    /// </summary>
    public bool UseSeperateWarningLogFile { get; set; } = true;

    /// <summary>
    /// Error dýþýndaki loglar için buffer kullanýlsýn mý? (Disk IO'su azaltýr)
    /// </summary>
    public bool UseBufferForNonErrors { get; set; } = true;

    /// <summary>
    /// Buffer boyutu (byte cinsinden)
    /// </summary>
    public int BufferSize { get; set; } = 32768; // 32KB

    /// <summary>
    /// Log dosyasý maksimum boyutu (byte cinsinden). Aþýldýðýnda yeni dosya oluþturulur.
    /// </summary>
    public long MaxLogFileSize { get; set; } = 100 * 1024 * 1024; // 100MB

    /// <summary>
    /// Saklanacak maksimum dosya sayýsý (null = sýnýrsýz)
    /// </summary>
    public int? RetainedFileCountLimit { get; set; } = 31;

    /// <summary>
    /// Belirli kategoriler için özel loglama seviyeleri
    /// Örn: { "Microsoft.AspNetCore", 3 } -> Microsoft.AspNetCore için Warning seviyesi
    /// </summary>
    public Dictionary<string, int> CategoryMinimumLevels { get; set; } = new();

    /// <summary>
    /// Belirli kategoriler için ayrý dosya kullanýlsýn mý?
    /// Örn: { "MyService", true } -> MyService için ayrý "MyService-20240101.log" oluþturulur
    /// </summary>
    public Dictionary<string, bool> CategorySeparateFiles { get; set; } = new();

    /// <summary>
    /// Log satýrý formatý
    /// </summary>
    public string OutputTemplate { get; set; } = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
}
