using DataGridSam.Extensions;
using DataGridSam.Internal;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam
{
    public class Row : Layout, ILayoutManager, IDataTriggerHost
    {
        private readonly DataGrid _dataGrid;
        private readonly Cell[] _cells;
        private readonly List<IDataTrigger> _enabledTriggers = new();

        private INotifyPropertyChanged? lastContext;
        private IList<IDataTrigger>? triggers;

        public Row(DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
            _cells = new Cell[dataGrid.Columns.Count];

            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                var col = dataGrid.Columns[i];
                var cell = new Cell(col, this);
                
                _cells[i] = cell;
                Children.Add(cell);
            }
        }

        public Color? TextColor { get; set; }
        public double? FontSize { get; set; }
        public FontAttributes? FontAttributes { get; set; }
        public TextAlignment? VerticalTextAlignment { get; set; }
        public TextAlignment? HorizontalTextAlignment { get; set; }
        private double[] Widths => _dataGrid.CachedWidths;

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
            // todo нужно ли отчищать сработанные тригеры ячеек???
            _enabledTriggers.Clear();
            UpdateTriggers();
        }

        private void Notify_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (triggers == null || triggers.Count == 0)
                return;

            foreach (var trigger in triggers!)
            {
                var binding = trigger.Binding as Binding;
                if (binding == null)
                    throw new NotSupportedException();

                if (e.PropertyName == binding.Path)
                {
                    object? value = BindingContext?.GetValueFromProperty(binding.Path);
                    View view;
                    if (trigger.CellTriggerId != null)
                        view = _cells[trigger.CellTriggerId.Value];
                    else
                        view = this;

                    if (view is IDataTriggerHost h)
                        h.Execute(trigger, value);
                }
            }
        }

        private void UpdateTriggers()
        {
            foreach (var trigger in triggers!)
            {
                var binding = trigger.Binding as Binding;
                if (binding == null)
                    throw new NotSupportedException();

                object? value = BindingContext?.GetValueFromProperty(binding.Path);

                View view;
                if (trigger.CellTriggerId != null)
                    view = _cells[trigger.CellTriggerId.Value];
                else
                    view = this;

                if (view is IDataTriggerHost h)
                    h.Execute(trigger, value);
            }
        }

        internal void SetTriggers(IList<IDataTrigger> triggers)
        {
            this.triggers = triggers;
        }

        public void Draw()
        {
            foreach (var cell in _cells)
                cell.Draw();
        }

        void IDataTriggerHost.Execute(IDataTrigger trigger, object? value)
        {
            bool isEnabled = trigger.IsEqualValue(value);

            if (isEnabled)
            {
                if (!_enabledTriggers.Contains(trigger))
                    _enabledTriggers.Add(trigger);
            }
            else
            {
                _enabledTriggers.Remove(trigger);
            }

            BackgroundColor = _enabledTriggers.FirstNonNull(x => x.BackgroundColor);
            TextColor = _enabledTriggers.FirstNonNull(x => x.TextColor);
            FontSize = _enabledTriggers.FirstNonNull(x => x.FontSize);
            FontAttributes = _enabledTriggers.FirstNonNull(x => x.FontAttributes);
            VerticalTextAlignment = _enabledTriggers.FirstNonNull(x => x.VerticalTextAlignment);
            HorizontalTextAlignment = _enabledTriggers.FirstNonNull(x => x.HorizontalTextAlignment);

            Draw();
        }

        protected override ILayoutManager CreateLayoutManager()
        {
            return this;
        }

        public Size ArrangeChildren(Rect bounds)
        {
            double x = 0;
            for (int i = 0; i < _cells.Length; i++)
            {
                var cell = _cells[i];
                double w = Widths[i];
                double h = bounds.Size.Height;

                var rect = new Rect(x, 0, w, h);
                ((IView)cell).Arrange(rect);

                x += w;
            }

            return bounds.Size;
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            double h = 0;
            for (int i = 0; i < _cells.Length; i++)
            {
                var cell = _cells[i];
                double w = Widths[i];

                var m = ((IView)cell).Measure(w, heightConstraint);

                if (m.Height > h)
                    h = m.Height;
            }

            return new Size(widthConstraint, h);
        }

        internal static double[] Calculate(GridLength[] viewRules, double availableWidth)
        {
            double[] result = new double[viewRules.Length];
            double totalSizeStar = 0;
            double totalSizePixel = 0;
            double freeSpace = availableWidth;

            // Сначала проходим по всем элементам и считаем общую сумму значений GridLength в Star и Pixel
            for (int i = 0; i < viewRules.Length; i++)
            {
                if (viewRules[i].IsStar)
                {
                    totalSizeStar += viewRules[i].Value;
                }
                else if (viewRules[i].IsAbsolute)
                {
                    totalSizePixel += viewRules[i].Value;
                }
            }

            freeSpace -= totalSizePixel;
            if (freeSpace < 0)
                freeSpace = 0;

            // Затем проходим по всем элементам и вычисляем их фактические размеры
            for (int i = 0; i < viewRules.Length; i++)
            {
                double pixelSize = 0;
                double starSize = 0;

                if (viewRules[i].IsStar)
                {
                    if (viewRules[i].Value > 0 && totalSizeStar > 0)
                        starSize = freeSpace * (viewRules[i].Value / totalSizeStar);
                }
                else if (viewRules[i].IsAbsolute)
                {
                    pixelSize = viewRules[i].Value;
                }

                result[i] = pixelSize + starSize;
            }

            return result;
        }
    }
}
