using DataGridSam.Handlers;
using DataGridSam.Internal;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam
{
    public class Cell : ContentView
    {
        private readonly DataGridColumn column;
        private readonly Row row;

        public Cell(DataGridColumn column, Row row)
        {
            this.column = column;
            this.row = row;

            if (column.CellTemplate == null)
            {
                var label = new Label
                {
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

            BackgroundColor = null;

            foreach (var cellTrigger in column.CellTriggers)
                Triggers.Add(cellTrigger);
        }

        #region bindable props
        // text color
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(Cell),
            null,
            propertyChanged: Draw
        );
        public Color? TextColor
        {
            get => GetValue(TextColorProperty) as Color;
            set => SetValue(TextColorProperty, value);
        }

        // font size
        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
            nameof(FontSize),
            typeof(double?),
            typeof(Cell),
            null,
            propertyChanged: Draw
        );
        public double? FontSize
        {
            get => GetValue(FontSizeProperty) as double?;
            set => SetValue(FontSizeProperty, value);
        }

        // font attributes
        public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(
            nameof(FontAttributes),
            typeof(FontAttributes?),
            typeof(Cell),
            null,
            propertyChanged: Draw
        );
        public FontAttributes? FontAttributes
        {
            get => GetValue(FontAttributesProperty) as FontAttributes?;
            set => SetValue(FontAttributesProperty, value);
        }

        // vertical text alignment
        public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create(
            nameof(VerticalTextAlignment),
            typeof(TextAlignment?),
            typeof(Cell),
            null,
            propertyChanged: Draw
        );
        public TextAlignment? VerticalTextAlignment
        {
            get => GetValue(VerticalTextAlignmentProperty) as TextAlignment?;
            set => SetValue(VerticalTextAlignmentProperty, value);
        }

        // horizontal text alignment
        public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(
            nameof(HorizontalTextAlignment),
            typeof(TextAlignment?),
            typeof(Cell),
            null,
            propertyChanged: Draw
        );
        public TextAlignment? HorizontalTextAlignment
        {
            get => GetValue(HorizontalTextAlignmentProperty) as TextAlignment?;
            set => SetValue(HorizontalTextAlignmentProperty, value);
        }
        #endregion bindable props

        public static void Draw(BindableObject b, object o, object n)
        {
            if (b is Cell cell)
                cell.Draw();
        }

        public void Draw()
        {
            this.BackgroundColor = ResolveProperty<Color>(
                BackgroundColor,
                row.BackgroundColor,
                column.CellBackgroundColor,
                column.DataGrid!.CellBackgroundColor);

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
    }
}
