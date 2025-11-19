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

public interface IDirectConverter
{
    Type FromType { get; }
    Type ToType { get; }

    [Obsolete("Use TryConvert() instead.")]
    object Convert(object from, Type toType);

    bool TryConvert(object from, Type toType, out object to);
}

public interface IDirectConverter<TFrom, TTo> : IDirectConverter
{
    new Type FromType => typeof(TFrom);
    new Type ToType => typeof(TTo);

    TTo Convert(TFrom from, TTo to);
}

public interface ICustomTypeConverter
{
    bool CanConvert(Type fromType, Type toType);
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





