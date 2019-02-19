using Bos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyNumber  {

    private double value;

    public CurrencyNumber() 
        : this(0.0) { }

    public CurrencyNumber(double val) {
        value = val;
    }

    public void Add(double val) {
        value += val;
    }

    public void Sub(double val) {
        value -= val;
    }

    public override string ToString() {
        return PrettyString(separator: " ");
    }

    public void SetValue(double val) {
        value = val;
    }

    public double Value
        => value;

    public string AbbreviationColored(string numColor, string suffixColor) {
        string[] abbrArr = NumberMinifier.PrettyAbbreviationValueSeparated(value);
        string result = abbrArr[0].Colored(numColor);
        if(!string.IsNullOrEmpty(abbrArr[1])) {
            result += " " + abbrArr[1].Colored(suffixColor);
        }
        return result;
    }

    public string[] AbbreviationColoredComponents(string numColor, string suffixColor) {
        string[] abbrArr = NumberMinifier.PrettyAbbreviationValueSeparated(value);
        string result = (!string.IsNullOrEmpty(numColor) ? abbrArr[0].Colored(numColor) : abbrArr[0]);
        string result2 = (!string.IsNullOrEmpty(suffixColor) ? abbrArr[1].Colored(suffixColor) : abbrArr[1]);
        return new string[] { result, result2 };

    }

    public enum CurrencySymbolPosition {
        Left,
        Right
    }

    public string[] LegacyComponents(CurrencySymbolPosition currencySymbolPosition = CurrencySymbolPosition.Left) {
        string[] result  = { string.Empty, string.Empty };
        string componentString = NumberMinifier.PrettyAbbreviatedValue(Value, false, "|");
        string[] components = componentString.Split(new char[] { '|' });
        switch(currencySymbolPosition) {
            case CurrencySymbolPosition.Left: {
                    result[0] = $"${components[0]}";
                }
                break;
            case CurrencySymbolPosition.Right: {
                    result[0] = $"{components[0]}$";
                }
                break;
        }
        if(components.Length > 1) {
            result[1] = components[1];
        }
        return result;
    }

    public string[] AbbreviationComponents()
        => NumberMinifier.PrettyAbbreviationValueSeparated(value);

    public string Abbreviation
        => NumberMinifier.PrettyAbbreviatedValue(value);

    public string PrefixedAbbreviation(string prefix = "$")
        => prefix + Abbreviation;

    public string[] Pretty
        => NumberMinifier.PrettyAbbreviationValueSeparated(value);

    public string PrettySuffix
        => NumberMinifier.PrettyAbbreviationValueSeparated(value)[1];

    public string PrettyPrefix
        => NumberMinifier.PrettyAbbreviationValueSeparated(value)[0];

    public string PrettyString(string separator = "\r\n") {
        string[] prettyArray = Pretty;
        if(string.IsNullOrEmpty(prettyArray[1])) {
            return prettyArray[0];
        } else {
            return prettyArray[0] + separator + prettyArray[1];
        }
    }

    /*
    public static CurrencyNumber operator+(CurrencyNumber n1, CurrencyNumber n2) {
        return new CurrencyNumber(n1.value + n2.value);
    }

    public static CurrencyNumber operator-(CurrencyNumber n1, CurrencyNumber n2) {
        return new CurrencyNumber(n1.value - n2.value);
    }

    public static CurrencyNumber operator*(CurrencyNumber n1, CurrencyNumber n2) {
        return new CurrencyNumber(n1.value * n2.value);
    }

    public static CurrencyNumber operator/(CurrencyNumber n1, CurrencyNumber n2) {
        return new CurrencyNumber(n1.value / n2.value);
    }

    public static CurrencyNumber operator+(CurrencyNumber n, double d) {
        return new CurrencyNumber(n.value + d);
    }

    public static CurrencyNumber operator+(double d, CurrencyNumber n) {
        return n + d;
    }

    public static CurrencyNumber operator-(CurrencyNumber n, double d) {
        return new CurrencyNumber(n.value - d);
    }

    public static CurrencyNumber operator-(double d, CurrencyNumber n) {
        return new CurrencyNumber(d - n.value);
    }

    public static CurrencyNumber operator*(CurrencyNumber n, double d) {
        return new CurrencyNumber(n.value * d);
    }

    public static CurrencyNumber operator*(double d, CurrencyNumber n) {
        return n * d;
    }

    public static CurrencyNumber operator/(CurrencyNumber n, double d) {
        return new CurrencyNumber(n.value / d);
    }

    public static CurrencyNumber operator/(double d, CurrencyNumber n) {
        return new CurrencyNumber(d / n.value);
    }

    public static CurrencyNumber operator+(CurrencyNumber n, int i) {
        return new CurrencyNumber(n.value + i);
    }

    public static CurrencyNumber operator+(int i, CurrencyNumber n) {
        return n + i;
    }

    public static CurrencyNumber operator-(CurrencyNumber n, int i) {
        return new CurrencyNumber(n.value - i);
    }

    public static CurrencyNumber operator-(int i, CurrencyNumber n) {
        return new CurrencyNumber(i - n.value);
    }

    public static CurrencyNumber operator*(CurrencyNumber n, int i) {
        return new CurrencyNumber(n.value * i);
    }

    public static CurrencyNumber operator*(int i, CurrencyNumber n) {
        return n * i;
    }

    public static CurrencyNumber operator/(CurrencyNumber n, int i) {
        return new CurrencyNumber(n.value / i);
    }

    public static CurrencyNumber operator/(int i, CurrencyNumber n) {
        return new CurrencyNumber(i / n.value);
    }
    */


}
