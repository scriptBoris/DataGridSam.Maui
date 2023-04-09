using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal
{
    internal class Header : Grid
    {
        private readonly List<Label> _views = new();
        private readonly BoxView _underline;
        private Color _borderColor;
        private double _headerFontSize;
        private Color _headerTextColor;
        private Color _headerBackgroundColor;
        private TextAlignment _headerHorizontalAlignment;
        private TextAlignment _headerVerticalAlignment;

        public Header()
        {
            RowDefinitions = new()
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            };
            RowSpacing = 0;
            ColumnSpacing = 0;

            _underline = new BoxView
            {
                VerticalOptions = LayoutOptions.Start,
                HeightRequest = 1,
            };

            HeaderFontSize = (double)DataGrid.HeaderFontSizeProperty.DefaultValue;
            BorderColor = (Color)DataGrid.BordersColorProperty.DefaultValue;
            HeaderTextColor = (Color)DataGrid.HeaderTextColorProperty.DefaultValue;
            HeaderBackgroundColor = (Color)DataGrid.HeaderBackgroundColorProperty.DefaultValue;
            HeaderHorizontalAlignment = (TextAlignment)DataGrid.HeaderHorizontalAlignmentProperty.DefaultValue;
            HeaderVerticalAlignment = (TextAlignment)DataGrid.HeaderVerticalAlignmentProperty.DefaultValue;
        }

        internal TextAlignment HeaderHorizontalAlignment
        {
            get => _headerHorizontalAlignment;
            set
            {
                _headerHorizontalAlignment = value;
                foreach (var item in _views)
                    item.HorizontalTextAlignment = value;
            }
        }

        internal TextAlignment HeaderVerticalAlignment
        {
            get => _headerVerticalAlignment;
            set
            {
                _headerVerticalAlignment = value;
                foreach (var item in _views)
                    item.VerticalTextAlignment = value;
            }
        }

        internal Color HeaderBackgroundColor
        {
            get => _headerBackgroundColor;
            set
            {
                _headerBackgroundColor = value;
                foreach (var item in _views)
                    item.BackgroundColor = value;
            }
        }

        internal Color HeaderTextColor
        {
            get => _headerTextColor;
            set
            {
                _headerTextColor = value;
                foreach (var item in _views)
                    item.TextColor = value;
            }
        }

        internal double HeaderFontSize
        {
            get => _headerFontSize;
            set
            {
                _headerFontSize = value;
                foreach (var item in _views)
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

        internal void BorderWidth(double width)
        {
            _underline.HeightRequest = width;
        }

        internal void Draw(IList<DataGridColumn> columns, Color borderColor, double borderWidth)
        {
            _views.Clear();
            Clear();

            if (columns.Count == 0)
            {
                return;
            }

            var cDefs = new ColumnDefinitionCollection();

            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                var colDef = new ColumnDefinition { Width = col.Width };
                cDefs.Add(colDef);

                CreateTitle(col, i);
            }
            ColumnDefinitions = cDefs;

            _underline.Color = borderColor;
            _underline.HeightRequest = borderWidth;
            this.SetRow(_underline, 1);
            this.SetColumnSpan(_underline, ColumnDefinitions.Count);
            this.Add(_underline);
        }

        private void CreateTitle(DataGridColumn col, int index)
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding(nameof(DataGridColumn.Title), source: col));
            label.FontSize = HeaderFontSize;
            label.TextColor = HeaderTextColor;
            label.BackgroundColor = HeaderBackgroundColor;
            label.VerticalTextAlignment = HeaderVerticalAlignment;
            label.HorizontalTextAlignment = HeaderHorizontalAlignment;

            this.SetColumn(label, index);
            Children.Add(label);

            _views.Insert(index, label);

            for (int i = index + 1; i < _views.Count; i++)
            {
                var view = _views[i];
                this.SetColumn(view, i);
            }
        }

        private void RemoveTitle(int index)
        {
            var v = _views[index];
            _views.RemoveAt(index);
            ColumnDefinitions.RemoveAt(index);
            Remove(v);
        }
    }
}
