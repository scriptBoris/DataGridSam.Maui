using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace DataGridSam.Internal;

public class DGCollection : CollectionView
{
    private readonly DataGrid _dataGrid;
    private readonly RowTemplateGenerator _generator = new();
    private readonly List<Row> _visibleRows = new();
    private Color _borderColor = Colors.Black;
    private double _borderThickness = 1;

    public event EventHandler<double>? VisibleHeightChanged;

    public DGCollection(DataGrid dataGrid)
    {
        this._dataGrid = dataGrid;
    }

    public double VisibleHeight { get; private set; }

    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            if (Handler is IDGCollectionHandler hand)
                hand.UpdateBorderColor();
        }
    }

    public double BorderThickness
    {
        get => _borderThickness;
        set
        {
            _borderThickness = value;
            if (Handler is IDGCollectionHandler hand)
                hand.UpdateBorderWidth();
        }
    }

    public void OnVisibleHeight(double height)
    {
        VisibleHeight = height;
        VisibleHeightChanged?.Invoke(this, height);
    }

    internal void Redraw()
    {
        BorderColor = _dataGrid.BordersColor;
        BorderThickness = _dataGrid.BordersThickness;
        ItemTemplate = _generator.Generate(_dataGrid);
    }

    internal void UpdateCellsVisual(bool needRecalcMeasure)
    {
        foreach (var row in _visibleRows)
            row.UpdateVisual(needRecalcMeasure);
    }

    internal void UpdateCellsPadding(int? cellId)
    {
        foreach (var row in _visibleRows)
            row.UpdateCellPadding(cellId);
    }

    internal void UpdateCellsMeasure()
    {
        foreach (var row in _visibleRows)
            row.ThrowInvalidateMeasure();
    }

    internal void RestructColumns()
    {
        foreach (var row in _visibleRows)
            row.Refab();
    }

    internal void RebindColumn(int columnId)
    {
        foreach (var row in _visibleRows)
            row.Rebind(columnId);
    }

    internal async Task<Row?> GetRowFast(int indexOfItemsSource)
    {
        foreach (var item in _visibleRows)
        {
            if (_dataGrid.ItemsSource!.IndexOf(item.BindingContext) == indexOfItemsSource)
                return item;
        }

        await Task.Delay(15);

        foreach (var item in _visibleRows)
        {
            if (_dataGrid.ItemsSource!.IndexOf(item.BindingContext) == indexOfItemsSource)
                return item;
        }

        return null;
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        _dataGrid.UpdateCellsWidthCache(widthConstraint, false);
        var res = base.MeasureOverride(widthConstraint, heightConstraint);
        return res;
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);

        var row = (Row)child;
        if (row.IsRemoved)
        {
            row.IsRemoved = false;
            row.Refab();
        }

        _visibleRows.Add(row);
    }

    protected override void OnChildRemoved(Element child, int oldLogicalIndex)
    {
        base.OnChildRemoved(child, oldLogicalIndex);
        var row = (Row)child;
        row.IsRemoved = true;
        _visibleRows.Remove(row);
    }
}
