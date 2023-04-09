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
    public class Cell : ContentView, IDataTriggerHost
    {
        private readonly List<IDataTrigger> enabledTriggers = new();
        private readonly DataGridColumn column;
        private readonly Row row;

        public Cell(DataGridColumn column, Row row)
        {
            this.Margin = 0;
            this.Padding = 0;
            this.column = column;
            this.row = row;

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
                var custom = column.CellTemplate.CreateContent() as View;
                custom?.SetBinding(View.BindingContextProperty, new Binding(column.PropertyName, stringFormat: column.StringFormat));
                Content = custom;
            }
        }

        #region bindable props
        // background color
        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
            nameof(BackgroundColor),
            typeof(Color),
            typeof(Cell),
            null,
            propertyChanged:(b,o,n) =>
            {
                if (b is Cell cell)
                    cell.Draw();
            }
        );
        public new Color? BackgroundColor
        {
            get => GetValue(BackgroundColorProperty) as Color;
            set => SetValue(BackgroundColorProperty, value);
        }
        #endregion bindable props

        public Color? TextColor { get; set; }
        public double? FontSize { get; set; }
        public FontAttributes? FontAttributes { get; set; }
        public TextAlignment? VerticalTextAlignment { get; set; }
        public TextAlignment? HorizontalTextAlignment { get; set; }

        public void Draw()
        {
            var bg = ResolveProperty<Color>(
                BackgroundColor,
                row.BackgroundColor,
                column.CellBackgroundColor,
                column.DataGrid!.CellBackgroundColor);

            if (bg == row.BackgroundColor)
                bg = Colors.Transparent;

            base.BackgroundColor = bg;

            if (Content is Label label)
            {
                label.TextColor = ResolveProperty<Color>(
                    TextColor,
                    row.TextColor,
                    column.CellTextColor,
                    column.DataGrid!.CellTextColor
                );

                label.FontSize = ResolveProperty<double>(
                    FontSize,
                    row.FontSize,
                    column.CellFontSize,
                    column.DataGrid!.CellFontSize
                );

                label.FontAttributes = ResolveProperty<FontAttributes>(
                    FontAttributes,
                    row.FontAttributes,
                    column.CellFontAttributes,
                    column.DataGrid!.CellFontAttributes
                );

                label.VerticalTextAlignment = ResolveProperty<TextAlignment>(
                    VerticalTextAlignment,
                    row.VerticalTextAlignment,
                    column.CellVerticalTextAlignment,
                    column.DataGrid!.CellVerticalTextAlignment
                );

                label.HorizontalTextAlignment = ResolveProperty<TextAlignment>(
                    HorizontalTextAlignment,
                    row.HorizontalTextAlignment,
                    column.CellHorizontalTextAlignment,
                    column.DataGrid!.CellHorizontalTextAlignment
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
