using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


public static class LinqExtensions
{
    public static T RandomElement<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var r = new Random();
        source = source.Where(predicate);
        return source.Skip(r.Next(source.Count())).FirstOrDefault();
    }

    public static T RandomElement<T>(this IEnumerable<T> source)
    {
        var r = new Random();
        return source.Skip(r.Next(source.Count())).FirstOrDefault();
    }
}

