using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Currency
{
    public string Name { get; set; }
    public double UniversalExchangeRate { get; set; }
    public string CurrencySign { get; set; }
    public bool SignPlacedOnLeft { get; set; }

    public override string ToString()
    {
        return CurrencySign;
    }


    public string CreatePriceString(double value, bool separateWithEndl = false, string separator = "", bool useDecimalFormat = false)
    {
        string format = null;
        if (SignPlacedOnLeft)
        {
            format = CurrencySign + " {0}";
        }
        else
        {
            format = "{0} " + CurrencySign;
        }

        var str = NumberMinifier.PrettyAbbreviatedValue(value, separateWithEndl, separator, !useDecimalFormat);
        return string.Format(format, str);
    }

    public string[] CreatePriceStringSeparated(double value)
    {
        string format = null;
        if (SignPlacedOnLeft)
        {
            format = CurrencySign + "{0}";
        }
        else
        {
            format = "{0}" + CurrencySign;
        }

        string qstr = NumberMinifier.PrettyAbbreviatedValue(value, false, "|");
        if (qstr == null)
            Debug.Break();
        var str = qstr.Split('|');
        str[0] = string.Format(format, str[0]);
        return str;
    }
}

public class Currencies
{
    public static readonly Currency Dollar = new Currency() { Name = "Dollars", UniversalExchangeRate = 1, CurrencySign = "$", SignPlacedOnLeft = true };
    public static readonly Currency RON = new Currency() { Name = "Lei", UniversalExchangeRate = 4.52, CurrencySign = "RON", SignPlacedOnLeft = false };
    public static readonly Currency Investors = new Currency() { Name = "Investors", UniversalExchangeRate = 1, CurrencySign = "Investors", SignPlacedOnLeft = false };

    public static readonly List<Currency> All = new List<Currency>() { Dollar, RON };
    public static readonly Currency DefaultCurrency = Dollar;

    public static Currency GetFromString(string sign)
    {
        foreach (var x in All)
        {
            if (x.CurrencySign == sign)
                return x;
        }

        return DefaultCurrency;
    }


}
