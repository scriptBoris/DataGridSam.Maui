using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGridSam.Extensions;
using Microsoft.Maui.Controls;

namespace DataGridSam.Internal;

internal class RowTemplateGenerator
{
    private readonly DataGrid _dataGrid;
    private List<IDataTrigger> _triggers = new();

    internal RowTemplateGenerator(DataGrid dataGrid)
    {
        _dataGrid = dataGrid;
    }

    public DataTemplate? RowTemplate { get; set; }

    internal void Recalc()
    {
        int totalTriggerCount = _dataGrid.RowTriggers.Count;
        var columns = _dataGrid.Columns;

        // triggers
        _triggers = new List<IDataTrigger>();
        _triggers.AddRange(_dataGrid.RowTriggers);

        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            foreach (var t in col.CellTriggers)
                ((IDataTrigger)t).CellTriggerId = i;

            _triggers.AddRange(col.CellTriggers);
        }

        _triggers.InitTriggers(_dataGrid);

        RowTemplate ??= new DataTemplate
        {
            LoadTemplate = GenereateRow,
        };
    }

    internal object GenereateRow()
    {
        try
        {
            var row = new Row(_dataGrid, _triggers);
            return row;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DataGrid::RowGenerator: " + ex.ToString());
            return null;
        }
    }
}