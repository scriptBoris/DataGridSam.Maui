using System;
using System.Reflection;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using DataGridSam.Internal;
using WSetter = Microsoft.UI.Xaml.Setter;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using WVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
using WDataTemplate = Microsoft.UI.Xaml.DataTemplate;

namespace DataGridSam.Platforms.Windows;

public class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
{
    private DGCollection_Windows Proxy => (DGCollection_Windows)VirtualView;

    protected override ListViewBase CreatePlatformView()
    {
        var res = base.CreatePlatformView();
        Update(res);
        return res;
    }

    protected override void ConnectHandler(ListViewBase platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ShowsScrollingPlaceholders = false;
        platformView.ItemContainerTransitions = null;

        platformView.ContainerContentChanging += PlatformView_ContainerContentChanging;
    }

    private void PlatformView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue)
        {
            // Item is being recycled, make sure first item has no border
            if (args.ItemIndex == 0)
            {
                var first = (ListViewItem)sender.ContainerFromIndex(0);
                if (first != null)
                {
                    first.BorderThickness = new WThickness(0);
                }
            }
        }
        else if (args.ItemIndex == 0)
        {
            // A new first item
            ((ListViewItem)args.ItemContainer).BorderThickness = new WThickness(0);

            var second = (ListViewItem)sender.ContainerFromIndex(1);
            if (second != null)
            {
                second.ClearValue(ListViewItem.BorderThicknessProperty);
            }
        }
        else
        {
            // A new internal item
            ((ListViewItem)args.ItemContainer).ClearValue(ListViewItem.BorderThicknessProperty);
        }
    }

    public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        var res = base.GetDesiredSize(widthConstraint, heightConstraint);
        Proxy.ViewPortHeight = res.Height;
        return res;
    }

    public Task<Row?> GetRowAsync(int index, TimeSpan? timeout)
    {
        return Task.FromResult<Row?>(null);
    }

    public void UpdateNativeTapColor(Color color)
    {
    }

    internal void UpdateNativeBorderColor()
    {
        Update(PlatformView);
    }

    internal void UpdateNativeBorderThickness()
    {
        Update(PlatformView);
    }

    internal void Update(ListViewBase platformListView)
    {
        var borderColor = Proxy.BorderColor.ToWindowsColor();
        var borderThickness = new WThickness(0, Proxy.BorderThickness, 0, 0);

        platformListView.ItemContainerStyle = new Microsoft.UI.Xaml.Style(typeof(ListViewItem))
        {
            Setters =
            {
                new WSetter(ListViewItem.MinHeightProperty, 0),
                new WSetter(ListViewItem.HorizontalContentAlignmentProperty, WHorizontalAlignment.Stretch),
                new WSetter(ListViewItem.PaddingProperty, new WThickness(0)),
                new WSetter(ListViewItem.BorderBrushProperty, borderColor),
                new WSetter(ListViewItem.BorderThicknessProperty, borderThickness),
            },
        };
    }
}