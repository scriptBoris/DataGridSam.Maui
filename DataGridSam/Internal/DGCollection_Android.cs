﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace DataGridSam.Internal;

internal class DGCollection_Android : CollectionView, IDGCollection
{
    private readonly DataGrid _dataGrid;
    private readonly RowTemplateGenerator _generator;
    private readonly List<Row> _visibleRows = new();
    private Color _borderColor = Colors.Black;
    private double _borderThickness = 1;
    private double _viewPortHeight;
    private LinearItemsLayout _layout;

    public event EventHandler<double>? VisibleHeightChanged;

    public DGCollection_Android(DataGrid dataGrid)
    {
        _dataGrid = dataGrid;
        _generator = new RowTemplateGenerator(dataGrid);
        BackgroundColor = dataGrid.BordersColor;

        ItemsLayout = _layout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
        {
            ItemSpacing = _dataGrid.BordersThickness,
        };
    }

    public double ViewPortHeight
    {
        get => _viewPortHeight;
        set
        {
            _viewPortHeight = value;
            VisibleHeightChanged?.Invoke(this, value);
        }
    }

    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            BackgroundColor = value;
        }
    }

    public double BorderThickness
    {
        get => _borderThickness;
        set
        {
            _borderThickness = value;
            _layout.ItemSpacing = value;
        }
    }

    public void Redraw()
    {
        BorderColor = _dataGrid.BordersColor;
        BorderThickness = _dataGrid.BordersThickness;
        _generator.Recalc();
        ItemTemplate = _generator.RowTemplate;
    }

    public void UpdateCellsVisual(bool needRecalcMeasure)
    {
        foreach (var row in _visibleRows)
            row.UpdateVisual(needRecalcMeasure);
    }

    public void UpdateCellsPadding(int? cellId)
    {
        foreach (var row in _visibleRows)
            row.UpdateCellPadding(cellId);
    }

    public void UpdateCellsMeasure()
    {
        foreach (var row in _visibleRows)
            row.ThrowInvalidateMeasure();
    }

    public void RestructColumns()
    {
        foreach (var row in _visibleRows)
            row.Refab();
    }

    public void RebindColumn(int columnId)
    {
        foreach (var row in _visibleRows)
            row.Rebind(columnId);
    }

    public async Task<Row?> GetRowFast(int indexOfItemsSource)
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