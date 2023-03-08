using DataGridSam.Extensions;
using DataGridSam.Internal;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam
{
    public class Row : Grid, IDataTriggerHost
    {
        private readonly DataGridColumn[] cols;
        private readonly Cell[] cells;
        private readonly List<IDataTrigger> enabledTriggers = new();
        private INotifyPropertyChanged? lastContext;
        private IEnumerable<IDataTrigger>? triggers;
        private int totalTriggerCount;

        public Row(int columns)
        {
            RowDefinitions = new() { new() { Height = GridLength.Auto } };
            ColumnSpacing = 0;
            cells = new Cell[columns];
            cols = new DataGridColumn[columns];
        }

        public Color? TextColor { get; set; }
        public double? FontSize { get; set; }
        public FontAttributes? FontAttributes { get; set; }
        public TextAlignment? VerticalTextAlignment { get; set; }
        public TextAlignment? HorizontalTextAlignment { get; set; }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (lastContext != null)
                lastContext.PropertyChanged -= Notify_PropertyChanged;

            if (BindingContext is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged += Notify_PropertyChanged;
                lastContext = notify;
            }
            else
            {
                lastContext = null;
            }

            enabledTriggers.Clear();
            UpdateTriggers();
        }

        private void Notify_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (totalTriggerCount == 0)
                return;

            foreach (var item in triggers!)
            {
                var binding = item.Binding as Binding;
                if (binding == null)
                    throw new NotSupportedException();

                if (e.PropertyName == binding.Path)
                {
                    object? value = BindingContext?.GetValueFromProperty(binding.Path);
                    View view;
                    if (item.CellTriggerId != null)
                        view = cells[item.CellTriggerId.Value];
                    else
                        view = this;

                    if (view is IDataTriggerHost h)
                        h.Execute(item, value);
                }
            }
        }

        private void UpdateTriggers()
        {
            foreach (var item in triggers!)
            {
                var binding = item.Binding as Binding;
                if (binding == null)
                    throw new NotSupportedException();

                object? value = BindingContext?.GetValueFromProperty(binding.Path);

                View view;
                if (item.CellTriggerId != null)
                    view = cells[item.CellTriggerId.Value];
                else
                    view = this;

                if (view is IDataTriggerHost h)
                    h.Execute(item, value);
            }
        }

        public void InitCell(DataGridColumn col)
        {
            int colId = col.Index;

            var cell = new Cell(col, this);
            cells[colId] = cell;
            cols[colId] = col;

            this.SetColumn(cell, colId);
            this.Add(cell);
        }

        internal void SetTriggers(IEnumerable<IDataTrigger> triggers, int totalTriggerCount)
        {
            this.triggers = triggers;
            this.totalTriggerCount = totalTriggerCount;
        }

        public void Draw()
        {
            foreach (var cell in cells)
                cell.Draw();
        }

        void IDataTriggerHost.Execute(IDataTrigger trigger, object? value)
        {
            bool isEnabled = trigger.IsEqualValue(value);

            if (isEnabled)
            {
                if (!enabledTriggers.Contains(trigger))
                    enabledTriggers.Add(trigger);
            }
            else
            {
                enabledTriggers.Remove(trigger);
            }

            BackgroundColor = enabledTriggers.FirstNonNull(x => x.BackgroundColor);
            TextColor = enabledTriggers.FirstNonNull(x => x.TextColor);
            FontSize = enabledTriggers.FirstNonNull(x => x.FontSize);
            FontAttributes = enabledTriggers.FirstNonNull(x => x.FontAttributes);
            VerticalTextAlignment = enabledTriggers.FirstNonNull(x => x.VerticalTextAlignment);
            HorizontalTextAlignment = enabledTriggers.FirstNonNull(x => x.HorizontalTextAlignment);

            Draw();
        }
    }
}
