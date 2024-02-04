﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ImageFanReloaded.CommonTypes.Disc.Implementation;

public class NaturalSortingComparer : IComparer<string>
{
    static NaturalSortingComparer()
    {
        ContiguousDigitBlockRegex = new Regex(@"\d+", RegexOptions.Compiled);
    }

    public int Compare(string x, string y)
    {
        if (!ContainsDigits(x) || !ContainsDigits(y))
        {
            return x.CompareTo(y);
        }

        var maximumContiguousDigitBlockLengthX = GetMaximumContiguousDigitBlockLength(x);
        var maximumContiguousDigitBlockLengthY = GetMaximumContiguousDigitBlockLength(y);

        var maximumContiguousDigitBlockLength = Math.Max(
            maximumContiguousDigitBlockLengthX, maximumContiguousDigitBlockLengthY);

        var paddedX = PadDigitBlocks(x, maximumContiguousDigitBlockLength);
        var paddedY = PadDigitBlocks(y, maximumContiguousDigitBlockLength);

        return paddedX.CompareTo(paddedY);
    }

    #region Private

    private static readonly Regex ContiguousDigitBlockRegex;

    private static bool ContainsDigits(string text) => ContiguousDigitBlockRegex.IsMatch(text);

    private static int GetMaximumContiguousDigitBlockLength(string text)
    {
        var maximumContiguousDigitBlockLength =
            ContiguousDigitBlockRegex
                .Matches(text)
                .Cast<Match>()
                .Select(aMatch => aMatch.Value.Length)
                .Max();

        return maximumContiguousDigitBlockLength;
    }

    private static string PadDigitBlocks(string text, int digitBlockLength)
    {
        var paddedText = ContiguousDigitBlockRegex.Replace(
            text,
            aMatch => aMatch.Value.PadLeft(digitBlockLength, '0'));

        return paddedText;
    }

    #endregion
}
