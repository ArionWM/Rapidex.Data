using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex;

public static class TimeHelper
{
    public static DateTimeOffset WithoutSeconds(this DateTimeOffset time)
    {
        if (time.Second > 0 || time.Millisecond > 0 || time.Microsecond > 0)
            time = new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, time.Offset);

        return time;
    }

    public static DateTimeOffset Day(this DateTimeOffset time)
    {
        DateTimeOffset _time = new DateTimeOffset(time.Year, time.Month, time.Day, 0, 0, 0, time.Offset);
        return _time;
    }

    public static DateTimeOffset WeekStart(this DateTimeOffset time)
    {
        DateTimeOffset _time = new DateTimeOffset(time.Year, time.Month, time.Day - (int)time.DayOfWeek, 0, 0, 0, TimeSpan.Zero); //time.Offset?
        return _time;
    }

    public static DateTimeOffset MonthStart(this DateTimeOffset time)
    {
        DateTimeOffset _time = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
        return _time;
    }

    public static DateTimeOffset YearStart(this DateTimeOffset time)
    {
        DateTimeOffset _time = new DateTimeOffset(time.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        return _time;
    }



    public static DateTimeOffset FirstDayOfWeek(this DateTimeOffset time)
    {
        DateTimeOffset date = time.Day();
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7; // 0 = Monday, 6 = Sunday
        date = date.AddDays(-1 * diff);
        return date;
    }

    public static DateTimeOffset FirstDayOfHalfyear(this DateTimeOffset time)
    {
        int month = time.Month;
        if (month <= 6)
            month = 1; // Ocak
        else
            month = 7; // Temmuz
        DateTimeOffset date = new DateTimeOffset(time.Year, month, 1, 0, 0, 0, TimeSpan.Zero);
        return date;
    }

    public static DateTimeOffset FirstDayOfQuarter(this DateTimeOffset time)
    {
        int month = time.Month;
        if (month <= 3)
            month = 1;
        else if (month <= 6)
            month = 4;
        else if (month <= 9)
            month = 7;
        else
            month = 10;
        DateTimeOffset date = new DateTimeOffset(time.Year, month, 1, 0, 0, 0, TimeSpan.Zero);
        return date;
    }

    public static DateTimeOffset FirstDayOfMonth(this DateTimeOffset time)
    {
        DateTimeOffset date = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
        return date;
    }

    public static DateTimeOffset FirstDayOfMonth(this DateTimeOffset? time)
    {
        DateTimeOffset date = new DateTimeOffset(time.Value.Year, time.Value.Month, 1, 0, 0, 0, TimeSpan.Zero);
        return date;
    }


    public static DateTimeOffset RoundToHour(this DateTimeOffset time)
    {
        //int hour = date.Hour, minute = date.Minute, second = date.Second;

        DateTimeOffset date = new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, TimeSpan.Zero);

        if (date.Second > 0)
        {
            date = date.AddMinutes(1);
            date = date.AddSeconds(-1 * date.Second);
        }

        if (date.Minute > 30)
        {
            date = date.AddMinutes(-1 * date.Minute);
        }
        else
        {
            if (date.Minute > 0)
            {
                date = date.AddMinutes(-1 * date.Minute + 60);
            }
        }

        return date;
    }

    public static DateTimeOffset RoundToHalfHour(this DateTimeOffset time)
    {
        //int hour = date.Hour, minute = date.Minute, second = date.Second;

        DateTimeOffset date = new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, TimeSpan.Zero);

        if (date.Second > 0)
        {
            date = date.AddMinutes(1);
            date = date.AddSeconds(-1 * date.Second);
        }

        if (date.Minute > 30)
        {
            date = date.AddHours(1);
            date = date.AddMinutes(-1 * date.Minute);
        }
        else
        {
            if (date.Minute > 0)
            {
                date = date.AddMinutes(-1 * date.Minute + 30);
            }
        }

        return date;
    }


    /// <summary>
    /// Milisecond bilgisi olmadan 
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static DateTimeOffset WithoutMs(this DateTimeOffset time)
    {
        DateTimeOffset date = new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, TimeSpan.Zero);
        return date;
    }

    /// <summary>
    /// Verilen periyotun son gününü bulur
    /// </summary>
    /// <param name="time"></param>
    /// <param name="period"></param>
    /// <returns></returns>
    public static DateTimeOffset GetEndTime(this DateTimeOffset time, Period period)
    {
        DateTimeOffset date = time.GetStart(period);
        switch (period)
        {
            case Period.Daily:
                return new DateTimeOffset(date.Year, date.Month, date.Day, 23, 59, 59, TimeSpan.Zero);
            case Period.Weekly:
                return date.AddDays(6).GetEndTime(Period.Daily);
            case Period.Monthly:
                return date.AddMonths(1).AddDays(-1).GetEndTime(Period.Daily);
            case Period.Quarterly:
                return date.AddMonths(3).AddDays(-1).GetEndTime(Period.Daily);
            case Period.HalfYear:
                return date.AddMonths(6).AddDays(-1).GetEndTime(Period.Daily);
            case Period.Yearly:
                return date.AddMonths(12).AddDays(-1).GetEndTime(Period.Daily);
            default:
                throw new NotSupportedException($"period: {period}");
        }
    }



    public static DateTimeOffset GetStart(this DateTimeOffset time, Period period)
    {
        DateTimeOffset date = new DateTimeOffset(time.Year, time.Month, time.Day, 0, 0, 0, TimeSpan.Zero);
        switch (period)
        {
            case Period.Daily:
                return date;
            case Period.Weekly:
                return time.FirstDayOfWeek();
            case Period.Monthly:
                return date.FirstDayOfMonth();
            case Period.Quarterly:
                return date.FirstDayOfQuarter();
            case Period.HalfYear:
                return date.FirstDayOfHalfyear();
            case Period.Yearly:
                return new DateTimeOffset(time.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
            default:
                throw new NotSupportedException($"period: {period}");
        }
    }

    public static DateTimeOffset GetNextStart(this DateTimeOffset time, Period period)
    {
        DateTimeOffset date = time.GetStart(period);
        switch (period)
        {
            case Period.Daily:
                return date.AddDays(1);
            case Period.Weekly:
                return date.AddDays(7);
            case Period.Monthly:
                return date.AddMonths(1);
            case Period.Quarterly:
                return date.AddMonths(3);
            case Period.HalfYear:
                return date.AddMonths(6);
            case Period.Yearly:
                return date.AddMonths(12);
            default:
                throw new NotSupportedException($"period: {period}");
        }
    }

    public static bool IsIn(this DateTimeOffset time, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        return time <= endTime && time >= startTime;
    }

    public static bool IsIn(this DateTimeOffset time, Tuple<DateTime, DateTime> period)
    {
        Tuple<DateTime, DateTime> periodPass = period;

        if (period.Item1 > period.Item2)
            periodPass = new Tuple<DateTime, DateTime>(period.Item2, period.Item1);

        return time <= periodPass.Item2 && time >= periodPass.Item1;
    }

    public static DateTimeOffset GetNext(this DateTimeOffset time, DateTimeOffset mustBiggerThan, TimeSpan span)
    {
        DateTimeOffset refTime = time + span;

        while (refTime < mustBiggerThan)
            refTime = refTime + span;

        return refTime;
    }

    public static DateTimeOffset GetNextIfSmallerThan(this DateTimeOffset time, DateTimeOffset mustBiggerThan, TimeSpan span)
    {
        DateTimeOffset refTime = time;

        while (refTime < mustBiggerThan)
            refTime = refTime + span;

        return refTime;
    }

    public static TimePeriod[] CalculateYearlyPeriods(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        List<TimePeriod> periods = new List<TimePeriod>();

        startTime = startTime.Day();
        endTime = endTime.Day();

        DateTimeOffset refTime = startTime;
        while (refTime < endTime)
        {
            DateTimeOffset nextYear = refTime.AddYears(1);
            DateTimeOffset priorPeriodEndDate = nextYear.AddDays(-1);
            priorPeriodEndDate = new DateTimeOffset(priorPeriodEndDate.Year, priorPeriodEndDate.Month, priorPeriodEndDate.Day, 23, 59, 59, TimeSpan.Zero);
            if (priorPeriodEndDate > endTime)
                priorPeriodEndDate = endTime;

            TimePeriod period = new TimePeriod(refTime, priorPeriodEndDate);
            periods.Add(period);

            refTime = nextYear;
        }

        return periods.ToArray();
    }

    public static int GetYearDifference(this DateTimeOffset startDate, DateTimeOffset currentDate)
    {
        try
        {
            if (currentDate < startDate)
                throw new ArgumentOutOfRangeException($"must be startDate < currentDate ({startDate} < {currentDate} ?)");

            TimePeriod[] timePeriods = CalculateYearlyPeriods(startDate, currentDate.AddYears(1).AddDays(1));
            return timePeriods.Length - 2;
        }
        catch (ArgumentOutOfRangeException ex)
        {

            ex.Log($"date: {startDate}, from: {currentDate}");
            throw;
        }
    }


    public static DateTimeOffset GetFirst(DateTimeOffset startDate, DayOfWeek dayOfWeek)
    {
        if (startDate.DayOfWeek == dayOfWeek)
            return startDate;

        do
        {
            startDate = startDate.AddDays(1);
        }
        while (startDate.DayOfWeek == dayOfWeek);

        return startDate;
    }


    public static bool IsSameDay(this DateTimeOffset time1, DateTimeOffset time2)
    {
        return time1.Day() == time2.Day();
    }

    public static bool IsSameDay(this DateTimeOffset? time1, DateTimeOffset? time2)
    {
        return time1.HasValue && time2.HasValue && time1.Value.Day() == time2.Value.Day();
    }

    public static bool IsSameMonth(this DateTimeOffset time1, DateTimeOffset time2)
    {
        return time1.Year == time2.Year && time1.Month == time2.Month;
    }

    public static (DateTimeOffset start, DateTimeOffset end)[] SplitToDays(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        List<(DateTimeOffset start, DateTimeOffset end)> result = new List<(DateTimeOffset start, DateTimeOffset end)>();
        if (!startDate.HasValue || !endDate.HasValue)
            return result.ToArray();
        DateTimeOffset currentStart = startDate.Value;
        DateTimeOffset finalEnd = endDate.Value;
        while (currentStart.Day() < finalEnd.Day())
        {
            DateTimeOffset currentEnd = new DateTimeOffset(currentStart.Year, currentStart.Month, currentStart.Day, 23, 59, 59, TimeSpan.Zero);
            result.Add((currentStart, currentEnd));
            currentStart = currentEnd.AddSeconds(1);
        }
        result.Add((currentStart, finalEnd));
        return result.ToArray();
    }

}
