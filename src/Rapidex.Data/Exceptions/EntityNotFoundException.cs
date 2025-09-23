using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
public class EntityNotFoundException : DataExceptionBase
{
    public string EntityName { get; set; }
    public object EntityId { get; set; }

    public EntityNotFoundException(string entityName, object entityId, string? message = null) : base("EntityNotFound", $"Entity '{entityName}' with Id '{entityId}' not found. \r\n{message}")
    {
        EntityName = entityName;
        EntityId = entityId;
    }


}
