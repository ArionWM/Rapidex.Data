using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

public enum OrderDirection
{
    Asc,
    Desc
}
public interface IDbCriteriaParser
{
    bool IsYours(string filterOrSqlClause);

    IQueryCriteria Parse(IQueryCriteria query, string filterOrSqlClause);


}


public interface IQueryBase
{
    public SqlKata.Query Query { get; set; }
    string TableName { get; }
    string Alias { get; set; }

    IDbSchemaScope Schema { get; set; }

    IDbEntityMetadata EntityMetadata { get; set; }

    void EnterUpdateMode();


    void SetSchema(IDbSchemaScope schema)
    {
        Schema = schema;
    }

    void Map(IDbEntityMetadata em)
    {
        EntityMetadata = em;
    }
}

public interface IQueryCriteria : IQueryBase
{
    IQueryCriteria Not(Action<IQueryCriteria> act);

    IQueryCriteria Eq(string field, object value);
    IQueryCriteria Like(string field, object value);
    IQueryCriteria In(string field, params object[] values);
    IQueryCriteria Between(string field, object value1, object value2);

    IQueryCriteria Gt(string field, object value);
    IQueryCriteria GtEq(string field, object value);
    IQueryCriteria Lt(string field, object value);
    IQueryCriteria LtEq(string field, object value);

    IQueryCriteria And(params Action<IQueryCriteria>[] acts);
    IQueryCriteria Or(params Action<IQueryCriteria>[] acts);

    IQueryCriteria Nested(string referenceField, Action<IQueryCriteria> nestedCriteria);
    IQueryCriteria Nested(string entityAFieldName, string entityBFieldName, IDbEntityMetadata referenceEntityMetadata, Action<IQueryCriteria> nestedCriteria);


    //IQueryCriteria Releated(string releatedField, IEntity parentEntity, Action<IQueryCriteria> additionalCriteria = null);

}

public interface IQueryPager : IQueryCriteria
{
    IPaging Paging { get; set; }
    IQueryPager Page(long pageSize, long skip, bool includeTotalCount = true);
    IQueryPager ClearPaging();
}

public class OrderInfo
{
    public string FieldOrExpression { get; set; }
    public OrderDirection Direction { get; set; }

    public OrderInfo()
    {

    }

    public OrderInfo(string fieldOrExpression, OrderDirection direction)
    {
        FieldOrExpression = fieldOrExpression;
        Direction = direction;
    }
}

public class OrderCollection : List<OrderInfo>
{
    public OrderCollection()
    {

    }

    public OrderCollection(params OrderInfo[] orders)
    {
        this.AddRange(orders);
    }
}

public interface IQueryOrder : IQueryPager
{
    OrderCollection Order { get; set; }
    IQueryOrder OrderBy(OrderDirection direction, params string[] fields);
}

public interface IQueryLoader : IQueryOrder, ICloneable
{
    IEntityLoadResult Load();

    ILoadResult<DataRow> LoadPartial(params string[] fields); //Şimdilik DataRow dönsün

    IEntity First();

    //IEntity Last();

    ILoadResult<DbEntityId> GetIds();
}

public interface IQueryLoader<T> : IQueryLoader, IQueryBase where T : IConcreteEntity
{
    new IEntityLoadResult<T> Load();

    T Find(long id);

    new T First();

}

public interface IQueryAggregate : IQueryLoader
{
    bool Exist();
    long Count();
    object Sum(string field);
    object Min(string field);
    object Max(string field);
    object Avg(string field);
}

public interface IQueryUpdater : IQueryCriteria
{
    ObjDictionary UpdateData { get; }
    void Update(IDbDataModificationScope workScope, IDictionary<string, object> data);
    //TODO: IDbDataModificationScope üzerinden çağırıldığında IDbDataModificationScope workScope parametresine gerek yok. 
    void Delete();
}

public interface IQuery : IQueryBase, IQueryCriteria, IQueryPager, IQueryOrder, IQueryLoader, IQueryAggregate, IQueryUpdater, ICloneable
{
    //new IQuery Add(IDbCriteria criteria);

    new IQuery Not(Action<IQueryCriteria> act);

    new IQuery Eq(string field, object value);
    new IQuery Like(string field, object value);
    new IQuery In(string field, params object[] values);
    new IQuery Between(string field, object value1, object value2);




    new IQuery Gt(string field, object value);
    new IQuery GtEq(string field, object value);
    new IQuery Lt(string field, object value);
    new IQuery LtEq(string field, object value);



    new IQuery And(params Action<IQueryCriteria>[] acts);
    new IQuery Or(params Action<IQueryCriteria>[] acts);

    new IQuery Nested(string referenceField, Action<IQueryCriteria> nestedCriteria);
    new IQuery Nested(string entityAFieldName, string entityBFieldName, IDbEntityMetadata referenceEntityMetadata, Action<IQueryCriteria> nestedCriteria);

    //new IQuery Releated(string releatedField, IEntity parentEntity, Action<IQueryCriteria> additionalCriteria = null);

    new IQuery OrderBy(OrderDirection direction, params string[] fields);
    new IQuery Page(long pageSize, long skip, bool includeTotalCount = true);
    new IQuery ClearPaging();

    new IQuery EnterUpdateMode();

}

public interface IQuery<T> : IQuery, IQueryLoader<T> where T : IConcreteEntity
{
    //new IQuery<T> Add(IDbCriteria criteria);

    new IQuery<T> Not(Action<IQueryCriteria> act);

    new IQuery<T> Eq(string field, object value);
    new IQuery<T> Like(string field, object value);
    new IQuery<T> In(string field, params object[] values);
    new IQuery<T> Between(string field, object value1, object value2);




    new IQuery<T> Gt(string field, object value);
    new IQuery<T> GtEq(string field, object value);
    new IQuery<T> Lt(string field, object value);
    new IQuery<T> LtEq(string field, object value);



    new IQuery<T> And(params Action<IQueryCriteria>[] acts);
    new IQuery<T> Or(params Action<IQueryCriteria>[] acts);

    new IQuery<T> Nested(string referenceField, Action<IQueryCriteria> nestedCriteria);
    new IQuery<T> Nested(string entityAFieldName, string entityBFieldName, IDbEntityMetadata referenceEntityMetadata, Action<IQueryCriteria> nestedCriteria);


    //new IQuery<T> Releated(string releatedField, IEntity parentEntity, Action<IQueryCriteria> additionalCriteria = null);


    new IQuery<T> OrderBy(OrderDirection direction, params string[] fields);
    new IQuery<T> Page(long pageSize, long skip, bool includeTotalCount = true);
    new IQuery<T> ClearPaging();
    new IQuery<T> EnterUpdateMode();

}
