using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;


/// <summary>
/// ThreadStatic context yığınları için
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IStack<T>
{
    internal Stack<T> Stack { get; }
    void Enter(T item);
    T GetCurrent();
    T Exit();
}

public interface IBaseConverter
{
    Type FromType { get; }
    Type ToType { get; }

    [Obsolete("Use TryConvert() instead.")]
    object Convert(object from, Type toType);

    bool TryConvert(object from, Type toType, out object to);
}

public interface IBaseConverter<TFrom, TTo> : IBaseConverter
{
    new Type FromType => typeof(TFrom);
    new Type ToType => typeof(TTo);

    TTo Convert(TFrom from, TTo to);
}


public interface IPaging
{
    long? PageSize { get; set; }
    long? StartIndex { get; set; }
    long? PageIndex { get; set; }
    long? PageCount { get; set; }

    bool IncludeTotalItemCount { get; set; }

}

public interface IEmptyCheckObject
{
    bool IsEmpty { get; }
}




public interface ILoggingHelper
{
    void EnterCategory(string category);
    void ExitCategory();

    void Info(string category, string message);
    void Info(string message);

    void Info(string category, Exception ex, string message = null);

    void Info(Exception ex, string message = null);

    void Debug(string category, string message);

    void Debug(string category, string format, params object[] args);

    void Debug(string format, params object[] args);

    void Debug(string message);

    void Verbose(string category, string message);

    void Verbose(string category, string format, params object[] args);

    void Verbose(string format, params object[] args);

    void Verbose(string message);

    void Warn(string category, string message);

    void Warn(string message);

    void Error(string category, string message);

    void Error(string message);

    void Error(string category, Exception ex, string message = null);

    void Error(Exception ex, string message = null);

    void Flush();
}