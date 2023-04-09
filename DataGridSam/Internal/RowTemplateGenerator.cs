﻿using DataGridSam.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal
{
    internal class RowTemplateGenerator
    {
        internal DataTemplate Generate(DataGrid dataGrid)
        {
            int totalTriggerCount = dataGrid.RowTriggers.Count;
            var columns = dataGrid.Columns;

            // triggers
            var triggers = new List<IDataTrigger>();
            triggers.AddRange(dataGrid.RowTriggers);

            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                foreach(var t in col.CellTriggers)
                    ((IDataTrigger)t).CellTriggerId = i;

                triggers.AddRange(col.CellTriggers);
            }

            triggers.InitTriggers(dataGrid);

            var template = new DataTemplate();
            template.LoadTemplate = () =>
            {
                try
                {
                    var row = new Row(dataGrid);

                    // TODO Сделать обработчик нажатия на сам Row
                    //Button? button = null;
                    //if (dataGrid.RowSelectedCommand != null || dataGrid.RowLongSelectedCommand != null)
                    //{
                    //    button = new DGButton()
                    //    {
                    //        CornerRadius = 0,
                    //        Padding = 0,
                    //        Margin = 0,
                    //        BackgroundColor = Colors.Transparent,
                    //        Command = dataGrid.RowSelectedCommand,
                    //        CommandLongClick = dataGrid.RowLongSelectedCommand,
                    //    };
                    //    button.SetBinding(Button.CommandParameterProperty, Binding.SelfPath);
                    //    row.Children.Add(button);
                    //}

                    //var colDef = new ColumnDefinitionCollection();
                    //for (int i = 0; i < columns.Count; i++)
                    //{
                    //    var column = columns[i];
                    //    colDef.Add(new ColumnDefinition { Width = column.Width });

                    //    row.InitCell(column);
                    //}

                    // triggers
                    row.SetTriggers(triggers);

                    row.Draw();
                    return row;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DataGrid::RowGenerator: " + ex.ToString());
                    return null;
                }
            };

            return template;
        }
    }
}
