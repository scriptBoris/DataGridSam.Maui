using DataGridSam.Extensions;
using DataGridSam.Internal;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam;

public class Row : Layout, ILayoutManager, IDataTriggerExecutor
{
    private readonly DataGrid _dataGrid;
    private readonly Cell[] _cells;
    private readonly List<IDataTrigger> _enabledTriggers = new();
    private readonly IList<IDataTrigger> _triggers;
    private readonly RowBackgroundView _backgroundView = new();

    private INotifyPropertyChanged? lastContext;
    private bool isPressed;
    private Color externalBackgroundColor = Colors.Transparent;
    private float externalBackgroundColorFill = 0;

    public Row(DataGrid dataGrid, IList<IDataTrigger> triggers)
    {
        _dataGrid = dataGrid;
        _cells = new Cell[dataGrid.Columns.Count];
        _triggers = triggers;

        BackgroundColor = dataGrid.CellBackgroundColor;
        Children.Add(_backgroundView);

        for (int i = 0; i < dataGrid.Columns.Count; i++)
        {
            var col = dataGrid.Columns[i];
            var cell = new Cell(col, this);
            
            _cells[i] = cell;
            Children.Add(cell.View);
        }
    }

    public new Color BackgroundColor { get; private set; }
    public Color? TriggeredBackgroundColor { get; private set; }
    public Color? TextColor { get; set; }
    public double? FontSize { get; set; }
    public FontAttributes? FontAttributes { get; set; }
    public TextAlignment? VerticalTextAlignment { get; set; }
    public TextAlignment? HorizontalTextAlignment { get; set; }
    public Color TapColor => _dataGrid.TapSelectedColor;

    private double[] Widths => _dataGrid.CachedWidths;
    private float[] WidthsSkia => _dataGrid.CachedWidthsForSkia;
    private IDispatcherTimer? Timer { get; set; }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (lastContext != null)
            lastContext.PropertyChanged -= OnBindingContextPropertyChanged;

        if (BindingContext is INotifyPropertyChanged notify)
        {
            notify.PropertyChanged += OnBindingContextPropertyChanged;
            lastContext = notify;
        }
        else
        {
            lastContext = null;
        }

        UpdateTriggers();
        UpdateVisual();
    }

    private void OnBindingContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_triggers.Count == 0)
            return;

        bool hasChanges = false;
        foreach (var trigger in _triggers)
        {
            if (e.PropertyName != trigger.PropertyName)
                continue;

            if (UpdateTrigger(trigger))
                hasChanges = true;
        }

        if (hasChanges)
            UpdateVisual();
    }

    private void UpdateTriggers()
    {
        if (_triggers.Count == 0)
            return;

        foreach (var trigger in _triggers)
            UpdateTrigger(trigger);
    }

    private bool UpdateTrigger(IDataTrigger trigger)
    {
        object? value = BindingContext.GetValueFromProperty(trigger.PropertyName);
        IDataTriggerExecutor executor;
        if (trigger.CellTriggerId != null)
            executor = _cells[trigger.CellTriggerId.Value];
        else
            executor = this;

        return executor.ExecuteTrigger(trigger, value);
    }

    public bool ExecuteTrigger(IDataTrigger trigger, object? value)
    {
        bool isEnabled = trigger.IsEqualValue(value);
        bool hasChanges;

        if (isEnabled)
        {
            hasChanges = !_enabledTriggers.Contains(trigger);
            if (hasChanges) _enabledTriggers.Add(trigger);
        }
        else
        {
            hasChanges = _enabledTriggers.Remove(trigger);
        }

        return hasChanges;
    }

    internal void UpdateVisual()
    {
        TriggeredBackgroundColor = _enabledTriggers.FirstNonNull(x => x.BackgroundColor);
        TextColor = _enabledTriggers.FirstNonNull(x => x.TextColor);
        FontSize = _enabledTriggers.FirstNonNull(x => x.FontSize);
        FontAttributes = _enabledTriggers.FirstNonNull(x => x.FontAttributes);
        VerticalTextAlignment = _enabledTriggers.FirstNonNull(x => x.VerticalTextAlignment);
        HorizontalTextAlignment = _enabledTriggers.FirstNonNull(x => x.HorizontalTextAlignment);
        BackgroundColor = TriggeredBackgroundColor ?? _dataGrid.CellBackgroundColor;

        foreach (var cell in _cells)
            cell.UpdateVisual();

        Redraw();
    }

    internal void Redraw()
    {
        var colors = new Color?[_cells.Length];
        for (int i = 0; i < _cells.Length; i++)
            colors[i] = _cells[i].BackgroundColor?.MultiplyAlpha(1 - externalBackgroundColorFill);

        float spacing = (float)_dataGrid.BordersThickness;
        var mainColor = BackgroundColor.MixColor(externalBackgroundColor, externalBackgroundColorFill);
        _backgroundView.Redraw(spacing, WidthsSkia, mainColor, colors);
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }

    public Size ArrangeChildren(Rect bounds)
    {
        double x = 0;

        ((IView)_backgroundView).Arrange(bounds);
        Redraw();

        for (int i = 0; i < _cells.Length; i++)
        {
            var cell = _cells[i];
            double w = Widths[i];
            double h = bounds.Size.Height;

            var rect = new Rect(x, 0, w, h);
            ((IView)cell.View).Arrange(rect);

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

        ((IView)_backgroundView).Measure(widthConstraint, heightConstraint);

        double h = 0;
        for (int i = 0; i < _cells.Length; i++)
        {
            var cell = _cells[i];
            double w = Widths[i];

            var m = ((IView)cell.View).Measure(w, heightConstraint);

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
            OnTapFinish(forceLongTap ? TapFinishModes.LongTap : TapFinishModes.Tap);
        else
            OnTapFinish(TapFinishModes.Cancel);
    }

    public void OnTapStart()
    {
        isPressed = true;
        if (Timer == null)
        {
            Timer = this.Dispatcher.CreateTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(_dataGrid.LongTapTimeout);
            Timer.IsRepeating = false;
            Timer.Tick += OnLongTapTimerCompleted;
            Timer.Start();
        }
        else
        {
            Timer.Stop();
            Timer.Start();
        }
    }

    public void OnTapFinish(TapFinishModes mode)
    {
        if (!isPressed)
            return;

        isPressed = false;
        Timer?.Stop();

        var finalMode = mode;
        if (mode == TapFinishModes.LongTap && _dataGrid.RowLongSelectedCommand == null)
            finalMode = TapFinishModes.Tap;

        switch (finalMode)
        {
            case TapFinishModes.Tap:
                bool can = _dataGrid.RowSelectedCommand?.CanExecute(BindingContext) ?? false;
                if (can)
                    _dataGrid.RowSelectedCommand?.Execute(BindingContext);
                break;

            case TapFinishModes.LongTap:
                bool canLong = _dataGrid.RowLongSelectedCommand?.CanExecute(BindingContext) ?? false;
                if (canLong)
                    _dataGrid.RowLongSelectedCommand?.Execute(BindingContext);
                break;

            case TapFinishModes.Cancel:
            default:
                break;
        }
    }

    private void OnLongTapTimerCompleted(object? sender, EventArgs e)
    {
        OnTapFinish(TapFinishModes.LongTap);
    }

    /// <summary>
    /// Sets the background of the row and cells to the specified
    /// </summary>
    /// <param name="color">New color</param>
    /// <param name="fill">Percent setup, from 0 to 1, where 1 is full setup color</param>
    public void SetRowBackgroundColor(Color color, float fill = 1)
    {
        externalBackgroundColor = color;
        externalBackgroundColorFill = fill;
        Redraw();
    }

    /// <summary>
    /// Restores the visual part of a string to the original DataGrid state
    /// </summary>
    /// <param name="fill">Percent resore, from 0 to 1, where 1 is full restore</param>
    public void RestoreRowBackgroundColor(float fill = 1)
    {
        externalBackgroundColorFill = 1 - fill;
        Redraw();
    }
}

public enum TapFinishModes
{
    Cancel,
    Tap,
    LongTap,
}

public static class VisualExtensions
{
    public const string AnimationName = "ColorTo";

    public static Task<bool> AnimateBackgroundColorRestore(this Row self, uint length = 250, Easing? easing = null)
    {
        easing ??= Easing.Linear;
        var taskCompletionSource = new TaskCompletionSource<bool>();

        var anim = new Microsoft.Maui.Controls.Animation((x) =>
        {
            self.RestoreRowBackgroundColor((float)x);
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

        var anim = new Microsoft.Maui.Controls.Animation((x) =>
        {
            self.SetRowBackgroundColor(toColor, (float)x);
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

    public static Color MixColor(this Color fromColor, Color toColor, double t)
    {
        return Microsoft.Maui.Graphics.Color.FromRgba(
                fromColor.Red + t * (toColor.Red - fromColor.Red),
                fromColor.Green + t * (toColor.Green - fromColor.Green),
                fromColor.Blue + t * (toColor.Blue - fromColor.Blue),
                fromColor.Alpha + t * (toColor.Alpha - fromColor.Alpha));
    }

    //public static Color MixColor(this Color fromColor, Color toColor, float progress)
    //{
    //    float r = fromColor.Red + (toColor.Red - fromColor.Red) * progress;
    //    float g = fromColor.Green + (toColor.Green - fromColor.Green) * progress;
    //    float b = fromColor.Blue + (toColor.Blue - fromColor.Blue) * progress;
    //    float a = fromColor.Alpha + (toColor.Alpha - fromColor.Alpha) * progress;

    //    return new Color(r, g, b, a);
    //}
}
