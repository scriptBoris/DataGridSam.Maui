using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Extensions;

internal static class ObjectExt
{
    private static readonly Dictionary<CacheKey, CacheItem> _propertyCache = new();

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
        var type = instance.GetType();
        var key = new CacheKey
        {
            Type = type,
            Path = path,
        };

        if (!_propertyCache.TryGetValue(key, out var cachedProperties))
        {
            cachedProperties = BuildPropertyPathCache(type, path);
            if (cachedProperties == null)
                return null;

            _propertyCache.Add(key, cachedProperties);
        }

        return GetPropertyValueFromCache(instance, cachedProperties);
    }

    private static CacheItem? BuildPropertyPathCache(Type type, string path)
    {
        CacheItem? link = null;

        int start = 0;
        for (int i = 0; i <= path.Length; i++)
        {
            if (i == path.Length || path[i] == '.')
            {
                var segment = path.AsSpan(start, i - start);
                var propInfo = type.GetProperty(segment.ToString());
                if (propInfo == null)
                    return null;

                link ??= new();
                link.AddLast(propInfo);
                type = propInfo.PropertyType;
                start = i + 1;
            }
        }

        return link;
    }

    private static object? GetPropertyValueFromCache(object instance, CacheItem properties)
    {
        foreach (var propInfo in properties)
        {
            instance = propInfo.GetValue(instance, null)!;
            if (instance == null) 
                return null;
        }
        return instance;
    }

    private readonly struct CacheKey
    {
        public Type Type { get; init; }
        public string Path { get; init; }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is CacheKey v2)
            {
                return Type == v2.Type && Path == v2.Path;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type.GetHashCode(), Path.GetHashCode());
        }

        public override string ToString()
        {
            return $"{Type.FullName}: {Path}";
        }
    }

    private class CacheItem : LinkedList<PropertyInfo>
    {
    }
}