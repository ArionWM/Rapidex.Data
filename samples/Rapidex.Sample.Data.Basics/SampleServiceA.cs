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
        var myDb = Database.Scopes.Db();
        Order order = myDb.New<Order>();
        order.Customer = contact;
        order.Save();

        foreach (var itemCode in itemCodes)
        {
            Item item = myDb.GetQuery<Item>()
                .Eq(nameof(Item.Code), itemCode)
                .First();

            item.NotNull($"Item not found: {itemCode}");

            OrderLine line = myDb.New<OrderLine>();
            line.Item = item;
            line.Quantity = 1;
            line.UnitPrice = item.Price;

            order.Lines.Add(line);
        }

        myDb.ApplyChanges();
    }

    public void CreateOrderOnMasterDbAndBaseSchema2(Contact contact, params string[] itemCodes)
    {
        var myDb = Database.Scopes.Db();
        using (var dmScope = myDb.Begin())
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
