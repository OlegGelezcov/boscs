using System;

public class LogarithmicScoreConverter : IValueConverter
{
    public int ConvertTo(double balance, int multiplier = 1000)
    {
        return (int)(Math.Log10(balance) * multiplier);
    }

    public string ConvertFrom(int score, int multiplier = 1000)
    {
        var dbl = Math.Pow(10, score / multiplier);
        return NumberMinifier.PrettyAbbreviatedValue(dbl, false, " ");
    }
}