using Microsoft.Extensions.Logging;

namespace Rapidex;

/// <summary>
/// Runtime'da loglama seviyelerini deðiþtirmek için interface
/// </summary>
public interface ILogLevelController
{
    /// <summary>
    /// Global minimum loglama seviyesini ayarlar
    /// </summary>
    void SetMinimumLevel(LogLevel logLevel);

    /// <summary>
    /// Belirli bir namespace veya tip için minimum loglama seviyesini ayarlar
    /// </summary>
    void SetMinimumLevel(string sourceContext, LogLevel logLevel);

    /// <summary>
    /// Mevcut global minimum loglama seviyesini döner
    /// </summary>
    LogLevel GetMinimumLevel();

    /// <summary>
    /// Belirli bir namespace veya tip için minimum loglama seviyesini döner
    /// </summary>
    LogLevel? GetMinimumLevel(string sourceContext);

    /// <summary>
    /// Tüm seviye ayarlarýný temizler
    /// </summary>
    void ResetAllLevels();
}
