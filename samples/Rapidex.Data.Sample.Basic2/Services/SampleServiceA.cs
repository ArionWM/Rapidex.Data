using Rapidex.Data.Sample.App2.ConcreteEntitites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Sample.App2.Services;
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


}
