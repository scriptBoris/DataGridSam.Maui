using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal;

internal class DGCollection_iOS : CollectionView, IDGCollection
{
    private readonly DataGrid _dataGrid;

    public DGCollection_iOS(DataGrid dataGrid)
    {
        _dataGrid = dataGrid;
    }

    public double ViewPortHeight => throw new NotImplementedException();

    public Color BorderColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public double BorderThickness { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Task<Row?> GetRowFast(int index)
    {
        throw new NotImplementedException();
    }

    public void RebindColumn(int index)
    {
        throw new NotImplementedException();
    }

    public void Redraw()
    {
        throw new NotImplementedException();
    }

    public void RestructColumns()
    {
        throw new NotImplementedException();
    }

    public void UpdateCellsMeasure()
    {
        throw new NotImplementedException();
    }

    public void UpdateCellsPadding(int? columnId)
    {
        throw new NotImplementedException();
    }

    public void UpdateCellsVisual(bool needRecalcMeasure)
    {
        throw new NotImplementedException();
    }
}