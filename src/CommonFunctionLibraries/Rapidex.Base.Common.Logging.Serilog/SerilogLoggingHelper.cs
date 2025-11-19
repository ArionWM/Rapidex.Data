//using Microsoft.Extensions.Logging;
//using Serilog;
//using System.Threading;

//namespace Rapidex.Base.Common.Logging.Serilog.Core8;

///// <summary>
///// Serilog tabanlý loglama helper implementasyonu
///// </summary>
//public class SerilogLoggingHelper : ILoggingHelper
//{
//    private readonly ILoggerFactory loggerFactory;
//    private readonly AsyncLocal<Stack<string>> categoryStack = new();

//    public SerilogLoggingHelper(ILoggerFactory loggerFactory)
//    {
//        this.loggerFactory = loggerFactory.NotNull();
//    }

//    private Stack<string> CategoryStack
//    {
//        get
//        {
//            categoryStack.Value ??= new Stack<string>();
//            return categoryStack.Value;
//        }
//    }

//    private string GetCurrentCategory()
//    {
//        return CategoryStack.Count > 0 ? CategoryStack.Peek() : "Application";
//    }

//    public void EnterCategory(string category)
//    {
//        category.NotNull();
//        CategoryStack.Push(category);
//    }

//    public void ExitCategory()
//    {
//        if (CategoryStack.Count > 0)
//        {
//            CategoryStack.Pop();
//        }
//    }

//    private Microsoft.Extensions.Logging.ILogger GetLogger(string category)
//    {
//        return loggerFactory.CreateLogger(category);
//    }

//    public void Verbose(string message)
//    {
//        Verbose(GetCurrentCategory(), message);
//    }

//    public void Verbose(string category, string message)
//    {
//        category.NotNull();
//        message.NotNull();
        
//        var logger = GetLogger(category);
//        logger.LogTrace(message);
//    }

//    public void Debug(string message)
//    {
//        Debug(GetCurrentCategory(), message);
//    }

//    public void Debug(string category, string message)
//    {
//        category.NotNull();
//        message.NotNull();
        
//        var logger = GetLogger(category);
//        logger.LogDebug(message);
//    }

//    public void Info(string message)
//    {
//        Info(GetCurrentCategory(), message);
//    }

//    public void Info(string category, string message)
//    {
//        category.NotNull();
//        message.NotNull();
        
//        var logger = GetLogger(category);
//        logger.LogInformation(message);
//    }

//    public void Info(Exception ex, string? message = null)
//    {
//        Info(GetCurrentCategory(), ex, message);
//    }

//    public void Info(string category, Exception ex, string? message = null)
//    {
//        category.NotNull();
//        ex.NotNull();
        
//        var logger = GetLogger(category);
//        logger.LogInformation(ex, message ?? ex.Message);
//    }

//    public void Warn(string message)
//    {
//        Warn(GetCurrentCategory(), message);
//    }

//    public void Warn(string category, string message)
//    {
//        category.NotNull();
//        message.NotNull();
        
//        var logger = GetLogger(category);
//        logger.LogWarning(message);
//    }

//    public void Warn(Exception ex, string? message = null)
//    {
//        Warn(GetCurrentCategory(), ex, message);
//    }

//    public void Warn(string category, Exception ex, string? message = null)
//    {
//        category.NotNull();
//        ex.NotNull();
        
//        var logger = GetLogger(category);
//        logger.LogWarning(ex, message ?? ex.Message);
//    }

//    public void Error(string message)
//    {
//        Error(GetCurrentCategory(), message);
//    }

//    public void Error(string category, string message)
//    {
//        category.NotNull();
//        message.NotNull();
        
//        var logger = GetLogger(category);
//        logger.LogError(message);
//    }

//    public void Error(Exception ex, string? message = null)
//    {
//        Error(GetCurrentCategory(), ex, message);
//    }

//    public void Error(string category, Exception ex, string? message = null)
//    {
//        category.NotNull();
//        ex.NotNull();
        
//        var logger = GetLogger(category);
//        logger.LogError(ex, message ?? ex.Message);
//    }

//    public void Flush()
//    {
//        // Serilog async sinkleri için flush
//        try
//        {
//            Log.CloseAndFlush();
//        }
//        catch
//        {
//            // Flush hatalarýný yoksay
//        }
//    }
//}
