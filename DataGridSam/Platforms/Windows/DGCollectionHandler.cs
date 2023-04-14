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
using WDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using Microsoft.UI.Xaml.Data;

namespace DataGridSam.Handlers
{
    internal partial class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
    {
        private DGCollection Proxy => (DGCollection)VirtualView;

        protected override ListViewBase CreatePlatformView()
        {
            var res = base.CreatePlatformView();
            Update(res);
            return res;
        }

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            var res = base.GetDesiredSize(widthConstraint, heightConstraint);
            Proxy.OnVisibleHeight(res.Height);
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

        public async Task<Row?> GetRow(int index)
        {
            var c = PlatformView.ItemsPanelRoot.Children;
            int count = 0;
            UIElement? item = null;
            while(count <3)
            {
                item = c[index];
                if (item == null)
                {
                    await Task.Delay(50);
                    count++;
                }
            }

            return null;
        }

        private void Update(ListViewBase ls)
        {
            var color = Proxy.BorderColor.ToWindowsColor();
            var width = new WThickness(0, 0, 0, Proxy.BorderThickness);

            // TODO WINDOWS: Сделать удаление сепаратора для последнего элемента
            ls.ItemContainerStyle = new Microsoft.UI.Xaml.Style(typeof(ListViewItem))
            {
                Setters =
                {
                    // todo нужны ли эти сеттеры?
                    //new WSetter(ListViewItem.VerticalAlignmentProperty, WVerticalAlignment.Top),
                    //new WSetter(ListViewItem.VerticalContentAlignmentProperty, WVerticalAlignment.Top),
                    //new WSetter(ListViewItem.MarginProperty, new WThickness(0)),

                    new WSetter(ListViewItem.MinHeightProperty, 0),
                    new WSetter(ListViewItem.HorizontalContentAlignmentProperty, WHorizontalAlignment.Stretch),
                    new WSetter(ListViewItem.PaddingProperty, new WThickness(0)),
                    new WSetter(ListViewItem.BorderBrushProperty, color),
                    new WSetter(ListViewItem.BorderThicknessProperty, width),
                    //new WSetter(ListViewItem.ContentTemplateProperty, ct),
                },
            };
        }
    }
}