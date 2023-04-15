using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal
{
    internal class Mask : Layout, ILayoutManager
    {
        private readonly DataGrid _dataGrid;
        private readonly Rectangle? _externalBorders = new();
        private readonly List<View> _internalBorders = new();

        internal Mask(DataGrid dataGrid)
        {
            InputTransparent = true;

            _dataGrid = dataGrid;
            _externalBorders.InputTransparent = true;
            _externalBorders.Stroke = new SolidColorBrush(Colors.Black);
            _externalBorders.StrokeThickness = 1;
        }

        internal bool HasExternalBorders { get; set; } = true;

        protected override ILayoutManager CreateLayoutManager()
        {
            return this;
        }

        public Size ArrangeChildren(Rect bounds)
        {
            if (_externalBorders is IView v)
                v.Arrange(bounds);

            double x = 0;
            for (int i = 0; i < _internalBorders.Count; i++)
            {
                var item = (IView)_internalBorders[i];
                double w = _dataGrid.BordersThickness;
                x += _dataGrid.CachedWidths[i] + w;

                var rect = new Rect(x, 0, w, bounds.Height);
                item.Arrange(rect);
            }

            return bounds.Size;
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            if (_externalBorders is IView v)
                v.Measure(widthConstraint, heightConstraint);

            foreach (IView item in _internalBorders)
                item.Measure(widthConstraint, heightConstraint);

            return new Size(widthConstraint, heightConstraint);
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
            Children.Clear();

            for (int i = 0; i < columns.Count; i++)
            {
                TryCreateInternalLine(i, columns.Count, bordersColor, borderWidth);
            }

            // external borders
            if (_externalBorders != null)
            {
                _externalBorders.Stroke = new SolidColorBrush(bordersColor);
                _externalBorders.StrokeThickness = borderWidth;
                Children.Add(_externalBorders);
            }
        }

        private void TryCreateInternalLine(int id, int of, Color borderColor, double borderWidth)
        {
            if (id + 1 == of)
                return;

            var line = new BoxView
            {
                InputTransparent = true,
                Color = borderColor,
            };

            Children.Add(line);
            _internalBorders.Insert(id, line);
        }
    }
}
