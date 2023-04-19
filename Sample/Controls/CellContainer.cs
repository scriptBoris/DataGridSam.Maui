using DataGridSam.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Controls
{
    public class CellContainer : VerticalStackLayout, ICell
    {
        private Color _textColor = Colors.Transparent;

        public double FontSize { get; set; }
        public FontAttributes FontAttributes { get; set; }
        public TextAlignment VerticalTextAlignment { get; set; }
        public TextAlignment HorizontalTextAlignment { get; set; }
        public Color TextColor 
        {
            get => _textColor; 
            set
            {
                _textColor = value;
                foreach (var item in Children)
                {
                    if (item is Label label)
                        label.TextColor = value;
                }
            }
        }
    }
}
