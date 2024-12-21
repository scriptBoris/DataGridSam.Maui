using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace DataGridSam.Internal;

public interface IHeaderCustomize
{
    double HeaderFontSize { get; set; }
    Color HeaderTextColor { get; set; }
    Color HeaderBackgroundColor { get; set; }
    TextAlignment HeaderHorizontalAlignment { get; set; }
    TextAlignment HeaderVerticalAlignment { get; set; }
}