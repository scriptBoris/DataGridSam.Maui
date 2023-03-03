using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Extensions
{
    // todo Нужно ли клонирование триггеров?
    internal static class TriggerExt
    {
        internal static TriggerBase Clone(this TriggerBase trigger)
        {
            return trigger;
            switch (trigger)
            {
                case DataTrigger dt:
                    return dt.Clone();

                default:
                    throw new NotSupportedException();
            }
        }

        internal static DataTrigger Clone(this DataTrigger trigger)
        {
            Type type = trigger.GetType();
            var clone = (DataTrigger)Activator.CreateInstance(type, trigger.TargetType)!;
            clone.Binding = trigger.Binding;
            clone.Value = trigger.Value;

            foreach (var item in trigger.Setters)
                clone.Setters.Add(item);

            return clone;
        }
    }
}
