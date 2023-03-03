using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal
{
    internal class Mask : Grid
    {
        //private readonly Frame _externalBorders = new();
        private readonly Rectangle _externalBorders = new();
        private readonly List<View> _internalBorders = new();

        internal Mask()
        {
            ColumnSpacing = 0;
            InputTransparent = true;
            VerticalOptions = LayoutOptions.Start;
            //BackgroundColor = Colors.Red;
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition{ Height = GridLength.Auto },
            };

            //_externalBorders.InputTransparent = true;
            //_externalBorders.HasShadow = false;
            //_externalBorders.CornerRadius = 0;
            //_externalBorders.Padding = 0;
            //_externalBorders.BackgroundColor = Colors.Transparent;
            //_externalBorders.BorderColor = Colors.Black;
            //_externalBorders.VerticalOptions = LayoutOptions.Start;
            _externalBorders.InputTransparent = true;
            _externalBorders.BackgroundColor = Colors.Transparent;
            _externalBorders.Stroke = new SolidColorBrush(Colors.Black);
            _externalBorders.StrokeThickness = 1;
            _externalBorders.VerticalOptions = LayoutOptions.Start;
        }

        internal void BorderColor(Color c)
        {
            foreach (var item in Children)
            {
                var v = (View)item;
                // external
                if (v is Rectangle box)
                    box.Stroke = new SolidColorBrush(c);
                // internal
                else if (v is BoxView line)
                    line.Color = c;
            }
        }

        internal void BorderWidth(double width)
        {
            foreach (var item in Children)
            {
                var v = (View)item;
                // external
                if (v is Rectangle f)
                    f.StrokeThickness = width;
                // internal
                else if (v is BoxView b)
                    b.WidthRequest = width;
            }
        }

        internal void Draw(ObservableCollection<DataGridColumn> columns, Color bordersColor, double borderWidth)
        {
            _internalBorders.Clear();
            Clear();

            var c = new ColumnDefinitionCollection();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                c.Add(new ColumnDefinition { Width = column.Width });
                TryCreateLine(i, columns.Count, bordersColor, borderWidth);
            }

            ColumnDefinitions = c;

            // external borders
            _externalBorders.Stroke = new SolidColorBrush(bordersColor);
            _externalBorders.StrokeThickness = borderWidth;
            this.SetColumnSpan(_externalBorders, ColumnDefinitions.Count);
            Add(_externalBorders);
        }

        internal void DrawColumn(DataGridColumn column, DrawType type, int id)
        {
            if (type == DrawType.Edit)
            {
                ColumnDefinitions[id] = new ColumnDefinition { Width = column.Width };
            }
            else if (type == DrawType.Delete)
            {
                ColumnDefinitions.RemoveAt(id);
                _internalBorders.RemoveAt(id);
            }
            else if (type == DrawType.Add)
            {
                ColumnDefinitions.Insert(id, new ColumnDefinition { Width = column.Width });

                // TODO пофиксить потом цвет и ширину
                TryCreateLine(id, ColumnDefinitions.Count, Colors.Black, 1);
            }

            this.SetColumnSpan(_externalBorders, ColumnDefinitions.Count);
        }

        internal void SetupHeight(double height)
        {
            if (Height == height)
                return;

            foreach (var item in Children)
            {
                var v = (View)item;
                v.HeightRequest = height;
            }
            HeightRequest = height;
            Debug.WriteLine($"Mask height: {height}");
        }

        private void TryCreateLine(int id, int of, Color borderColor, double borderWidth)
        {
            if (id + 1 == of)
                return;

            var line = new BoxView
            {
                WidthRequest = borderWidth,
                InputTransparent = true,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Fill,
                BackgroundColor = borderColor,
                Color = borderColor,
            };

            this.SetColumn(line, id);
            Add(line);
            _internalBorders.Insert(id, line);
        }
    }
}
