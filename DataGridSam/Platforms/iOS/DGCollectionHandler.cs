using UIKit;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using DataGridSam.Internal;

namespace DataGridSam.Platforms.iOS;

public class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
{
    protected override ItemsViewController<ReorderableItemsView> CreateController(ReorderableItemsView itemsView, ItemsViewLayout layout)
    {
        var res = base.CreateController(itemsView, layout);
        var type = res.GetType();
        return res;
    }

    protected override UIView CreatePlatformView()
    {
        var res = base.CreatePlatformView();
        //var cv = res.Subviews.FirstOrDefault() as UICollectionView;

        //var l = cv.CollectionViewLayout as ListViewLayout;
        //l.SectionInset = new UIEdgeInsets(0, 0, 2, 0);
        //l.MinimumInteritemSpacing = 2;
        //l.MinimumLineSpacing = 2;

        return res;
    }

    public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        var size = base.GetDesiredSize(widthConstraint, heightConstraint);
        if (VirtualView is DGCollection_iOS collection)
            collection.ViewPortHeight = size.Height;

        return size;
    }

    public async Task<Row?> GetRowAsync(int index, TimeSpan? timeout)
    {
        return null;
    }

    public void UpdateNativeTapColor(Color color)
    {
        // todo сделать нативный эффект нажатия на строку? (ios)
    }
}