using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Infrastructure;


public static class EnumerableExtensions
{
    public static string ToReportTable<T>(this IEnumerable<T> source, params Func<T, object?>[] columns)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(columns);
        var rows = source.Select(item => string.Join(" | ", columns.Select(c => c(item)))).ToList();
        return rows.Count == 0 ? "(нет данных)" : string.Join(Environment.NewLine, rows);
    }
}

