using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal;

internal class Mask : Layout, ILayoutManager
{
    private readonly DataGrid _dataGrid;
    private readonly Rectangle _externalBorders = new();
    private readonly List<IView> _internalBorders = new();

    internal Mask(DataGrid dataGrid)
    {
        InputTransparent = true;

        _dataGrid = dataGrid;
        _externalBorders.InputTransparent = true;
        //_externalBorders.Stroke = new SolidColorBrush(Colors.Black);
        //_externalBorders.StrokeThickness = 1;

        _externalBorders.Stroke = new SolidColorBrush(dataGrid.BordersColor);
        _externalBorders.StrokeThickness = dataGrid.BordersThickness;
        Children.Add(_externalBorders);
    }

    internal bool HasExternalBorders { get; set; } = true;


    internal Color? BorderColor
    {
        set
        {
            foreach (var item in Children)
            {
                var v = (View)item;
                // external
                if (v is Rectangle box)
                    box.Stroke = new SolidColorBrush(value);
                // internal
                else if (v is BoxView line)
                    line.Color = value;
            }
        }
    }

    internal double BorderWidth
    {
        set
        {
            _externalBorders.StrokeThickness = value;
            //foreach (var item in Children)
            //{
            //    var v = (View)item;
            //    // external
            //    if (v is Rectangle f)
            //        f.StrokeThickness = value;
            //    // internal
            //    else if (v is BoxView b)
            //        b.WidthRequest = value;
            //}
        }
    }

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
            var item = _internalBorders[i];
            double w = _dataGrid.BordersThickness;

            if (i == _internalBorders.Count - 1)
                w = 0;

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

    internal void Redraw(IList<DataGridColumn> columns)
    {
        _internalBorders.Clear();
        Children.Clear();

        for (int i = 0; i < columns.Count; i++)
        {
            var view = TryCreateInternalLine();
            Children.Add(view);
            _internalBorders.Insert(i, view);
        }

        Children.Add(_externalBorders);

        BorderColor = _dataGrid.BordersColor;
        BorderWidth = _dataGrid.BordersThickness;

        // external borders
        //if (_externalBorders != null)
        //{
        //    _externalBorders.Stroke = new SolidColorBrush(bordersColor);
        //    _externalBorders.StrokeThickness = borderWidth;
        //    Children.Add(_externalBorders);
        //}
    }

    internal void DrawByInsert(DataGridColumn column)
    {
        var view = TryCreateInternalLine();
        Children.Add(view);
        _internalBorders.Add(view);
        InvalidateMeasure();
    }

    internal void DrawByRemove(int repId)
    {
        IView view = _internalBorders.Last();
        _internalBorders.Remove(view);
        Children.Remove(view);
        InvalidateMeasure();
    }

    private View TryCreateInternalLine()
    {
        var line = new BoxView
        {
            InputTransparent = true,
        };

        return line;
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
