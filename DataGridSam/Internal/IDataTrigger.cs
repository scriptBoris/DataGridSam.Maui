using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal
{
    internal interface IDataTrigger
    {
        DataGrid? DataGrid { get; set; }
        int? CellTriggerId { get; }
        BindingBase Binding { get; }
        object? Value { get; }
        object? CSharpValue { get; set; }


        Color? BackgroundColor { get; }
        Color? TextColor { get; }
        double? FontSize { get; }
        FontAttributes? FontAttributes { get; }
        TextAlignment? VerticalTextAlignment { get; }
        TextAlignment? HorizontalTextAlignment { get; }
    }
}
