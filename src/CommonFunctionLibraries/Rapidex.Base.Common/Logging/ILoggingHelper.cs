//using Microsoft.Extensions.Logging;

//namespace Rapidex;

///// <summary>
///// Loglama iþlemleri için temel interface
///// </summary>
//public interface ILoggingHelper
//{
//    /// <summary>
//    /// Kategori yýðýnýna yeni bir kategori ekler
//    /// </summary>
//    void EnterCategory(string category);

//    /// <summary>
//    /// Kategori yýðýnýndan çýkar
//    /// </summary>
//    void ExitCategory();

//    /// <summary>
//    /// Verbose seviyesinde loglama yapar
//    /// </summary>
//    void Verbose(string message);

//    /// <summary>
//    /// Verbose seviyesinde kategori ile loglama yapar
//    /// </summary>
//    void Verbose(string category, string message);

//    /// <summary>
//    /// Debug seviyesinde loglama yapar
//    /// </summary>
//    void Debug(string message);

//    /// <summary>
//    /// Debug seviyesinde kategori ile loglama yapar
//    /// </summary>
//    void Debug(string category, string message);

//    /// <summary>
//    /// Info seviyesinde loglama yapar
//    /// </summary>
//    void Info(string message);

//    /// <summary>
//    /// Info seviyesinde kategori ile loglama yapar
//    /// </summary>
//    void Info(string category, string message);

//    /// <summary>
//    /// Info seviyesinde exception ile loglama yapar
//    /// </summary>
//    void Info(Exception ex, string? message = null);

//    /// <summary>
//    /// Info seviyesinde kategori ve exception ile loglama yapar
//    /// </summary>
//    void Info(string category, Exception ex, string? message = null);

//    /// <summary>
//    /// Warning seviyesinde loglama yapar
//    /// </summary>
//    void Warn(string message);

//    /// <summary>
//    /// Warning seviyesinde kategori ile loglama yapar
//    /// </summary>
//    void Warn(string category, string message);

//    /// <summary>
//    /// Warning seviyesinde exception ile loglama yapar
//    /// </summary>
//    void Warn(Exception ex, string? message = null);

//    /// <summary>
//    /// Warning seviyesinde kategori ve exception ile loglama yapar
//    /// </summary>
//    void Warn(string category, Exception ex, string? message = null);

//    /// <summary>
//    /// Error seviyesinde loglama yapar
//    /// </summary>
//    void Error(string message);

//    /// <summary>
//    /// Error seviyesinde kategori ile loglama yapar
//    /// </summary>
//    void Error(string category, string message);

//    /// <summary>
//    /// Error seviyesinde exception ile loglama yapar
//    /// </summary>
//    void Error(Exception ex, string? message = null);

//    /// <summary>
//    /// Error seviyesinde kategori ve exception ile loglama yapar
//    /// </summary>
//    void Error(string category, Exception ex, string? message = null);

//    /// <summary>
//    /// Buffered loglarý flush eder (özellikle Error için önemli)
//    /// </summary>
//    void Flush();
//}
