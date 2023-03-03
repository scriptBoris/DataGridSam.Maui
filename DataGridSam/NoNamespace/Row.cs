using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam
{
    public class Row : Grid
    {
        private readonly DataGridColumn[] cols;
        private readonly Cell[] cells;

        public Row(int columns)
        {
            ColumnSpacing = 0;
            cells = new Cell[columns];
            cols = new DataGridColumn[columns];
        }

        internal bool IsDrawed { get; set; }

        #region bindable props
        // text color
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(Row),
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
            typeof(Row),
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
            typeof(Row),
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
            typeof(Row),
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
            typeof(Row),
            null,
            propertyChanged: Draw
        );
        public TextAlignment? HorizontalTextAlignment
        {
            get => GetValue(HorizontalTextAlignmentProperty) as TextAlignment?;
            set => SetValue(HorizontalTextAlignmentProperty, value);
        }
        #endregion bindable props

        public void InitCell(DataGridColumn col)
        {
            int colId = col.Index;

            var cell = new Cell(col, this);
            cells[colId] = cell;
            cols[colId] = col;

            this.SetColumn(cell, colId);
            this.Add(cell);
        }

        private static void Draw(BindableObject b, object o, object n)
        {
            if (b is Row self)
                self.Draw();
        }

        public void Draw()
        {
            foreach (var cell in cells)
                cell.Draw();

            IsDrawed = true;
        }
    }
}
