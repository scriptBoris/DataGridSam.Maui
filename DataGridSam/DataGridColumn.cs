using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam
{
    public class DataGridColumn : BindableObject
    {
        public DataGrid? DataGrid { get; private set; }
        public int Index { get; private set; } = -1;

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
            propertyChanged: (b, o, n) =>
            {
                if (b is DataGridColumn self)
                    self.DataGrid?.DrawColumn(self, DrawType.Edit, self.Index);
            }
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

        // todo Использовать кастомную реализацию триггеров (потом) (наверное?)
        // cell triggers
        //public static readonly BindableProperty CellTriggersProperty = BindableProperty.Create(
        //    nameof(CellTriggers),
        //    typeof(IList<TriggerBase>),
        //    typeof(DataGridColumn),
        //    defaultValueCreator: b =>
        //    {
        //        var list = new ObservableCollection<TriggerBase>();
        //        list.CollectionChanged += ((DataGridColumn)b).List_CollectionChanged;
        //        return list;
        //    }
        //);
        //private void List_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    foreach (var item in CellTriggers)
        //        if (item.TargetType != typeof(Cell))
        //        {
        //            Debug.WriteLine($"{nameof(DataGridSam.DataGrid)}: all RowTriggers must contain TargetType as Cell", "**CRITICAL ERROR**");
        //            throw new Exception($"{nameof(DataGridSam.DataGrid)}: all RowTriggers must contain TargetType as Cell");
        //        }

        //    DataGrid?.Draw();
        //}
        //public IList<TriggerBase> CellTriggers => (IList<TriggerBase>)GetValue(CellTriggersProperty);

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
