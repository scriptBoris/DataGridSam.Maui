using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Core
{
    public interface ICell
    {
        Color TextColor { get; set; }
        double FontSize { get; set; }
        FontAttributes FontAttributes { get; set; }
        TextAlignment VerticalTextAlignment { get; set; }
        TextAlignment HorizontalTextAlignment { get; set; }
    }
}
