using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex;

internal class DateTimeOffsetConverter : ConverterBase<DateTime, DateTimeOffset>
{
    public override object Convert(object from, Type toType)
    {
        DateTime dt = (DateTime)from;
        if (dt == default(DateTime))
            return default(DateTimeOffset);

        return new DateTimeOffset(dt, TimeSpan.Zero); //Evet, UTC olacak
    }

    public override bool TryConvert(object from, Type toType, out object to)
    {
        DateTime dt = (DateTime)from;
        if (dt == default(DateTime))
        {
            to = default(DateTimeOffset);
            return true;
        }
        to = new DateTimeOffset(dt, TimeSpan.Zero); //Evet, UTC olacak
        return true;
    }
}

internal class DateTimeOffsetNullableConverter : ConverterBase<DateTime?, DateTimeOffset?>
{
    public override object Convert(object from, Type toType)
    {
        if (from == null)
        {
            return null;
        }

        DateTime dt = (DateTime)from;
        if (dt == default(DateTime))
            return default(DateTimeOffset);

        return new DateTimeOffset(dt, TimeSpan.Zero);
    }

    public override bool TryConvert(object from, Type toType, out object to)
    {
        if (from == null)
        {
            to = null;
            return true;
        }
        DateTime dt = (DateTime)from;
        if (dt == default(DateTime))
        {
            to = default(DateTimeOffset);
            return true;
        }
        to = new DateTimeOffset(dt, TimeSpan.Zero);
        return true;

    }

}

//Datetimeoffset to datetime
internal class DateTimeOffsetToDateTimeConverter : ConverterBase<DateTimeOffset, DateTime>
{
    public override object Convert(object from, Type toType)
    {
        DateTimeOffset _from = (DateTimeOffset)from;
        if (_from.Offset != TimeSpan.Zero)
        {
            _from = _from.ToOffset(TimeSpan.Zero);
        }
        return _from.DateTime;
    }

    public override bool TryConvert(object from, Type toType, out object to)
    {
        DateTimeOffset _from = (DateTimeOffset)from;
        if (_from.Offset != TimeSpan.Zero)
        {
            _from = _from.ToOffset(TimeSpan.Zero);
        }
        to = _from.DateTime;
        return true;
    }
}

internal class DateTimeOffsetNullableToDateTimeNullableConverter : ConverterBase<DateTimeOffset?, DateTime?>
{
    public override object Convert(object from, Type toType)
    {
        if (from == null)
        {
            return null;
        }

        DateTimeOffset _from = (DateTimeOffset)from;
        if (_from.Offset != TimeSpan.Zero)
        {
            _from = _from.ToOffset(TimeSpan.Zero);
        }
        return _from.DateTime;
    }

    public override bool TryConvert(object from, Type toType, out object to)
    {
        if (from == null)
        {
            to = null;
            return true;
        }
        DateTimeOffset _from = (DateTimeOffset)from;
        if (_from.Offset != TimeSpan.Zero)
        {
            _from = _from.ToOffset(TimeSpan.Zero);
        }
        to = _from.DateTime;
        return true;
    }
}

internal class DateTimeOffsetStrConverter : ConverterBase<string, DateTimeOffset>
{
    public override object Convert(object from, Type toType)
    {
        string _from = (string)from;
        if (_from == "0001-01-01T00:00:00")
            return default(DateTimeOffset);

        if (_from == "0001-01-01")
            return default(DateTimeOffset);

        if (DateTimeOffset.TryParse(_from, out DateTimeOffset result))
            return result;

        else throw new FormatException($"Invalid date format: {from}");

    }

    public override bool TryConvert(object from, Type toType, out object to)
    {
        string _from = (string)from;
        if (_from == "0001-01-01T00:00:00")
        {
            to = default(DateTimeOffset);
            return true;
        }
        if (_from == "0001-01-01")
        {
            to = default(DateTimeOffset);
            return true;
        }
        if (DateTimeOffset.TryParse(_from, out DateTimeOffset result))
        {
            to = result;
            return true;
        }
        else
        {
            to = null;
            return false;
        }
    }
}
