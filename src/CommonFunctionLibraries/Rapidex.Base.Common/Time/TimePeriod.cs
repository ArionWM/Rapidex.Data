using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;

public struct TimePeriod //TODO: IPeriod
{
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public Period? Period { get; set; }
    public int? Index { get; set; }

    public TimePeriod()
    {

    }

    public TimePeriod(DateTimeOffset start, DateTimeOffset end, Period? period = null, int? index = null)
    {
        if (start < end)
        {
            this.Start = start;
            this.End = end;
        }
        else
        {
            this.Start = end;
            this.End = start;
        }

        this.Period = period;
        this.Index = index;
    }

    public bool IsIn(DateTimeOffset time)
    {
        return this.Start <= time && this.End >= time;
    }

    public override int GetHashCode()
    {
        return this.Start.GetHashCode() ^ this.End.GetHashCode();
    }

    public static implicit operator TimePeriod(Tuple<DateTime, DateTime> tuple)
    {
        return new TimePeriod(tuple.Item1, tuple.Item2);
    }
}

public static class TimePeriodExtender
{
    public static bool IsIn(this DateTimeOffset time, TimePeriod period)
    {
        return time <= period.End && time >= period.Start;
    }

    public static bool IsIntersect(this TimePeriod p1, TimePeriod p2)
    {
        return p1.Start.IsIn(p2) || p1.End.IsIn(p2);
    }

    public static TimePeriod? Select(this IEnumerable<TimePeriod> timePeriods, DateTimeOffset time)
    {
        TimePeriod? timePeriod = timePeriods.FirstOrDefault(tp => tp.IsIn(time));
        return timePeriod;
    }
}

