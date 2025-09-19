using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public class ExceptionManagerBase : IExceptionManager
    {
        public virtual Exception Translate(Exception ex)
        {
            return ex;
        }
    }

    public class ExceptionManager : ExceptionManagerBase, IExceptionManager
    {
        public IServiceProvider Services { get; }
        public ExceptionManager(IServiceProvider sb)
        {
            Services = sb;
        }

        public override Exception Translate(Exception ex)
        {
            if (ex is TranslatedException)
            {
                return ex;
            }

            var trs = Services.GetServices<IExceptionTranslator>();
            foreach (var tr in trs)
            {
                var result = tr.Translate(ex);
                if (result != null)
                {
                    return result;
                }
            }

            return ex;
        }
    }
}
