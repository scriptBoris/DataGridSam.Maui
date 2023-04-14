using DataGridSam.Handlers;
using DataGridSam.Internal;
using Microsoft.Maui.Layouts;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace DataGridSam
{
    [ContentProperty(nameof(Columns))]
    public class DataGrid : Layout, ILayoutManager, IHeaderCustomize
    {
        private readonly Header _header;
        private readonly DGCollection _collection;
        private readonly Mask _mask;
        private readonly RowTemplateGenerator _generator = new();

        private bool isInitialized;

        public DataGrid()
        {
            // header
            _header = new();
            Children.Add(_header);

            // collection
            _collection = new(this)
            {
                ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
                {
                    ItemSpacing = 0,
                }
            };
            Children.Add(_collection);
            _collection.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(ItemsSource), source: this));

            // mask
            _mask = new(this);
            Children.Add(_mask);

            _collection.VisibleHeightChanged += (o, e) => OnHeightChanged(); ;
            _header.SizeChanged += (o, e) => OnHeightChanged();
        }

        #region bindable props
        // columns
        public static readonly BindableProperty ColumnsProperty = BindableProperty.Create(
            nameof(Columns),
            typeof(ObservableCollection<DataGridColumn>),
            typeof(DataGrid),
            defaultValueCreator: b =>
            {
                if (b is not DataGrid self)
                    return null;

                var cols = new ObservableCollection<DataGridColumn>();
                cols.CollectionChanged += self.Cols_CollectionChanged;
                return cols;
            },
            propertyChanged: (b, o, n) =>
            {
                if (b is not DataGrid self)
                    return;

                if (o is ObservableCollection<DataGridColumn> old)
                    old.CollectionChanged -= self.Cols_CollectionChanged;

                if (n is ObservableCollection<DataGridColumn> newest)
                    newest.CollectionChanged += self.Cols_CollectionChanged;

                self.Draw();
            }
        );
        public ObservableCollection<DataGridColumn> Columns
        {
            get => (ObservableCollection<DataGridColumn>)GetValue(ColumnsProperty);
        }

        // items source
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IList),
            typeof(DataGrid),
            null
        );
        public IList? ItemsSource
        {
            get => GetValue(ItemsSourceProperty) as IList;
            set => SetValue(ItemsSourceProperty, value);
        }

        // borders color
        public static readonly BindableProperty BordersColorProperty = BindableProperty.Create(
            nameof(BordersColor),
            typeof(Color),
            typeof(DataGrid),
            Colors.Black,
            propertyChanged: (b, o, n) =>
            {
                if (b is not DataGrid self)
                    return;

                var c = (Color)n;
                self._header.BorderColor = c;
                self._mask.BorderColor(c);
                self._collection.BorderColor = c;
            }
        );
        public Color BordersColor
        {
            get => (Color)GetValue(BordersColorProperty);
            set => SetValue(BordersColorProperty, value);
        }

        // borders thickness
        public static readonly BindableProperty BordersThicknessProperty = BindableProperty.Create(
            nameof(BordersThickness),
            typeof(double),
            typeof(DataGrid),
            1.0,
            propertyChanged: (b, o, n) =>
            {
                if (b is not DataGrid self)
                    return;

                var v = (double)n;
                self._header.BorderWidth(v);
                self._mask.BorderWidth(v);
                self._collection.BorderThickness = v;
            }
        );
        public double BordersThickness
        {
            get => (double)GetValue(BordersThicknessProperty);
            set => SetValue(BordersThicknessProperty, value);
        }

        // row selected command
        public static readonly BindableProperty RowSelectedCommandProperty = BindableProperty.Create(
            nameof(RowSelectedCommand),
            typeof(ICommand),
            typeof(DataGrid),
            null,
            propertyChanged: Draw
        );
        public ICommand? RowSelectedCommand
        {
            get => GetValue(RowSelectedCommandProperty) as ICommand;
            set => SetValue(RowSelectedCommandProperty, value);
        }

        // row long selected command
        public static readonly BindableProperty RowLongSelectedCommandProperty = BindableProperty.Create(
            nameof(RowLongSelectedCommand),
            typeof(ICommand),
            typeof(DataGrid),
            null,
            propertyChanged: Draw
        );
        public ICommand? RowLongSelectedCommand
        {
            get => GetValue(RowLongSelectedCommandProperty) as ICommand;
            set => SetValue(RowLongSelectedCommandProperty, value);
        }

        // long tap timeout
        public static readonly BindableProperty LongTapTimeoutProperty = BindableProperty.Create(
            nameof(LongTapTimeout),
            typeof(int),
            typeof(DataGrid),
            500
        );
        /// <summary>
        /// Timeout to trigger RowLongSelectedCommand, value is specified in milliseconds
        /// default: 500
        /// </summary>
        public int LongTapTimeout
        {
            get => (int)GetValue(LongTapTimeoutProperty);
            set => SetValue(LongTapTimeoutProperty, value);
        }

        // tap selected color
        public static readonly BindableProperty TapSelectedColorProperty = BindableProperty.Create(
            nameof(TapSelectedColor),
            typeof(Color),
            typeof(DataGrid),
            Colors.Gray,
            //propertyChanged: Draw
            propertyChanged: (b,o,n) =>
            {
#if ANDROID
                if (b is DataGrid self) self.Draw();
#endif
            }
        );
        public Color TapSelectedColor
        {
            get => (Color)GetValue(TapSelectedColorProperty);
            set => SetValue(TapSelectedColorProperty, value);
        }

        // header font size
        public static readonly BindableProperty HeaderFontSizeProperty = BindableProperty.Create(
            nameof(HeaderFontSize),
            typeof(double),
            typeof(DataGrid),
            12.0,
            propertyChanged: (b, o, n) =>
            {
                if (b is not DataGrid self)
                    return;

                var v = (double)n;
                self._header.HeaderFontSize = v;
            }
        );
        public double HeaderFontSize
        {
            get => (double)GetValue(HeaderFontSizeProperty);
            set => SetValue(HeaderFontSizeProperty, value);
        }

        // header text color
        public static readonly BindableProperty HeaderTextColorProperty = BindableProperty.Create(
            nameof(HeaderTextColor),
            typeof(Color),
            typeof(DataGrid),
            Colors.Black,
            propertyChanged: (b, o, n) =>
            {
                if (b is DataGrid self)
                    self._header.HeaderTextColor = (Color)n;
            }
        );
        public Color HeaderTextColor
        {
            get => (Color)GetValue(HeaderTextColorProperty);
            set => SetValue(HeaderTextColorProperty, value);
        }

        // header background color
        public static readonly BindableProperty HeaderBackgroundColorProperty = BindableProperty.Create(
            nameof(HeaderBackgroundColor),
            typeof(Color),
            typeof(DataGrid),
            Colors.SkyBlue,
            propertyChanged: (b, o, n) =>
            {
                if (b is DataGrid self)
                    self._header.HeaderBackgroundColor = (Color)n;
            }
        );
        public Color HeaderBackgroundColor
        {
            get => (Color)GetValue(HeaderBackgroundColorProperty);
            set => SetValue(HeaderBackgroundColorProperty, value);
        }

        // header horizontal alignment
        public static readonly BindableProperty HeaderHorizontalAlignmentProperty = BindableProperty.Create(
            nameof(HeaderHorizontalAlignment),
            typeof(TextAlignment),
            typeof(DataGrid),
            TextAlignment.Center,
            propertyChanged: (b, o, n) =>
            {
                if (b is DataGrid self)
                    self._header.HeaderHorizontalAlignment = (TextAlignment)n;
            }
        );
        public TextAlignment HeaderHorizontalAlignment
        {
            get => (TextAlignment)GetValue(HeaderHorizontalAlignmentProperty);
            set => SetValue(HeaderHorizontalAlignmentProperty, value);
        }

        // header vertical alignment
        public static readonly BindableProperty HeaderVerticalAlignmentProperty = BindableProperty.Create(
            nameof(HeaderVerticalAlignment),
            typeof(TextAlignment),
            typeof(DataGrid),
            TextAlignment.Center,
            propertyChanged: (b, o, n) =>
            {
                if (b is DataGrid self)
                    self._header.HeaderVerticalAlignment = (TextAlignment)n;
            }
        );
        public TextAlignment HeaderVerticalAlignment
        {
            get => (TextAlignment)GetValue(HeaderVerticalAlignmentProperty);
            set => SetValue(HeaderVerticalAlignmentProperty, value);
        }

        // cell background color
        public static readonly BindableProperty CellBackgroundColorProperty = BindableProperty.Create(
            nameof(CellBackgroundColor),
            typeof(Color),
            typeof(DataGrid),
            Colors.White,
            propertyChanged: Draw
        );
        public Color CellBackgroundColor
        {
            get => (Color)GetValue(CellBackgroundColorProperty);
            set => SetValue(CellBackgroundColorProperty, value);
        }

        // cell text color
        public static readonly BindableProperty CellTextColorProperty = BindableProperty.Create(
            nameof(CellTextColor),
            typeof(Color),
            typeof(DataGrid),
            Colors.Black,
            propertyChanged: Draw
        );
        public Color CellTextColor
        {
            get => (Color)GetValue(CellTextColorProperty);
            set => SetValue(CellTextColorProperty, value);
        }

        // cell font size
        public static readonly BindableProperty CellFontSizeProperty = BindableProperty.Create(
            nameof(CellFontSize),
            typeof(double),
            typeof(DataGrid),
            14.0,
            propertyChanged: Draw
        );
        public double CellFontSize
        {
            get => (double)GetValue(CellFontSizeProperty);
            set => SetValue(CellFontSizeProperty, value);
        }

        // cell font attributes
        public static readonly BindableProperty CellFontAttributesProperty = BindableProperty.Create(
            nameof(CellFontAttributes),
            typeof(FontAttributes),
            typeof(DataGrid),
            FontAttributes.None,
            propertyChanged: Draw
        );
        public FontAttributes CellFontAttributes
        {
            get => (FontAttributes)GetValue(CellFontAttributesProperty);
            set => SetValue(CellFontAttributesProperty, value);
        }

        // cell horizontal text alignment
        public static readonly BindableProperty CellHorizontalTextAlignmentProperty = BindableProperty.Create(
            nameof(CellHorizontalTextAlignment),
            typeof(TextAlignment),
            typeof(DataGrid),
            TextAlignment.Start,
            propertyChanged: Draw
        );
        public TextAlignment CellHorizontalTextAlignment
        {
            get => (TextAlignment)GetValue(CellHorizontalTextAlignmentProperty);
            set => SetValue(CellHorizontalTextAlignmentProperty, value);
        }

        // cell vertical text alignment
        public static readonly BindableProperty CellVerticalTextAlignmentProperty = BindableProperty.Create(
            nameof(CellVerticalTextAlignment),
            typeof(TextAlignment),
            typeof(DataGrid),
            TextAlignment.Start,
            propertyChanged: Draw
        );
        public TextAlignment CellVerticalTextAlignment
        {
            get => (TextAlignment)GetValue(CellVerticalTextAlignmentProperty);
            set => SetValue(CellVerticalTextAlignmentProperty, value);
        }

        // cell padding
        public static readonly BindableProperty CellPaddingProperty = BindableProperty.Create(
            nameof(CellPadding),
            typeof(Thickness),
            typeof(DataGrid),
            new Thickness(0),
            propertyChanged: Draw
        );
        public Thickness CellPadding
        {
            get => (Thickness)GetValue(CellPaddingProperty);
            set => SetValue(CellPaddingProperty, value);
        }

        // row triggers
        public static readonly BindableProperty RowTriggersProperty = BindableProperty.Create(
            nameof(RowTriggers),
            typeof(IList<RowTrigger>),
            typeof(DataGrid),
            defaultValueCreator: (b) =>
            {
                var list = new ObservableCollection<RowTrigger>();
                list.CollectionChanged += ((DataGrid)b).RowTriggers_CollectionChanged;
                return list;
            }
        );
        private void RowTriggers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Draw();
        }
        public IList<RowTrigger> RowTriggers => (IList<RowTrigger>)GetValue(RowTriggersProperty);
#endregion bindable props

        internal double[] CachedWidths { get; set; } = Array.Empty<double>();
        internal IDispatcherTimer? Timer { get; set; }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            for (int i = 0; i < Columns.Count; i++)
            {
                var item = Columns[i];
                item.SetParent(this, i);
            }

            isInitialized = true;
            Draw();
        }

        public void ScrollTo(object item, object? group = null, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
        {
            _collection.ScrollTo(item, group, position, animate);
        }

        public void ScrollTo(int index, int groupIndex = -1, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
        {
            _collection.ScrollTo(index, groupIndex, position, animate);
        }

        internal void Draw()
        {
            if (!isInitialized)
                return;

            _header.Draw(Columns, BordersColor, BordersThickness);
            _mask.Draw(Columns, BordersColor, BordersThickness);
            _collection.ItemTemplate = _generator.Generate(this);
        }

        private void OnHeightChanged()
        {
            if (_header.Height < 0 || _collection.VisibleHeight < 0)
                return;

            double h = _collection.VisibleHeight + _header.Height;
            _mask.SetupHeight(h);
        }

        private void Cols_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!isInitialized)
                return;

            for (int i = 0; i < Columns.Count; i++)
                Columns[i].SetParent(this, i);

            _collection.Recalc(_collection.Width, true);
            Draw();
        }

        private static void Draw(BindableObject b, object o, object n)
        {
            if (b is DataGrid self)
                self.Draw();
        }

        protected override ILayoutManager CreateLayoutManager()
        {
            return this;
        }

        public Size ArrangeChildren(Rect bounds)
        {
            double x = 0;
            double y = 0;
            double w = bounds.Width;
            double h = bounds.Height;

            if (_mask.HasExternalBorders)
            {
                y = BordersThickness;
                x = BordersThickness;
                w -= BordersThickness * 2;
                h -= BordersThickness * 2;
            }

            // head
            double headHeight = heightHeadCache + y;
            ((IView)_header).Arrange(new Rect(x, y, w, heightHeadCache));

            // collection
            ((IView)_collection).Arrange(new Rect(x, headHeight, w, h - heightHeadCache));

            // mask
            ((IView)_mask).Arrange(new Rect(0, 0, bounds.Width, bounds.Height));

            return bounds.Size;
        }
        
        private double heightHeadCache;
        public Size Measure(double widthConstraint, double heightConstraint)
        {
            double h = double.IsInfinity(heightConstraint) ? 0 : heightConstraint;
            double w = double.IsInfinity(widthConstraint) ? 300 : widthConstraint;

            if (_mask.HasExternalBorders)
            {
                h -= BordersThickness * 2;
                w -= BordersThickness * 2;
            }

            // head
            var mhead = ((IView)_header).Measure(w, double.PositiveInfinity);

            // collection
            double freeSpaceH = h - mhead.Height;
            var mcollection = ((IView)_collection).Measure(w, freeSpaceH);

            // mask
            ((IView)_mask).Measure(widthConstraint, heightConstraint);

            if (double.IsInfinity(heightConstraint))
            {
                h += mhead.Height + mcollection.Height;

                // reset h
                if (_mask.HasExternalBorders)
                    h += BordersThickness * 2;
            }
            else
            {
                h = heightConstraint;
            }

            heightHeadCache = mhead.Height;


            return new Size(widthConstraint, h);
        }

        public async Task<Row?> GetRowAsync(int index)
        {
            Row? row = null;
            if (_collection.Handler is DGCollectionHandler h)
            {
#if ANDROID
                row = await h.GetRow(index);
#elif WINDOWS
                row = await h.GetRow(index);
#endif
            }
            return row;
        }
    }
}