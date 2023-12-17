using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal
{
    internal class Header : Layout, ILayoutManager
    {
        private readonly DataGrid _dataGrid;
        private readonly List<Label> _labels = new();
        private readonly BoxView _underline;

        private double _headerFontSize = (double)DataGrid.HeaderFontSizeProperty.DefaultValue;
        private Color _borderColor = (Color)DataGrid.BordersColorProperty.DefaultValue;
        private double _borderWidth = (double)DataGrid.BordersThicknessProperty.DefaultValue;
        private Color _headerTextColor = (Color)DataGrid.HeaderTextColorProperty.DefaultValue;
        private Color _headerBackgroundColor = (Color)DataGrid.HeaderBackgroundColorProperty.DefaultValue;
        private TextAlignment _headerHorizontalAlignment = (TextAlignment)DataGrid.HeaderHorizontalAlignmentProperty.DefaultValue;
        private TextAlignment _headerVerticalAlignment = (TextAlignment)DataGrid.HeaderVerticalAlignmentProperty.DefaultValue;

        public Header(DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
            _underline = new BoxView();
            Children.Add(_underline);
        }

        #region props
        internal TextAlignment HeaderHorizontalAlignment
        {
            get => _headerHorizontalAlignment;
            set
            {
                _headerHorizontalAlignment = value;
                foreach (var item in _labels)
                    item.HorizontalTextAlignment = value;
            }
        }

        internal TextAlignment HeaderVerticalAlignment
        {
            get => _headerVerticalAlignment;
            set
            {
                _headerVerticalAlignment = value;
                foreach (var item in _labels)
                    item.VerticalTextAlignment = value;
            }
        }

        internal Color HeaderBackgroundColor
        {
            get => _headerBackgroundColor;
            set
            {
                _headerBackgroundColor = value;
                foreach (var item in _labels)
                    item.BackgroundColor = value;
            }
        }

        internal Color HeaderTextColor
        {
            get => _headerTextColor;
            set
            {
                _headerTextColor = value;
                foreach (var item in _labels)
                    item.TextColor = value;
            }
        }

        internal double HeaderFontSize
        {
            get => _headerFontSize;
            set
            {
                _headerFontSize = value;
                foreach (var item in _labels)
                    item.FontSize = value;
            }
        }

        internal Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                _underline.Color = value;
            }
        }

        internal double BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                _underline.HeightRequest = value;
                InvalidateMeasure();
            }
        }
        #endregion props

        private double[] Widths => _dataGrid.CachedWidths;

        protected override ILayoutManager CreateLayoutManager()
        {
            return this;
        }

        public Size ArrangeChildren(Rect bounds)
        {
            double x = 0;
            double sp = _underline.DesiredSize.Height;
            double cellsHeight = bounds.Size.Height - sp;

            for (int i = 0; i < _labels.Count; i++)
            {
                var cell = _labels[i];
                double w = Widths[i];

                var rect = new Rect(x, 0, w, cellsHeight);
                ((IView)cell).Arrange(rect);

                x += w + _dataGrid.BordersThickness;
            }

            ((IView)_underline).Arrange(
                new Rect(
                    0,                       //x
                    bounds.Size.Height - sp, //y
                    bounds.Size.Width,       //w
                    sp                       //h
                )
            );

            return bounds.Size;
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            double h = 0;
            for (int i = 0; i < _labels.Count; i++)
            {
                var cell = _labels[i];
                double w = Widths[i];

                var m = ((IView)cell).Measure(w, heightConstraint);
                if (m.Height > h)
                    h = m.Height;
            }

            h += ((IView)_underline).Measure(widthConstraint, _dataGrid.BordersThickness).Height;

            return new Size(widthConstraint, h);
        }

        internal void Redraw(IList<DataGridColumn> columns)
        {
            _labels.Clear();
            Children.Clear();

            if (columns.Count == 0)
                return;

            for (int i = 0; i < columns.Count; i++)
            {
                var title = CreateTitle(columns[i]);
                Children.Add(title);
                _labels.Insert(i, (Label)title);
            }

            Children.Add(_underline);
            _underline.Color = _dataGrid.BordersColor;
            _underline.HeightRequest = _dataGrid.BordersThickness;
            InvalidateMeasure();
        }

        internal void DrawByInsert(DataGridColumn col)
        {
            var view = CreateTitle(col);
            Children.Add(view);
            _labels.Insert(col.Index, (Label)view);
            InvalidateMeasure();
        }

        internal void DrawByRemove(int index)
        {
            var view = _labels[index];
            _labels.RemoveAt(index);
            Children.Remove(view);
            InvalidateMeasure();
        }

        private View CreateTitle(DataGridColumn col)
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding(nameof(DataGridColumn.Title), source: col));
            label.FontSize = HeaderFontSize;
            label.TextColor = HeaderTextColor;
            label.BackgroundColor = HeaderBackgroundColor;
            label.VerticalTextAlignment = HeaderVerticalAlignment;
            label.HorizontalTextAlignment = HeaderHorizontalAlignment;
            return label;
        }

        internal void ThrowInvalidateArrange()
        {
            ((IView)this).InvalidateArrange();
        }

        internal void ThrowInvalidateMeasure()
        {
            InvalidateMeasure();
        }
    }
}
