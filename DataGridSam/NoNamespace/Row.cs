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
    public class Row : Layout, ILayoutManager
    {
        private readonly DataGrid _dataGrid;
        private readonly Cell[] _cells;
        private readonly List<IDataTrigger> _enabledTriggers = new();

        private INotifyPropertyChanged? lastContext;
        private IList<IDataTrigger>? triggers;
        private bool isPressed;
        private Color beforeAnimationColor;
        private Color externalBackgroundColor = Colors.Transparent;

        public Row(DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
            _cells = new Cell[dataGrid.Columns.Count];

            BackgroundColor = dataGrid.CellBackgroundColor;
            beforeAnimationColor = BackgroundColor;

            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                var col = dataGrid.Columns[i];
                var cell = new Cell(col, this);
                
                _cells[i] = cell;
                Children.Add(cell.Content);
            }
        }

        public Color? LogicalBackgroundColor { get; private set; }
        public Color? TextColor { get; set; }
        public double? FontSize { get; set; }
        public FontAttributes? FontAttributes { get; set; }
        public TextAlignment? VerticalTextAlignment { get; set; }
        public TextAlignment? HorizontalTextAlignment { get; set; }
        public Color TapColor => _dataGrid.TapSelectedColor;

        private double[] Widths => _dataGrid.CachedWidths;
        private IDispatcherTimer? Timer { get; set; }

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
            if (triggers == null || triggers.Count == 0 || BindingContext == null)
                return;

            foreach (var trigger in triggers!)
            {
                var binding = trigger.Binding as Binding;
                if (binding == null)
                    throw new NotSupportedException();

                if (e.PropertyName == binding.Path)
                {
                    object? value = BindingContext.GetValueFromProperty(binding.Path);
                    Cell? cell = null;
                    if (trigger.CellTriggerId != null)
                        cell = _cells[trigger.CellTriggerId.Value];

                    cell?.ExecuteTrigger(trigger, value);
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
                Cell? cell = null;
                if (trigger.CellTriggerId != null)
                    cell = _cells[trigger.CellTriggerId.Value];

                cell?.ExecuteTrigger(trigger, value);
            }
        }

        internal void SetTriggers(IList<IDataTrigger> triggers)
        {
            this.triggers = triggers;
        }

        internal void UpdateVisual()
        {
            BackgroundColor = LogicalBackgroundColor ?? _dataGrid.CellBackgroundColor;
            beforeAnimationColor = BackgroundColor;

            foreach (var cell in _cells)
                cell.UpdateVisual();
        }

        internal void ExecuteTrigger(IDataTrigger trigger, object? value)
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

            LogicalBackgroundColor = _enabledTriggers.FirstNonNull(x => x.BackgroundColor);
            TextColor = _enabledTriggers.FirstNonNull(x => x.TextColor);
            FontSize = _enabledTriggers.FirstNonNull(x => x.FontSize);
            FontAttributes = _enabledTriggers.FirstNonNull(x => x.FontAttributes);
            VerticalTextAlignment = _enabledTriggers.FirstNonNull(x => x.VerticalTextAlignment);
            HorizontalTextAlignment = _enabledTriggers.FirstNonNull(x => x.HorizontalTextAlignment);

            UpdateVisual();
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
                ((IView)cell.Content).Arrange(rect);

                x += w + _dataGrid.BordersThickness;
            }

            return bounds.Size;
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            // на iOS срабатывает Measure строки быстре, чем в родительских контейнерах,
            // по этому первично надо вычислить кэш ширины ячеек строк
#if IOS
            if (Widths.Length == 0)
                _dataGrid.UpdateCellsWidthCache(widthConstraint, false);
#endif

            double h = 0;
            for (int i = 0; i < _cells.Length; i++)
            {
                var cell = _cells[i];
                double w = Widths[i];

                var m = ((IView)cell.Content).Measure(w, heightConstraint);

                if (m.Height > h)
                    h = m.Height;
            }

            return new Size(widthConstraint, h);
        }

        internal static double[] CalculateWidthRules(GridLength[] viewRules, double availableWidth)
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

        public void OnTapStart_Common()
        {
            this.CancelAnimation();
            this.AnimateBackgroundColor(TapColor, 100);
            OnTapStart();
        }

        public void OnTapFinish_Common(bool isThrowTap, bool forceLongTap = false)
        {
            this.CancelAnimation();
            this.AnimateBackgroundColorRestore(400);

            if (isThrowTap)
                OnTapFinish(forceLongTap ? ThrowTapMode.LongTap : ThrowTapMode.Tap);
            else
                OnTapFinish(ThrowTapMode.Cancel);
        }

        public void OnTapStart()
        {
            isPressed = true;
            if (Timer == null)
            {
                Timer = this.Dispatcher.CreateTimer();
                Timer.Interval = TimeSpan.FromMilliseconds(_dataGrid.LongTapTimeout);
                Timer.IsRepeating = false;
                Timer.Tick += Timer_Tick;
                Timer.Start();
            }
            else
            {
                Timer.Stop();
                Timer.Start();
            }
        }

        public void OnTapFinish(ThrowTapMode mode)
        {
            if (!isPressed)
                return;

            isPressed = false;
            Timer?.Stop();

            var finalMode = mode;
            if (mode == ThrowTapMode.LongTap && _dataGrid.RowLongSelectedCommand == null)
                finalMode = ThrowTapMode.Tap;

            switch (finalMode)
            {
                case ThrowTapMode.Tap:
                    bool can = _dataGrid.RowSelectedCommand?.CanExecute(BindingContext) ?? false;
                    if (can)
                        _dataGrid.RowSelectedCommand?.Execute(BindingContext);
                    break;

                case ThrowTapMode.LongTap:
                    bool canLong = _dataGrid.RowLongSelectedCommand?.CanExecute(BindingContext) ?? false;
                    if (canLong)
                        _dataGrid.RowLongSelectedCommand?.Execute(BindingContext);
                    break;

                case ThrowTapMode.Cancel:
                default:
                    break;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            OnTapFinish(ThrowTapMode.LongTap);
        }

        /// <summary>
        /// Sets the background of the row and cells to the specified
        /// </summary>
        /// <param name="color">New color</param>
        /// <param name="fill">Percent setup, from 0 to 1, where 1 is full setup color</param>
        public void SetRowBackgroundColor(Color color, double fill = 1)
        {
            BackgroundColor = VisualExtensions.MixColor(beforeAnimationColor, color, fill);
            externalBackgroundColor = color;

            for (int i = 0; i < _cells.Length; i++)
            {
                var cell = _cells[i];
                var bgitem = cell.BeforeAnimationColor ?? Colors.Transparent;
                cell.Content.BackgroundColor = VisualExtensions.MixColor(bgitem, color, fill);
                cell.ExternalBackgroundColor = color;
            }
        }

        /// <summary>
        /// Restores the visual part of a string to the original DataGrid state
        /// </summary>
        /// <param name="fill">Percent resore, from 0 to 1, where 1 is full restore</param>
        public void RestoreRowBackgroundColor(double fill = 1)
        {
            BackgroundColor = VisualExtensions.MixColor(externalBackgroundColor, beforeAnimationColor, fill);

            for (int i = 0; i < _cells.Length; i++)
            {
                var cell = _cells[i];
                var bgitem = cell.BeforeAnimationColor ?? Colors.Transparent;
                cell.Content.BackgroundColor = VisualExtensions.MixColor(cell.ExternalBackgroundColor, bgitem, fill);
            }
        }

        public enum ThrowTapMode
        {
            Cancel,
            Tap,
            LongTap,
        }
    }

    public static class VisualExtensions
    {
        public const string AnimationName = "ColorTo";

        public static Task<bool> AnimateBackgroundColorRestore(this Row self, uint length = 250, Easing? easing = null)
        {
            easing ??= Easing.Linear;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            var anim = new Animation((x) =>
            {
                self.RestoreRowBackgroundColor(x);
            }, 0, 1, easing);

            anim.Commit(self, AnimationName, length: length, easing: easing, finished: (v, b) =>
            {
                taskCompletionSource.SetResult(b);
            });
            return taskCompletionSource.Task;
        }

        public static Task<bool> AnimateBackgroundColor(this Row self, Color toColor, uint length = 250, Easing? easing = null)
        {
            easing ??= Easing.Linear;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            var anim = new Animation((x) =>
            {
                self.SetRowBackgroundColor(toColor, x);
            }, 0, 1, easing);

            anim.Commit(self, AnimationName, length: length, easing: easing, finished: (v, b) =>
            {
                taskCompletionSource.SetResult(b);
            });
            return taskCompletionSource.Task;
        }

        public static void CancelAnimation(this VisualElement self)
        {
            self.AbortAnimation(AnimationName);
        }

        public static Color MixColor(Color fromColor, Color toColor, double t)
        {
            return Microsoft.Maui.Graphics.Color.FromRgba(
                    fromColor.Red + t * (toColor.Red - fromColor.Red),
                    fromColor.Green + t * (toColor.Green - fromColor.Green),
                    fromColor.Blue + t * (toColor.Blue - fromColor.Blue),
                    fromColor.Alpha + t * (toColor.Alpha - fromColor.Alpha));
        }
    }
}
