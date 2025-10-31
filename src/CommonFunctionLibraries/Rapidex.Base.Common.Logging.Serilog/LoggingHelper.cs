using Serilog.Context;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Logging.Serilog.Core8;

internal class LoggingHelper : ILoggingHelper
{
    ILogger logger;

    [ThreadStatic]
    static string CurrentCategory;

    public LoggingHelper(ILogger logger)
    {
        this.logger = logger;
    }


    public void Info(string category, string message)
    {
        using (LogContext.PushProperty("Category", category))
        {
            this.logger.Information(message);
        }
    }

    public void Info(string message)
    {
        if (CurrentCategory.IsNullOrEmpty())
            this.logger.Information(message);
        else
            this.Info(CurrentCategory, message);
    }

    public void Info(string category, Exception ex, string message = null)
    {
        using (LogContext.PushProperty("Category", category))
        {
            this.logger.Information(ex, message);
        }
    }

    public void Info(Exception ex, string message = null)
    {
        if (CurrentCategory.IsNullOrEmpty())
            this.logger.Information(ex, message);
        else
            this.Info(CurrentCategory, ex, message);
    }

    public void Debug(string category, string message)
    {
        using (LogContext.PushProperty("Category", category))
        {
            this.logger.Debug(message);
        }
    }

    public void Debug(string category, string format, params object[] args)
    {
        Log.Debug(category, string.Format(format, args));
    }

    public void Debug(string format, params object[] args)
    {
        if (CurrentCategory.IsNullOrEmpty())
            Log.Debug(string.Format(format, args));
        else
            this.Debug(CurrentCategory, string.Format(format, args));
    }

    public void Debug(string message)
    {
        if (CurrentCategory.IsNullOrEmpty())
            this.logger.Debug(message);
        else
            this.Debug(CurrentCategory, message);
    }

    public void Verbose(string category, string message)
    {
        using (LogContext.PushProperty("Category", category))
        {
            this.logger.Verbose(message);
        }
    }

    public void Verbose(string category, string format, params object[] args)
    {
        Log.Verbose(category, string.Format(format, args));
    }

    public void Verbose(string format, params object[] args)
    {
        if (CurrentCategory.IsNullOrEmpty())
            Log.Verbose(string.Format(format, args));
        else
            this.Verbose(CurrentCategory, string.Format(format, args));
    }

    public void Verbose(string message)
    {
        if (CurrentCategory.IsNullOrEmpty())
            this.logger.Verbose(message);
        else
            this.Verbose(CurrentCategory, message);
    }

    public void Warn(string category, string message)
    {
        using (LogContext.PushProperty("Category", category))
        {
            this.logger.Warning(message);
        }
    }

    public void Warn(string message)
    {
        if (CurrentCategory.IsNullOrEmpty())
            this.logger.Warning(message);
        else
            this.Warn(CurrentCategory, message);
    }

    public void Error(string category, string message)
    {
        using (LogContext.PushProperty("Category", category))
        {
            this.logger.Error(message);
        }
    }

    public void Error(string message)
    {
        if (CurrentCategory.IsNullOrEmpty())
            this.logger.Error(message);
        else
            this.Error(CurrentCategory, message);
    }

    public void Error(string category, Exception ex, string message = null)
    {
        using (LogContext.PushProperty("Category", category))
        {
            this.logger.Error(ex, message);
        }
    }

    public void Error(Exception ex, string message = null)
    {
        if (CurrentCategory.IsNullOrEmpty())
            this.logger.Error(ex, message);
        else
            this.Error(CurrentCategory, ex, message);
    }

    public void Flush()
    {
        SerilogExtensions.Flush();


    }

    public void EnterCategory(string category)
    {
        CurrentCategory = category;
    }

    public void ExitCategory()
    {
        CurrentCategory = null;
    }
}
