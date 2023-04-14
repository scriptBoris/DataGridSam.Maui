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
using Microsoft.Maui.Controls.Platform;
using System;

namespace DataGridSam.Handlers
{
    public partial class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
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

        public async Task<Row?> GetRowAsync(int index, TimeSpan? timeout)
        {
            return GetRowFast(index);
        }

        public Row? GetRowFast(int index)
        {
            int offset = GetOffset();
            if (offset < 0)
                return null;

            var item = PlatformView.ItemsPanelRoot.Children[index - offset] as Microsoft.UI.Xaml.Controls.ListViewItem;
            if (item == null)
                return null;

            var rootItem = (ItemContentControl)item.ContentTemplateRoot;
            var rowPanel = (LayoutPanelLinked)rootItem.Content;
            var row = rowPanel.Handler.Proxy;

            return row;
        }

        private int GetOffset()
        {
            var item = PlatformView.ItemsPanelRoot.Children.FirstOrDefault() as Microsoft.UI.Xaml.Controls.ListViewItem;
            if (item == null)
                return -1;

            int id = PlatformView.Items.IndexOf(item.Content);
            return id;
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