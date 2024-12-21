using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace DataGridSam.Internal;

public interface ICellCustomize
{
    double CellFontSize { get; set; }
    Color CellFontColor { get; set; }
    Color CellBackgroundColor { get; set; }

    TextAlignment CellHorizontalAlignment { get; set; }
    TextAlignment CellVerticalAlignment { get; set; }
}