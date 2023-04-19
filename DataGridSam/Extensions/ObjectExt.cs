using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Extensions
{
    internal static class ObjectExt
    {
        internal static object? GetValueFromProperty(this object target, string? propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath))
                return null;

            if (target is INotifyPropertyChangedFast propertyExchange)
                return propertyExchange.GetPropertyValue(propertyPath);
            else
                return GetDeepPropertyValue(target, propertyPath);
        }

        private static object? GetDeepPropertyValue(object? instance, string path)
        {
            var branches = path.Split('.');
            var t = instance!.GetType();
            foreach (var branch in branches)
            {
                var propInfo = t.GetProperty(branch);
                if (propInfo != null)
                {
                    instance = propInfo.GetValue(instance, null);
                    t = propInfo.PropertyType;
                }
                else throw new ArgumentException("Properties path is not correct");
            }
            return instance;
        }
    }
}
