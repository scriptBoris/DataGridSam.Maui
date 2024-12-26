using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGridSam.Extensions;
using DataGridSam.Internal;
using Microsoft.Maui;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace DataGridSam;

public class Row : Layout, ILayoutManager, IDataTriggerExecutor
{
    private readonly DataGrid _dataGrid;
    private readonly List<Cell> _cells = new();
    private readonly List<IDataTrigger> _enabledTriggers = new();
    private readonly IList<IDataTrigger> _triggers;
    private readonly RowBackgroundView _backgroundView = new();

    private INotifyPropertyChanged? lastContext;
    private bool isPressed;
    private Color externalBackgroundColor = Colors.Transparent;
    private float externalBackgroundColorFill = 0;
    private double _rowHeight = -1;

    public Row(DataGrid dataGrid, IList<IDataTrigger> triggers)
    {
        _dataGrid = dataGrid;
        _triggers = triggers;

        BackgroundColor = dataGrid.CellBackgroundColor;
        Children.Add(_backgroundView);

        for (int i = 0; i < dataGrid.Columns.Count; i++)
        {
            var col = dataGrid.Columns[i];
            var cell = new Cell(col, this);

            _cells.Add(cell);
            Children.Add(cell.View);
        }
    }

    public new Color BackgroundColor { get; private set; }
    public Color? TriggeredBackgroundColor { get; private set; }
    public Color? TextColor { get; private set; }
    public double? FontSize { get; private set; }
    public FontAttributes? FontAttributes { get; private set; }
    public TextAlignment? VerticalTextAlignment { get; private set; }
    public TextAlignment? HorizontalTextAlignment { get; private set; }
    internal bool IsRemoved { get; set; }
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

    internal void UpdateVisual(bool needRecalcMeasure = false)
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

        RedrawBackground();

        if (needRecalcMeasure)
            InvalidateMeasure();
    }

    internal void RedrawBackground()
    {
        var colors = new Color?[_cells.Count];
        for (int i = 0; i < _cells.Count; i++)
            colors[i] = _cells[i].BackgroundColor?.MultiplyAlpha(1 - externalBackgroundColorFill);

        float spacing = (float)_dataGrid.BordersThickness;
        var mainColor = BackgroundColor.MixColor(externalBackgroundColor, externalBackgroundColorFill);
        _backgroundView.Redraw(spacing, WidthsSkia, mainColor, colors);
    }

    internal void Refab()
    {
        int max = Math.Max(_dataGrid.Columns.Count, _cells.Count);

        for (int i = 0; i < max; i++)
        {
            var cell = (i < _cells.Count) ? _cells[i] : null;
            var gridCol = (i < _dataGrid.Columns.Count) ? _dataGrid.Columns[i] : null;
            var cellCol = cell?.Column;

            // No changes (or both is null? WTF??)
            if (cellCol == gridCol)
            {
                continue;
            }
            // Removed last
            else if (gridCol == null && cell != null)
            {
                _cells.Remove(cell);
                Children.Remove(cell.View);
            }
            // Added
            else if (cellCol == null && gridCol != null)
            {
                var newCell = new Cell(gridCol, this);
                newCell.UpdateVisual();
                _cells.Add(newCell);
                Children.Add(newCell.View);
            }
            // replace
            else if (gridCol != null)
            {
                var newCell = new Cell(gridCol, this);
                newCell.UpdateVisual();
                _cells[i] = newCell;
                Children[i + 1] = newCell.View;
            }
        }

        //RedrawBackground();
        //InvalidateMeasure();
        UpdateVisual(true);
    }

    internal void Rebind(int columnId)
    {
        _cells[columnId].Rebind();
    }

    internal void UpdateCellPadding(int? cellId)
    {
        if (cellId != null)
        {
            _cells[cellId.Value].UpdatePadding();
        }
        else
        {
            foreach (var cell in _cells)
                cell.UpdatePadding();
        }
    }

    internal void ThrowInvalidateMeasure()
    {
        InvalidateMeasure();
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }

    public Size ArrangeChildren(Rect bounds)
    {
        double x = 0;

        double rowHeight = _rowHeight;
        var rowRect = new Rect(0,0, bounds.Width, rowHeight);
        ((IView)_backgroundView).Arrange(rowRect);
        //RedrawBackground();

        for (int i = 0; i < _cells.Count; i++)
        {
            var cell = _cells[i];
            double w = Widths[i];
            double h = rowHeight;

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
        if (double.IsInfinity(widthConstraint))
        {
            widthConstraint = Widths.Sum();
        }

        ((IView)_backgroundView).Measure(widthConstraint, heightConstraint);

        double h = 0;
        for (int i = 0; i < _cells.Count; i++)
        {
            var cell = _cells[i];
            double w = Widths[i];

            var m = ((IView)cell.View).Measure(w, heightConstraint);

            if (m.Height > h)
                h = m.Height;
        }

        _rowHeight = h;

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
        RedrawBackground();
    }

    /// <summary>
    /// Restores the visual part of a string to the original DataGrid state
    /// </summary>
    /// <param name="fill">Percent resore, from 0 to 1, where 1 is full restore</param>
    public void RestoreRowBackgroundColor(float fill = 1)
    {
        externalBackgroundColorFill = 1 - fill;
        RedrawBackground();
    }

    public void UpdateBorderColor()
    {
        base.BackgroundColor = _dataGrid.BordersColor;
    }

    public void UpdateBorderThickness()
    {
        if (this is IView view)
            view.InvalidateMeasure();
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
}