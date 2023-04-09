using Microsoft.Maui.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam
{
    public class DataGridColumn : BindableObject
    {
        public DataGrid? DataGrid { get; private set; }
        public int Index { get; private set; } = -1;
        public bool IsCellPaddingNull { get; private set; } = true;

        #region bindable props
        // property name
        public static readonly BindableProperty PropertyNameProperty = BindableProperty.Create(
            nameof(PropertyName),
            typeof(string),
            typeof(DataGridColumn),
            null,
            propertyChanged: Draw
        );
        public string? PropertyName
        {
            get => GetValue(PropertyNameProperty) as string;
            set => SetValue(PropertyNameProperty, value);
        }

        // title
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(DataGridColumn),
            null,
            propertyChanged: Draw
        );
        public string? Title
        {
            get => GetValue(TitleProperty) as string;
            set => SetValue(TitleProperty, value);
        }

        // string format
        public static readonly BindableProperty StringFormatProperty = BindableProperty.Create(
            nameof(StringFormat),
            typeof(string),
            typeof(DataGridColumn),
            null,
            propertyChanged: Draw
        );
        public string? StringFormat
        {
            get => GetValue(StringFormatProperty) as string;
            set => SetValue(StringFormatProperty, value);
        }

        // width
        public static readonly BindableProperty WidthProperty = BindableProperty.Create(
            nameof(Width),
            typeof(GridLength),
            typeof(DataGridColumn),
            GridLength.Star,
            propertyChanged: Draw
        );
        [TypeConverter(typeof(GridLengthTypeConverter))]
        public GridLength Width
        {
            get => (GridLength)GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        // cell triggers
        public static readonly BindableProperty CellTriggersProperty = BindableProperty.Create(
            nameof(CellTriggers),
            typeof(IList<CellTrigger>),
            typeof(DataGridColumn),
            defaultValueCreator: b =>
            {
                var list = new ObservableCollection<CellTrigger>();
                list.CollectionChanged += ((DataGridColumn)b).List_CollectionChanged;
                return list;
            }
        );
        private void List_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DataGrid?.Draw();
        }
        public IList<CellTrigger> CellTriggers => (IList<CellTrigger>)GetValue(CellTriggersProperty);

        // cell background color
        public static readonly BindableProperty CellBackgroundColorProperty = BindableProperty.Create(
            nameof(CellBackgroundColor),
            typeof(Color),
            typeof(DataGridColumn),
            null,
            propertyChanged: Draw
        );
        public Color? CellBackgroundColor
        {
            get => GetValue(CellBackgroundColorProperty) as Color;
            set => SetValue(CellBackgroundColorProperty, value);
        }

        // cell template
        public static readonly BindableProperty CellTemplateProperty = BindableProperty.Create(
            nameof(CellTemplate),
            typeof(DataTemplate),
            typeof(DataGridColumn),
            null,
            propertyChanged: Draw
        );
        public DataTemplate? CellTemplate
        {
            get => GetValue(CellTemplateProperty) as DataTemplate;
            set => SetValue(CellTemplateProperty, value);
        }

        // cell text color
        public static readonly BindableProperty CellTextColorProperty = BindableProperty.Create(
            nameof(CellTextColor),
            typeof(Color),
            typeof(DataGridColumn),
            null,
            propertyChanged: Draw
        );
        public Color? CellTextColor
        {
            get => GetValue(CellTextColorProperty) as Color;
            set => SetValue(CellTextColorProperty, value);
        }

        // cell font size
        public static readonly BindableProperty CellFontSizeProperty = BindableProperty.Create(
            nameof(CellFontSize),
            typeof(double?),
            typeof(DataGridColumn),
            null,
            propertyChanged: Draw
        );
        public double? CellFontSize
        {
            get => GetValue(CellFontSizeProperty) as double?;
            set => SetValue(CellFontSizeProperty, value);
        }

        // cell font attributes
        public static readonly BindableProperty CellFontAttributesProperty = BindableProperty.Create(
            nameof(CellFontAttributes),
            typeof(FontAttributes?),
            typeof(DataGridColumn),
            null,
            propertyChanged: Draw
        );
        public FontAttributes? CellFontAttributes
        {
            get => GetValue(CellFontAttributesProperty) as FontAttributes?;
            set => SetValue(CellFontAttributesProperty, value);
        }

        // cell horizontal text alignment
        public static readonly BindableProperty CellHorizontalTextAlignmentProperty = BindableProperty.Create(
            nameof(CellHorizontalTextAlignment),
            typeof(TextAlignment?),
            typeof(DataGridColumn),
            null,
            propertyChanged: Draw
        );
        public TextAlignment? CellHorizontalTextAlignment
        {
            get => GetValue(CellHorizontalTextAlignmentProperty) as TextAlignment?;
            set => SetValue(CellHorizontalTextAlignmentProperty, value);
        }

        // cell vertical text alignment
        public static readonly BindableProperty CellVerticalTextAlignmentProperty = BindableProperty.Create(
            nameof(CellVerticalTextAlignment),
            typeof(TextAlignment?),
            typeof(DataGridColumn),
            null,
            propertyChanged: Draw
        );
        public TextAlignment? CellVerticalTextAlignment
        {
            get => GetValue(CellVerticalTextAlignmentProperty) as TextAlignment?;
            set => SetValue(CellVerticalTextAlignmentProperty, value);
        }

        // cell padding
        public static readonly BindableProperty CellPaddingProperty = BindableProperty.Create(
            nameof(CellPadding),
            typeof(Thickness),
            typeof(DataGridColumn),
            new Thickness(-322, 0, 0, 0),
            propertyChanged: (b,o,n) =>
            {
                if (b is DataGridColumn self)
                    self.IsCellPaddingNull = ((Thickness)n).Left == -322;

                Draw(b,o,n);
            }
        );
        public Thickness CellPadding
        {
            get => (Thickness)GetValue(CellPaddingProperty);
            set => SetValue(CellPaddingProperty, value);
        }
        #endregion bindable props

        internal void SetParent(DataGrid? dataGrid, int? columnId)
        {
            DataGrid = dataGrid;
            Index = columnId ?? -1;
        }

        internal static void Draw(BindableObject b, object o, object n)
        {
            if (b is DataGridColumn self)
                self.DataGrid?.Draw();
        }
    }
}
