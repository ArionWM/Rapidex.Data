using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
public class DbInsufficientPermissionsException : DataExceptionBase
{
    public DbInsufficientPermissionsException(string operation, string userId, string? message = null)
        : base("DbInsufficientPermissions", $"Insufficient permissions for operation '{operation}' by user '{userId}'. \r\n{message} \r\nSee: abc") //TODO: Link to documentation
    {
        this.Operation = operation;
        this.UserId = userId;
    }

    public string Operation { get; }
    public string UserId { get; }
}
