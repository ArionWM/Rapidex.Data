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

    string? Description { get; set; }
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
    
    [JsonPropertyOrder(9990)]
    IReadOnlyList<T> AddedItems { get; }

    [JsonPropertyOrder(9991)]
    IReadOnlyList<T> ModifiedItems { get; }

    [JsonPropertyOrder(9992)]
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
#pragma warning disable IDE1006 // Naming Styles
    protected List<T> modifiedItems { get; } = new List<T>();
    protected List<T> addedItems { get; } = new List<T>();
    protected List<T> deletedItems { get; } = new List<T>();
#pragma warning restore IDE1006 // Naming Styles

    [JsonPropertyOrder(-9999)]
    public bool Success { get; set; }

    [JsonPropertyOrder(-9990)]
    public string Description { get; set; }


    [JsonPropertyOrder(9990)]
    public IReadOnlyList<T> AddedItems => this.addedItems.AsReadOnly();


    [JsonPropertyOrder(9991)]
    public IReadOnlyList<T> ModifiedItems => this.modifiedItems.AsReadOnly();


    [JsonPropertyOrder(9992)]
    public IReadOnlyList<T> DeletedItems => this.deletedItems.AsReadOnly();


    public void Added(T item)
    {
        this.addedItems.Add(item);
    }

    public void Deleted(T item)
    {
        this.deletedItems.Add(item);
    }

    public void Modified(T item)
    {
        this.modifiedItems.Add(item);
    }

    public void MergeWith(IUpdateResult<T> with)
    {
        this.modifiedItems.AddRange(with.ModifiedItems);
        this.addedItems.AddRange(with.AddedItems);
        this.deletedItems.AddRange(with.DeletedItems);
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
