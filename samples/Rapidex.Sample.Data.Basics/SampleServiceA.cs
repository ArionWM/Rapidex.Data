using Rapidex.Sample.Data.Basics.ConcreteEntitites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Sample.Data.Basics;
internal class SampleServiceA
{
    public void CreateOrderOnMasterDbAndBaseSchema(Contact contact, params string[] itemCodes)
    {
        var myDb = Database.Dbs.Db(); //TODO: Database.MasterDb() ....
        using var work = myDb.BeginWork();

        Order order = work.New<Order>();
        order.Customer = contact;
        order.Save();

        foreach (var itemCode in itemCodes)
        {
            Item item = work.GetQuery<Item>()
                .Eq(nameof(Item.Code), itemCode)
                .First();

            item.NotNull($"Item not found: {itemCode}");

            OrderLine line = work.New<OrderLine>();
            line.Item = item;
            line.Quantity = 1;
            line.UnitPrice = item.Price;

            order.Lines.Add(line);
        }

        work.CommitChanges();
    }

    public void CreateOrderOnMasterDbAndBaseSchema2(Contact contact, params string[] itemCodes)
    {
        var myDb = Database.Dbs.Db();
        using (var dmScope = myDb.BeginWork())
        {
            Order order = dmScope.New<Order>();
            order.Customer = contact;
            order.Save();

            foreach (var itemCode in itemCodes)
            {
                Item item = dmScope.GetQuery<Item>()
                    .Eq(nameof(Item.Code), itemCode)
                    .First();

                item.NotNull($"Item not found: {itemCode}");

                OrderLine line = dmScope.New<OrderLine>();
                line.Item = item;
                line.Quantity = 1;
                line.UnitPrice = item.Price;

                order.Lines.Add(line);
            }

            //Do not need to commit changes, transaction commit on dispose.
        }
    }

    public void CreateOrder(IDbSchemaScope scope, Contact contact, params string[] itemCodes)
    {
        //Order order = 


    }

}
