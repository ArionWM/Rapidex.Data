using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex;

public static class NumericsExtensions
{
    //long ve int için tekil ya da çift satı kontrol fonksiyonları
    public static bool IsOdd(this long value)
    {
        return value % 2 != 0;
    }

    public static bool IsEven(this long value)
    {
        return value % 2 == 0;
    }

    public static bool IsOdd(this int value)
    {
        return value % 2 != 0;
    }

    public static bool IsEven(this int value)
    {
        return value % 2 == 0;
    }

    public static int DigitCount(this int value)
    {
        var digitCount = Math.Floor(Math.Log10(value)) + 1;
        return digitCount.As<int>();
    }

    public static bool IsBetween(this int? value, int valueStart, int valueEnd)
    {
        if (value == null)
            return false;
        if (valueStart > valueEnd)
        {
            (valueStart, valueEnd) = (valueEnd, valueStart);
        }

        return value >= valueStart && value <= valueEnd;
    }

    public static bool IsBetween(this int value, int valueStart, int valueEnd)
    {
        if (valueStart > valueEnd)
        {
            (valueStart, valueEnd) = (valueEnd, valueStart);
        }

        return value >= valueStart && value <= valueEnd;
    }

    public static bool IsBetween(this decimal value, decimal valueStart, decimal valueEnd)
    {
        if (valueStart > valueEnd)
        {
            (valueStart, valueEnd) = (valueEnd, valueStart);
        }

        return value >= valueStart && value <= valueEnd;
    }

    public static bool IsBetween(this decimal? value, decimal valueStart, decimal valueEnd)
    {
        if (value == null)
            return false;

        if (valueStart > valueEnd)
        {
            (valueStart, valueEnd) = (valueEnd, valueStart);
        }

        return value >= valueStart && value <= valueEnd;
    }
}
