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

namespace DataGridSam
{
    public class Cell : IDataTriggerHost
    {
        private readonly List<IDataTrigger> _enabledTriggers = new();
        private readonly DataGridColumn _column;
        private readonly Row _row;

        public Cell(DataGridColumn column, Row row)
        {
            this._column = column;
            this._row = row;

            if (column.CellTemplate == null)
            {
                var label = new Label
                {
                    VerticalOptions = LayoutOptions.Fill,
                    HorizontalOptions = LayoutOptions.Fill,
                };
                Content = label;
                label.SetBinding(Label.TextProperty, new Binding(column.PropertyName, stringFormat: column.StringFormat));
            }
            else
            {
                var custom = (View)column.CellTemplate.CreateContent();
                custom.SetBinding(View.BindingContextProperty, new Binding(column.PropertyName, stringFormat: column.StringFormat));
                Content = custom;
            }
        }
        
        public View Content { get; private set; }
        public Color? BackgroundColor { get; set; }
        public Color? TextColor { get; set; }
        public double? FontSize { get; set; }
        public FontAttributes? FontAttributes { get; set; }
        public TextAlignment? VerticalTextAlignment { get; set; }
        public TextAlignment? HorizontalTextAlignment { get; set; }

        public void Draw()
        {
            var bg = ResolveProperty<Color>(
                BackgroundColor,
                _row.BackgroundColor,
                _column.CellBackgroundColor,
                _column.DataGrid!.CellBackgroundColor);

            if (bg == _row.BackgroundColor)
                bg = Colors.Transparent;

            Content.BackgroundColor = bg;

            if (Content is Label label)
            {
                label.TextColor = ResolveProperty<Color>(
                    TextColor,
                    _row.TextColor,
                    _column.CellTextColor,
                    _column.DataGrid!.CellTextColor
                );

                label.FontSize = ResolveProperty<double>(
                    FontSize,
                    _row.FontSize,
                    _column.CellFontSize,
                    _column.DataGrid!.CellFontSize
                );

                label.FontAttributes = ResolveProperty<FontAttributes>(
                    FontAttributes,
                    _row.FontAttributes,
                    _column.CellFontAttributes,
                    _column.DataGrid!.CellFontAttributes
                );

                label.VerticalTextAlignment = ResolveProperty<TextAlignment>(
                    VerticalTextAlignment,
                    _row.VerticalTextAlignment,
                    _column.CellVerticalTextAlignment,
                    _column.DataGrid!.CellVerticalTextAlignment
                );

                label.HorizontalTextAlignment = ResolveProperty<TextAlignment>(
                    HorizontalTextAlignment,
                    _row.HorizontalTextAlignment,
                    _column.CellHorizontalTextAlignment,
                    _column.DataGrid!.CellHorizontalTextAlignment
                );
            }
        }

        public T ResolveProperty<T>(object? cellValue, object? rowValue, object? columnValue, object dataGridValue)
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
    }
}
