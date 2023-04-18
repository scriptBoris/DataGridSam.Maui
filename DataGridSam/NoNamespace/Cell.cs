using DataGridSam.Core;
using DataGridSam.Extensions;
using DataGridSam.Handlers;
using DataGridSam.Internal;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Content = new LabelCell
            {
                Padding = ResolvePadding(),
            };
            Content.SetBinding(Label.TextProperty, new Binding(column.PropertyName, stringFormat: column.StringFormat));
        }
        else
        {
            Content = (View)column.CellTemplate.CreateContent();

            if (string.IsNullOrEmpty(column.PropertyName))
                Content.SetBinding(View.BindingContextProperty, new Binding("BindingContext", source:row));
            else
                Content.SetBinding(View.BindingContextProperty, new Binding(column.PropertyName, stringFormat: column.StringFormat));

            switch (Content)
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

        LogicalBackgroundColor = Content.BackgroundColor ?? Colors.Transparent;
    }

    internal Color LogicalBackgroundColor { get; private set; }
    public View Content { get; private set; }
    public Color? BackgroundColor { get; set; }
    public Color? TextColor { get; set; }
    public double? FontSize { get; set; }
    public FontAttributes? FontAttributes { get; set; }
    public TextAlignment? VerticalTextAlignment { get; set; }
    public TextAlignment? HorizontalTextAlignment { get; set; }

    public void UpdateVisual()
    {
        BackgroundColor = _enabledTriggers.FirstNonNull(x => x.BackgroundColor);
        TextColor = _enabledTriggers.FirstNonNull(x => x.TextColor);
        FontSize = _enabledTriggers.FirstNonNull(x => x.FontSize);
        FontAttributes = _enabledTriggers.FirstNonNull(x => x.FontAttributes);
        VerticalTextAlignment = _enabledTriggers.FirstNonNull(x => x.VerticalTextAlignment);
        HorizontalTextAlignment = _enabledTriggers.FirstNonNull(x => x.HorizontalTextAlignment);

        var bg = ResolveProperty<Color>(
            BackgroundColor,
            _row.TriggeredBackgroundColor,
            _column.CellBackgroundColor,
            _column.DataGrid!.CellBackgroundColor) ?? Colors.Transparent;

        Content.BackgroundColor = bg;
        LogicalBackgroundColor = bg;

        if (Content is ICell cell)
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

    // todo что за порнография? Нужно сделать Nullable Thickness для паддингов, а для этого сделать конвентер из XAML в объект
    private Thickness ResolvePadding()
    {
        if (!_column.IsCellPaddingNull)
            return _column.CellPadding;

        return _column.DataGrid!.CellPadding;
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

    public static T ResolveProperty<T>(object? cellValue, object? rowValue, object? columnValue, object dataGridValue)
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
