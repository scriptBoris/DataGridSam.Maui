using DataGridSam.Internal;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WSetter = Microsoft.UI.Xaml.Setter;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using WVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace DataGridSam.Handlers
{
    internal partial class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
    {
        private DGCollection View => (DGCollection)VirtualView;

        protected override ListViewBase CreatePlatformView()
        {
            var res = base.CreatePlatformView();
            Update(res);
            return res;
        }

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            var res = base.GetDesiredSize(widthConstraint, heightConstraint);
            View.OnVisibleHeight(res.Height);
            return res;
        }

        public void UpdateBorderColor()
        {
            Update(PlatformView);
        }

        public void UpdateBorderWidth()
        {
            Update(PlatformView);
        }

        private void Update(ListViewBase ls)
        {
            var color = View.BorderColor.ToWindowsColor();
            var width = new WThickness(0, 0, 0, View.BorderThickness);
            ls.ItemContainerStyle = new Microsoft.UI.Xaml.Style(typeof(ListViewItem))
            {
                Setters =
                {
                    new WSetter(ListViewItem.MinHeightProperty, 0),
                    new WSetter(ListViewItem.VerticalAlignmentProperty, WVerticalAlignment.Top),
                    new WSetter(ListViewItem.VerticalContentAlignmentProperty, WVerticalAlignment.Top),
                    new WSetter(ListViewItem.HorizontalContentAlignmentProperty, WHorizontalAlignment.Stretch),
                    new WSetter(ListViewItem.PaddingProperty, new WThickness(0)),
                    new WSetter(ListViewItem.MarginProperty, new WThickness(0)),
                    new WSetter(ListViewItem.BorderBrushProperty, color),
                    new WSetter(ListViewItem.BorderThicknessProperty, width),
                },
            };
        }
    }
}