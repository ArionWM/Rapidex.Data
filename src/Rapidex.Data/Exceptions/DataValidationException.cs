using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Exceptions;
public class DataValidationException : DataExceptionBase
{
    public IValidationResult ValidationResult { get; }

    public DataValidationException(IValidationResult validationResult)
    {
        this.ValidationResult = validationResult;
    }

}
