using DataGridSam.Internal;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DataGridSam.Handlers
{
    // All the code in this file is only included on Windows.
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
            var width = new Microsoft.UI.Xaml.Thickness(0, 0, 0, View.BorderThickness);
            ls.ItemContainerStyle = new Microsoft.UI.Xaml.Style(typeof(ListViewItem))
            {
                Setters =
                {
                    new Microsoft.UI.Xaml.Setter(ListViewItem.BorderBrushProperty, color),
                    new Microsoft.UI.Xaml.Setter(ListViewItem.BorderThicknessProperty, width),
                },
            };
        }
    }
}