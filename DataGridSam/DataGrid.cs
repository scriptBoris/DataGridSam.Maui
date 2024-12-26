using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DataGridSam.Internal;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Platform;

namespace DataGridSam;

[ContentProperty(nameof(Columns))]
public class DataGrid : Layout, ILayoutManager, IHeaderCustomize
{
    private readonly Header _header;
    private readonly IDGCollection _collection;
    private readonly Mask _mask;

    private double cachedWidth;
    private bool isInitialized;

    public event EventHandler<ItemsViewScrolledEventArgs> Scrolled
    {
        add
        {
            _collection.Scrolled += value;
        }
        remove
        {
            _collection.Scrolled -= value;
        }
    }
    
    public DataGrid()
    {
        // fill and expand
        VerticalOptions = new LayoutOptions(LayoutAlignment.Fill, true);

        // header
        _header = new(this);
        Children.Add(_header);

        // collection
#if WINDOWS
        _collection = new DGCollection_Windows(this);
#elif ANDROID
        _collection = new DGCollection_Android(this);
#elif IOS
        _collection = new DGCollection_iOS(this);
#else
        throw new NotSupportedException();
#endif
        Children.Add(_collection);

        // mask
        _mask = new(this);
        Children.Add(_mask);
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

            self.TryRedraw("setter Columns");
        }
    );
    public ObservableCollection<DataGridColumn> Columns
    {
        get => (ObservableCollection<DataGridColumn>)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    // items source
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IList),
        typeof(DataGrid),
        null,
        propertyChanged:(b,o,n) =>
        {
            if (b is DataGrid self)
                self._collection.ItemsSource = n as IList;
        }
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

            var color = (Color)n;
            self._header.BorderColor = color;
            self._mask.BorderColor = color;
            self._collection.BorderColor = color;
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

            double width = (double)n;
            self._mask.BorderWidth = width;

            if (!self.isInitialized)
                return;

            self._collection.BorderThickness = width;
            self._header.BorderWidth = width;
            self.UpdateCellsWidthCache(null, true);
            self.TryUpdateCellsVisual(true);
            //((IView)self).InvalidateMeasure();
            self.InvalidateMeasure();
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
        null
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
        null
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
        propertyChanged: (b, o, n) =>
        {
            if (b is DataGrid self)
                self.TryUpdateRowsTapSelectColor();
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
        propertyChanged: (b, o, n) =>
        {
            if (b is DataGrid self)
                self.TryUpdateCellsVisual(false);
        }
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
        propertyChanged: (b, o, n) =>
        {
            if (b is DataGrid self)
                self.TryUpdateCellsVisual(false);
        }
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
        propertyChanged: (b, o, n) =>
        {
            if (b is DataGrid self)
                self.TryUpdateCellsVisual(true);
        }
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
        propertyChanged: (b, o, n) =>
        {
            if (b is DataGrid self)
                self.TryUpdateCellsVisual(true);
        }
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
        propertyChanged: (b, o, n) =>
        {
            if (b is DataGrid self)
                self.TryUpdateCellsVisual(false);
        }
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
        propertyChanged: (b, o, n) =>
        {
            if (b is DataGrid self)
                self.TryUpdateCellsVisual(false);
        }
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
        propertyChanged: (b, o, n) =>
        {
            if (b is DataGrid self)
                self.TryUpdateCellsPadding(null);
        }
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
        TryRedraw("row triggers Obs collection changed");
    }
    public IList<RowTrigger> RowTriggers => (IList<RowTrigger>)GetValue(RowTriggersProperty);
    #endregion bindable props

    internal double[] CachedWidths { get; private set; } = Array.Empty<double>();
    internal double[] CachedWidthsForMask { get; private set; } = Array.Empty<double>();
    internal float[] CachedWidthsForSkia { get; private set; } = Array.Empty<float>();
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
        TryRedraw("Init");
    }

    internal void TryUpdateCellsVisual(bool needRecalcMeasure)
    {
        if (isInitialized)
            _collection.UpdateCellsVisual(needRecalcMeasure);
    }

    internal void TryUpdateCellsPadding(int? columnId)
    {
        if (isInitialized)
            _collection.UpdateCellsPadding(columnId);
    }

    internal void TryUpdateRowsTapSelectColor()
    {
#if ANDROID
        if (isInitialized && _collection.Handler is IDGCollectionHandler handler)
        {
            handler.UpdateNativeTapColor(TapSelectedColor);
        }
#endif
    }

    internal void TryUpdateCellsMeasure()
    {
        if (isInitialized)
            _collection.UpdateCellsMeasure();
    }

    internal bool TryRedraw(string reason)
    {
        if (!isInitialized)
            return false;

        _header.Redraw(Columns);
        _mask.Redraw(Columns);
        _collection.Redraw();

        Debug.WriteLine($"DataGrid :: DRAW [REASON:{reason}]");
        return true;
    }

    private void Cols_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!isInitialized)
            return;

        UpdateCellsWidthCache(null, true);

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var add = (DataGridColumn)e.NewItems![0]!;
                add.SetParent(this, e.NewStartingIndex);

                _header.DrawByInsert(add);
                _mask.Redraw();
                _collection.RestructColumns();
                break;

            case NotifyCollectionChangedAction.Replace:
                var rep = (DataGridColumn)e.NewItems![0]!;
                int repId = e.NewStartingIndex;
                rep.SetParent(this, repId);

                _header.DrawByRemove(repId);
                _header.DrawByInsert(rep);

                _mask.Redraw();
                _collection.RestructColumns();
                break;

            case NotifyCollectionChangedAction.Remove:
                int removeIndex = e.OldStartingIndex;
                _header.DrawByRemove(removeIndex);
                _mask.Redraw();
                _collection.RestructColumns();
                break;

            case NotifyCollectionChangedAction.Move:
                throw new NotImplementedException();

            case NotifyCollectionChangedAction.Reset:
                TryRedraw("columns is cleared");
                break;

            default:
                break;
        }
    }

    internal void OnColumnWidthChanged(DataGridColumn column)
    {
        UpdateCellsWidthCache(null, true);
        _collection.UpdateCellsMeasure();
        _header.ThrowInvalidateMeasure();
        _mask.ThrowInvalidateMeasure();
    }

    internal void OnColumnRebind(DataGridColumn column)
    {
        _collection.RebindColumn(column.Index);
    }

    internal void UpdateCellsWidthCache(double? availableWidth, bool isForce)
    {
        double width = availableWidth ?? _collection.Width;

        if (cachedWidth != width || isForce)
        {
            int vlines = Columns.Count - 1;
            if (vlines < 0)
                vlines = 0;

            double freeWidth = width - vlines * BordersThickness;
            double maskFreeWidth = _mask.HasExternalBorders ? width - BordersThickness * 2 : width;
            var lengths = Columns.Select(x => x.Width).ToArray();

            CachedWidths = Row.CalculateWidthRules(lengths, freeWidth);
            CachedWidthsForMask = Row.CalculateWidthRules(lengths, maskFreeWidth);
            CachedWidthsForSkia = CachedWidths
                .Select(x => (float)(x * DeviceDisplay.Current.MainDisplayInfo.Density))
                .ToArray();
            cachedWidth = width;
        }
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
        double heightHeadCache = _header.DesiredSize.Height;
        ((IView)_header).Arrange(new Rect(x, y, w, heightHeadCache));

        y += heightHeadCache;

        // collection
        double collectionHeight = _collection.DesiredSize.Height;
        ((IView)_collection).Arrange(new Rect(x, y, w, collectionHeight));

        // mask
        double maskHeight = heightHeadCache + collectionHeight;
        if (_mask.HasExternalBorders)
        {
            maskHeight += BordersThickness * 2;
        }
        ((IView)_mask).Arrange(new Rect(0, 0, bounds.Width, maskHeight));

        return bounds.Size;
    }

    //public Size Measure(double widthConstraint, double heightConstraint)
    //{
    //    double h = double.IsInfinity(heightConstraint) ? 200 : heightConstraint;
    //    double w = double.IsInfinity(widthConstraint) ? 300 : widthConstraint;

    //    if (_mask.HasExternalBorders)
    //    {
    //        h -= BordersThickness * 2;
    //        w -= BordersThickness * 2;
    //    }

    //    UpdateCellsWidthCache(w, false);

    //    // head
    //    var mhead = ((IView)_header).Measure(w, double.PositiveInfinity);

    //    // collection
    //    double freeSpaceH = h - mhead.Height;
    //    var mcollection = ((IView)_collection).Measure(w, freeSpaceH);

    //    // mask
    //    ((IView)_mask).Measure(widthConstraint, heightConstraint);

    //    if (double.IsInfinity(heightConstraint))
    //    {
    //        h += mhead.Height + mcollection.Height;

    //        // reset h
    //        if (_mask.HasExternalBorders)
    //            h += BordersThickness * 2;
    //    }
    //    else
    //    {
    //        h = heightConstraint;
    //    }

    //    return new Size(widthConstraint, h);
    //}

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        double sizeWidth = double.IsInfinity(widthConstraint) ? 300 : widthConstraint;
        double sizeHeight = double.IsInfinity(heightConstraint) ? 200 : heightConstraint;
        double freeWidth = sizeWidth;
        double freeHeight = sizeHeight;

        if (_mask.HasExternalBorders)
        {
            freeWidth -= BordersThickness * 2;
            freeHeight -= BordersThickness * 2;
        }
        
        // very important
        UpdateCellsWidthCache(sizeWidth, false);

        // head
        var mhead = ((IView)_header).Measure(freeWidth, double.PositiveInfinity);
        freeHeight -= mhead.Height;

        // collection
        if (double.IsInfinity(heightConstraint))
        {
            var mcollection = ((IView)_collection).Measure(freeWidth, double.PositiveInfinity);
            sizeHeight = mhead.Height + mcollection.Height;

            if (_mask.HasExternalBorders)
                sizeHeight += BordersThickness * 2;
        }
        else
        {
            ((IView)_collection).Measure(freeWidth, freeHeight);
        }

        // mask
        ((IView)_mask).Measure(sizeWidth, sizeHeight);

        return new Size(sizeWidth, sizeHeight);
    }

    public void ScrollTo(object item, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
    {
        _collection.ScrollTo(item, null, position, animate);
    }

    public void ScrollTo(int index, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
    {
        _collection.ScrollTo(index, -1, position, animate);
    }

    public async Task<Row?> GetRowAsync(int index, TimeSpan? timeout)
    {
        if (ItemsSource == null)
            return null;

        var fastRow = await _collection.GetRowFast(index);
        if (fastRow != null)
            return fastRow;

        if (_collection.Handler is IDGCollectionHandler h)
            return await h.GetRowAsync(index, timeout);

        return null;
    }
}