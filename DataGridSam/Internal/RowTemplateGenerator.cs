using DataGridSam.Extensions;
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
            var triggers = dataGrid.RowTriggers;

            triggers.InitTriggers(dataGrid);

            var template = new DataTemplate();
            template.LoadTemplate = () =>
            {
                try
                {
                    var row = new Row(columns.Count);

                    Button? button = null;
                    if (dataGrid.RowSelectedCommand != null || dataGrid.RowLongSelectedCommand != null)
                    {
                        button = new DGButton()
                        {
                            CornerRadius = 0,
                            Padding = 0,
                            Margin = 0,
                            BackgroundColor = Colors.Transparent,
                            Command = dataGrid.RowSelectedCommand,
                            CommandLongClick = dataGrid.RowLongSelectedCommand,
                        };
                        button.SetBinding(Button.CommandParameterProperty, Binding.SelfPath);
                        row.Children.Add(button);
                    }

                    var colDef = new ColumnDefinitionCollection();
                    for (int i = 0; i < columns.Count; i++)
                    {
                        var column = columns[i];
                        colDef.Add(new ColumnDefinition { Width = column.Width });

                        row.InitCell(column);
                    }

                    if (button != null)
                        row.SetColumnSpan(button, columns.Count);

                    // row triggers
                    row.SetTriggers(triggers, totalTriggerCount);

                    row.ColumnDefinitions = colDef;
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
