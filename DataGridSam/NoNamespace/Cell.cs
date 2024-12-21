using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGridSam.Core;
using DataGridSam.Extensions;
using DataGridSam.Internal;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace DataGridSam;

public class Cell : IDataTriggerExecutor
{
    private readonly List<IDataTrigger> _enabledTriggers = new();
    private readonly DataGridColumn _column;
    private readonly Row _row;

    public Cell(DataGridColumn column, Row row)
    {
        _column = column;
        _row = row;

        if (column.CellTemplate == null)
        {
            View = new LabelCell
            {
                Padding = ResolvePadding(),
            };

            if (!string.IsNullOrEmpty(column.PropertyName))
                View.SetBinding(Label.TextProperty, new Binding(column.PropertyName, stringFormat: column.StringFormat));
        }
        else
        {
            View = (View)column.CellTemplate.CreateContent();

            switch (View)
            {
                case Button button:
                    button.Padding = ResolvePadding();
                    break;
                case ImageButton ibutton:
                    ibutton.Padding = ResolvePadding();
                    break;
                case Label cl:
                    cl.Padding = ResolvePadding();
                    break;
                case ContentView cv:
                    cv.Padding = ResolvePadding();
                    break;
                case Layout clt:
                    clt.Padding = ResolvePadding();
                    break;
                default:
                    break;
            }
        }

        UpdatePadding();
    }

    internal DataGridColumn Column => _column;
    internal View View { get; private set; }
    internal Color? BackgroundColor { get; private set; }
    internal Color? TriggeredBackgroundColor { get; private set; }
    internal Color? TextColor { get; private set; }
    internal double? FontSize { get; private set; }
    internal FontAttributes? FontAttributes { get; private set; }
    internal TextAlignment? VerticalTextAlignment { get; private set; }
    internal TextAlignment? HorizontalTextAlignment { get; private set; }

    internal void UpdateVisual()
    {
        TriggeredBackgroundColor = _enabledTriggers.FirstNonNull(x => x.BackgroundColor);
        TextColor = _enabledTriggers.FirstNonNull(x => x.TextColor);
        FontSize = _enabledTriggers.FirstNonNull(x => x.FontSize);
        FontAttributes = _enabledTriggers.FirstNonNull(x => x.FontAttributes);
        VerticalTextAlignment = _enabledTriggers.FirstNonNull(x => x.VerticalTextAlignment);
        HorizontalTextAlignment = _enabledTriggers.FirstNonNull(x => x.HorizontalTextAlignment);

        BackgroundColor = ResolvePropertyBg(
            TriggeredBackgroundColor,
            _row.TriggeredBackgroundColor,
            _column.CellBackgroundColor);

        if (View is ICell cell)
        {
            cell.TextColor = ResolveProperty<Color>(
                TextColor,
                _row.TextColor,
                _column.CellTextColor,
                _column.DataGrid!.CellTextColor
            );

            cell.FontSize = ResolveProperty<double>(
                FontSize,
                _row.FontSize,
                _column.CellFontSize,
                _column.DataGrid!.CellFontSize
            );

            cell.FontAttributes = ResolveProperty<FontAttributes>(
                FontAttributes,
                _row.FontAttributes,
                _column.CellFontAttributes,
                _column.DataGrid!.CellFontAttributes
            );

            cell.VerticalTextAlignment = ResolveProperty<TextAlignment>(
                VerticalTextAlignment,
                _row.VerticalTextAlignment,
                _column.CellVerticalTextAlignment,
                _column.DataGrid!.CellVerticalTextAlignment
            );

            cell.HorizontalTextAlignment = ResolveProperty<TextAlignment>(
                HorizontalTextAlignment,
                _row.HorizontalTextAlignment,
                _column.CellHorizontalTextAlignment,
                _column.DataGrid!.CellHorizontalTextAlignment
            );
        }
    }

    // todo Нужно сделать Nullable Thickness для паддингов, а для этого сделать конвентер из XAML в объект
    private Thickness ResolvePadding()
    {
        if (!_column.IsCellPaddingNull)
            return _column.CellPadding;

        return _column.DataGrid!.CellPadding;
    }

    internal void ClearEnabledTriggers()
    {
        _enabledTriggers.Clear();
    }

    internal void Rebind()
    {
        if (!string.IsNullOrEmpty(_column.PropertyName))
            View.SetBinding(Label.TextProperty, new Binding(_column.PropertyName, stringFormat: _column.StringFormat));
        else
            View.RemoveBinding(Label.TextProperty);
    }

    internal void UpdatePadding()
    {
        if (View is LabelCell cell)
            cell.Padding = ResolvePadding();
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

    public static Color? ResolvePropertyBg(Color? v1, Color? v2, Color? v3)
    {
        if (v1 != null)
            return v1;
        else if (v2 != null)
            return v2;
        else 
            return v3;
    }

    public static T ResolveProperty<T>(object? cellValue, object? rowValue, object? columnValue, object dataGridValue) where T : notnull
    {
        if (cellValue is T cellT)
            return cellT;
        else if (rowValue is T rowT)
            return rowT;
        else if (columnValue is T columnT)
            return columnT;
        else
            return (T)dataGridValue;
    }
}