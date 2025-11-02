using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;
public class TestTimeProvider : DefaultTimeProvider, ITimeProvider
{
    protected TimeSpan? testTimeDiff;
    public override DateTimeOffset Now => this.testTimeDiff == null ? base.Now : base.Now - this.testTimeDiff.Value;
    public override DateTimeOffset UtcNow => this.testTimeDiff == null ? base.UtcNow : base.UtcNow - this.testTimeDiff.Value;

    public void SetTestStartTime(DateTimeOffset startValue)
    {
        var now = base.UtcNow;
        this.testTimeDiff = now - startValue;
    }
}
