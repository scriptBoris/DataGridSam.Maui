using DataGridSam.Handlers;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using UIKit;

namespace DataGridSam.Handlers
{
    public partial class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
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

        public void UpdateBorderColor()
        {
        }

        public void UpdateBorderWidth()
        {
        }

        public async Task<Row?> GetRowAsync(int index, TimeSpan? timeout)
        {
            return null;
        }
    }
}