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
    private DGCollection Proxy => (DGCollection)VirtualView;

    protected override ListViewBase CreatePlatformView()
    {
        var res = base.CreatePlatformView();
        return res;
    }

    protected override void ConnectHandler(ListViewBase platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ShowsScrollingPlaceholders = false;
        platformView.ItemContainerTransitions = null;
    }

    public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        var res = base.GetDesiredSize(widthConstraint, heightConstraint);
        Proxy.OnVisibleHeight(res.Height);
        return res;
    }

    public Task<Row?> GetRowAsync(int index, TimeSpan? timeout)
    {
        return Task.FromResult<Row?>(null);
    }

    public void UpdateNativeTapColor(Color color)
    {
        // todo сделать нативный эффект нажатия на строку? (windows)
    }
}