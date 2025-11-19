namespace Rapidex;

using Microsoft.Extensions.Logging;

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
    /// Varsayýlan minimum loglama seviyesi
    /// </summary>
    public LogLevel DefaultMinimumLevel { get; set; } = LogLevel.Debug;

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
    /// Buffer flush süresi (saniye cinsinden)
    /// </summary>
    public int BufferFlushIntervalSeconds { get; set; } = 5;

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
    /// Örn: { "Microsoft.AspNetCore", LogLevel.Warning } -> Microsoft.AspNetCore için Warning seviyesi
    /// </summary>
    public Dictionary<string, LogLevel> CategoryMinimumLevels { get; set; } = new();

    /// <summary>
    /// Belirli kategoriler için ayrý dosya kullanýlsýn mý?
    /// Örn: { "MyService", true } -> MyService için ayrý "MyService-20240101.log" oluþturulur
    /// </summary>
    public Dictionary<string, bool> CategorySeparateFiles { get; set; } = new();

    /// <summary>
    /// Log satýrý formatý
    /// </summary>
    public string OutputTemplate { get; set; } = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Standart Serilog konfigürasyonu kullanýlsýn mý? (appsettings.json'daki Serilog bölümü)
    /// True ise, yukarýdaki ayarlar Serilog yapýlandýrmasý üzerine eklenir.
    /// </summary>
    public bool UseStandardSerilogConfiguration { get; set; } = false;

    /// <summary>
    /// Console'a da log yazýlsýn mý?
    /// </summary>
    public bool WriteToConsole { get; set; } = false;
}
