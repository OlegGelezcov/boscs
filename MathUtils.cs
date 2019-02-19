using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class MathUtils
{
    public static readonly double DoubleMinNormal = 2.225074e-308;
    public static readonly double DoubleMinDenormal = double.Epsilon;

    private static bool IsFlushToZeroEnabled
        => DoubleMinDenormal == 0.0;

    private static double DoubleEpsilon
        => (!IsFlushToZeroEnabled) ? DoubleMinDenormal : DoubleMinNormal;

    public static bool Approximately(double a, double b) {
        return Math.Abs(b - a) < Math.Max(1e-15 * Math.Max(Math.Abs(a), Math.Abs(b)), DoubleEpsilon * 8);
    }

    public static double Sum(int from, int to, Func<double, double> fct)
    {
        double rv = 0;

        if (from == to)
            return fct(from);

        for (int x = from; x < to; x++)
        {
            rv += fct(x);
        }

        return rv;
    }
}
