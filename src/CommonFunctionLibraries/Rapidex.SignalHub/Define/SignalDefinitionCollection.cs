using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;
public class SignalDefinitionCollection : DictionaryA<ISignalDefinition>, ISignalDefinitionCollection
{
    public ISignalDefinition Find(string signal)
    {
        return this.Get(signal);
    }

    public void Add(ISignalDefinition def)
    {
        this.Set(def.SignalName, def);
    }
}
