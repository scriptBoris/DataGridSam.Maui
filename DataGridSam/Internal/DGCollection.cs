using DataGridSam.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal
{
    public class DGCollection : CollectionView
    {
        private readonly DataGrid _dataGrid;
        private Color _borderColor = Colors.Black;
        private double _borderThickness = 1;

        private double cachedWidth = -1;

        public event EventHandler<double>? VisibleHeightChanged;

        public DGCollection(DataGrid dataGrid)
        {
            this._dataGrid = dataGrid;
        }

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

        protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
        {
            _dataGrid.UpdateCellsWidthCache(widthConstraint, false);
            var res = base.MeasureOverride(widthConstraint, heightConstraint);
            return res;
        }
    }
}
