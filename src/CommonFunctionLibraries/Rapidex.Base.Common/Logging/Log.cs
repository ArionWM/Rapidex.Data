using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Rapidex
{
    public static class Log
    {
        //public static ILoggerFactory LoggerFactory { get; set; }
        public static ILoggingHelper Logger { get; set; }


#if DEBUG
        public static bool IsDebugEnabled { get { return true; } }
#else
        public static bool IsDebugEnabled { get { return false; } }
#endif

        private static bool CheckLogger()
        {
            if (Logger == null)
            {
                System.Diagnostics.Debug.WriteLine("Logger is not set. Please set Logger property of Log class. See: abc"); //TODO: Create document and reference here
                return false;
            }
            return true;
        }

        public static void EnterCategory(string category)
        {
            Logger.EnterCategory(category);
        }

        public static void ExitCategory()
        {
            Logger.ExitCategory();
        }

        public static void Info(string category, string message)
        {
            if (CheckLogger())
                Log.Logger.Info(category, message);
        }

        public static void Info(string message)
        {
            if (CheckLogger())
                Log.Logger.Info(message);
        }

        public static void Info(string category, Exception ex, string message = null)
        {
            if (CheckLogger())
                Log.Logger.Info(ex, message);

        }

        public static void Info(Exception ex, string message = null)
        {
            if (CheckLogger())
                Log.Logger.Info(ex, message);
        }

        public static void Debug(string category, string message)
        {
            if (CheckLogger())
                Log.Logger.Debug(category, message);

        }

        public static void Debug(string category, string format, params object[] args)
        {
            if (CheckLogger())
                Log.Debug(category, string.Format(format, args));
        }

        public static void Debug(string format, params object[] args)
        {
            if (CheckLogger())
                Log.Debug(string.Format(format, args));
        }

        public static void Debug(string message)
        {
            if (CheckLogger())
                Log.Logger.Debug(message);
        }

        public static void Verbose(string category, string message)
        {
            if (CheckLogger())
                Log.Logger.Verbose(category, message);
        }

        public static void Verbose(string category, string format, params object[] args)
        {
            if (CheckLogger())
                Log.Verbose(category, string.Format(format, args));
        }

        public static void Verbose(string format, params object[] args)
        {
            if (CheckLogger())
                Log.Verbose(string.Format(format, args));
        }

        public static void Verbose(string message)
        {
            if (CheckLogger())
                Log.Logger.Verbose(message);
        }

        public static void Warn(string category, string message)
        {
            if (CheckLogger())
                Log.Logger.Warn(category, message);
        }

        public static void Warn(string message)
        {
            if (CheckLogger())
                Log.Logger.Warn(message);
        }

        public static void Error(string category, string message)
        {
            if (CheckLogger())
            {
                Log.Logger.Error(category, message);
                Log.Logger.Flush();
            }
        }

        public static void Error(string message)
        {
            if (CheckLogger())
            {
                Log.Logger.Error(message);
                Log.Logger.Flush();
            }

        }

        public static void Error(string category, Exception ex, string message = null)
        {
            if (CheckLogger())
            {
                Log.Logger.Error(category, ex, message);
                Log.Logger.Flush();
            }
        }

        public static void Error(Exception ex, string message = null)
        {
            if (CheckLogger())
            {
                Log.Logger.Error(ex, message);
                Log.Logger.Flush();
            }
        }


    }

    public static class LogExtensions
    {
        public static void Log(this Exception ex, string message = null)
        {
            Rapidex.Log.Error(ex, message);
        }
    }
}
