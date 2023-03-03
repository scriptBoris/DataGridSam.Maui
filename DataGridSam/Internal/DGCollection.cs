using DataGridSam.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal
{
    internal class DGCollection : CollectionView
    {
        private Color _borderColor = Colors.Black;
        private double _borderThickness = 1;

        public event EventHandler<double>? VisibleHeightChanged;

        public double VisibleHeight { get; private set; }
        
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                if (Handler is IDGCollectionHandler hand)
                    hand.UpdateBorderColor();
            }
        }

        public double BorderThickness 
        {
            get => _borderThickness;
            set
            {
                _borderThickness = value;
                if (Handler is IDGCollectionHandler hand)
                    hand.UpdateBorderWidth();
            } 
        }

        public void OnVisibleHeight(double height)
        {
            VisibleHeight = height;
            VisibleHeightChanged?.Invoke(this, height);
        }
    }
}
