using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Extensions;

internal static class ObjectExt
{
    private static readonly Dictionary<string, PropertyInfo[]> _propertyCache = new();

    internal static object? GetValueFromProperty(this object? target, string? propertyPath)
    {
        if (target == null || string.IsNullOrEmpty(propertyPath))
            return null;

        if (target is INotifyPropertyChangedFast propertyExchange)
            return propertyExchange.GetPropertyValue(propertyPath);
        else
            return GetDeepPropertyValue(target, propertyPath);
    }

    public static object? GetDeepPropertyValue(object instance, string path)
    {
        if (!_propertyCache.TryGetValue(path, out var cachedProperties))
        {
            cachedProperties = BuildPropertyPathCache(instance.GetType(), path);
            if (cachedProperties == null)
                return null;

            _propertyCache[path] = cachedProperties;
        }

        return GetPropertyValueFromCache(instance, cachedProperties);
    }

    private static PropertyInfo[]? BuildPropertyPathCache(Type type, string path)
    {
        // Изначально массив PropertyInfo выделяется с запасом
        var properties = new PropertyInfo[path.Length / 2];
        int count = 0;

        int start = 0;
        for (int i = 0; i <= path.Length; i++)
        {
            if (i == path.Length || path[i] == '.')
            {
                var segment = path.AsSpan(start, i - start);
                var propInfo = type.GetProperty(segment.ToString());
                if (propInfo == null)
                    return null;

                properties[count++] = propInfo;
                type = propInfo.PropertyType;
                start = i + 1;
            }
        }

        // Обрезаем массив до актуального размера
        Array.Resize(ref properties, count);
        return properties;
    }

    private static object? GetPropertyValueFromCache(object instance, PropertyInfo[] properties)
    {
        foreach (var propInfo in properties)
        {
            instance = propInfo.GetValue(instance, null)!;
            if (instance == null) 
                return null;
        }
        return instance;
    }
}