using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex;

public interface IResult
{
    [JsonPropertyOrder(-9999)]
    bool Success { get; set; }

    [JsonPropertyOrder(-9998)]

    string Description { get; set; }
}

public interface IResult<T> : IResult
{
    T Content { get; set; }
}



public interface IValidationResultItem //From ProCore
{
    bool MarkProblem { get; set; }
    string ParentName { get; set; }
    string MemberName { get; set; }
    string Description { get; set; }
}

public interface IValidationResult : IResult
{
    //Geliştirilecek

    List<IValidationResultItem> Errors { get; }
    List<IValidationResultItem> Warnings { get; }
    List<IValidationResultItem> Infos { get; }
}


public interface IUpdateResult<T> : IResult
{
    IReadOnlyList<T> ModifiedItems { get; }
    IReadOnlyList<T> AddedItems { get; }
    IReadOnlyList<T> DeletedItems { get; }

    void MergeWith(IUpdateResult<T> with);

    void Modified(T item);
    void Added(T item);
    void Deleted(T item);
}

public interface IUpdateResult : IUpdateResult<object>
{

}

public class UpdateResult<T> : IUpdateResult<T>
{
    List<T> _modifiedItems { get; } = new List<T>();
    List<T> _addedItems { get; } = new List<T>();
    List<T> _deletedItems { get; } = new List<T>();



    public bool Success { get; set; }
    public string Description { get; set; }
    public IReadOnlyList<T> ModifiedItems => this._modifiedItems.AsReadOnly();
    public IReadOnlyList<T> AddedItems => this._addedItems.AsReadOnly();
    public IReadOnlyList<T> DeletedItems => this._deletedItems.AsReadOnly();


    public void Added(T item)
    {
        this._addedItems.Add(item);
    }

    public void Deleted(T item)
    {
        this._deletedItems.Add(item);
    }

    public void Modified(T item)
    {
        this._modifiedItems.Add(item);
    }

    public void MergeWith(IUpdateResult<T> with)
    {
        this._modifiedItems.AddRange(with.ModifiedItems);
        this._addedItems.AddRange(with.AddedItems);
        this._deletedItems.AddRange(with.DeletedItems);
    }
}

public class UpdateResult : UpdateResult<object>, IUpdateResult
{
    public static UpdateResult AddedMany(params object[] items)
    {
        var result = new UpdateResult();
        result.Added(items);
        return result;
    }

    public static UpdateResult ModifiedMany(params object[] items)
    {
        var result = new UpdateResult();
        result.Modified(items);
        return result;
    }

    public static UpdateResult DeletedMany(params object[] items)
    {
        var result = new UpdateResult();
        result.Deleted(items);
        return result;
    }
}

public interface ILoadResult<T> : IReadOnlyList<T>, IPaging, IEmptyCheckObject
{
    long TotalItemCount { get; set; }
    long ItemCount { get; }
}
