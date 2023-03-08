using DataGridSam.Handlers;
using DataGridSam.Internal;
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
    public class DataGrid : Grid, IHeaderCustomize
    {
        private readonly RowTemplateGenerator _generator = new();
        private readonly Header _header = new();
        private readonly DGCollection _collection = new();
        private readonly Mask _mask = new();

        private bool isInitialized;

        public DataGrid()
        {
            RowDefinitions = new()
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
            };
            RowSpacing = 0;

            // header
            Add(_header);

            // collection
            this.SetRow(_collection, 1);
            Add(_collection);
            _collection.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(ItemsSource), source: this));

            // mask
            this.SetRowSpan(_mask, 2);
            Add(_mask);

            _collection.VisibleHeightChanged += (o, e) => Update(); ;
            _header.SizeChanged += (o, e) => Update();
        }

        private void Update()
        {
            if (_header.Height < 0 || _collection.VisibleHeight < 0)
                return;

            double h = _collection.VisibleHeight + _header.Height;
            _mask.SetupHeight(h);
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

        internal void Draw()
        {
            if (!isInitialized)
                return;

            _header.Draw(Columns, BordersColor, BordersThickness);
            _mask.Draw(Columns, BordersColor, BordersThickness);
            _collection.ItemTemplate = _generator.Generate(this);
        }

        internal void DrawColumn(DataGridColumn column, DrawType type, int id)
        {
            _header.DrawColumn(column, type, id);
            _mask.DrawColumn(column, type, id);
            _collection.ItemTemplate = _generator.Generate(this);
        }

        private void Cols_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!isInitialized)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var addItem = (DataGridColumn)e.NewItems![0]!;
                    addItem.SetParent(this, e.NewStartingIndex);
                    DrawColumn(addItem, DrawType.Add, e.NewStartingIndex);
                    return;

                case NotifyCollectionChangedAction.Remove:
                    var removeItem = (DataGridColumn)e.OldItems![0]!;
                    removeItem.SetParent(null, null);
                    DrawColumn(removeItem, DrawType.Delete, e.OldStartingIndex);
                    return;

                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in Columns)
                        item.SetParent(null, null);
                    break;

                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    Draw();
                    break;
            }
        }

        private static void Draw(BindableObject b, object o, object n)
        {
            if (b is DataGrid self)
                self.Draw();
        }
    }

    internal enum DrawType
    {
        Edit,
        Add,
        Delete,
    }
}