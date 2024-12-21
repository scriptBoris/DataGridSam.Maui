using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGridSam.Internal;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace DataGridSam.Extensions;

internal static class TriggerExt
{
    internal static void InitTrigger(this IDataTrigger trigger, DataGrid dataGrid)
    {
        object? csharpValue = null;
        trigger.DataGrid = dataGrid;

        if (trigger.Value is string s)
        {
            if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase))
            {
                csharpValue = true;
            }
            else if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase))
            {
                csharpValue = false;
            }
        }

        trigger.CSharpValue = csharpValue;
    }

    internal static void InitTriggers(this IEnumerable<IDataTrigger> triggers, DataGrid dataGrid)
    {
        foreach (var item in triggers)
        {
            item.InitTrigger(dataGrid);
        }
    }

    internal static bool IsEqualValue(this IDataTrigger trigger, object? value)
    {
        bool isEqualValue = false;
        
        if (trigger.CSharpValue != null)
        {
            isEqualValue = trigger.CSharpValue.Equals(value);
        }
        else
        {
            isEqualValue = trigger.Value?.Equals(value) ?? false;
        }

        return isEqualValue;
    }

    internal static object? FirstNonNull(this IEnumerable<IDataTrigger> triggers, Func<IDataTrigger, object?> select)
    {
        foreach (var item in triggers)
        {
            var res = select(item);

            if (res != null)
                return res;
        }
        return null;
    }

    internal static T? FirstNonNull<T>(this IList<IDataTrigger> triggers, Func<IDataTrigger, T?> select)
    {
        if (triggers.Count == 0)
            return default;

        foreach (var item in triggers)
        {
            var res = select(item);

            if (res is T t)
                return t;
        }
        return default;
    }
}