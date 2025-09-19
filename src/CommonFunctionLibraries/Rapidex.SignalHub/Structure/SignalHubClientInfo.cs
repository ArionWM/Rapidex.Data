using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.SignalHub.Structure;
internal class SignalHubClientInfo
{
    public bool IsExternal { get; set; } = false;
    public string ClientId { get; set; } = null;
    public string ClientIp { get; set; } = null;
    //public IUser User / Context?



}
