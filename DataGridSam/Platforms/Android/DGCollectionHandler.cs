using Android;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using AndroidX.RecyclerView.Widget;
using DataGridSam.Internal;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Org.W3c.Dom;

namespace DataGridSam.Handlers
{
    internal partial class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
    {
        private readonly List<GetRowRequestItem> tcsList = new();
        private DividerItemDecoration? last;
        private ScrollListener? scrollListener;

        private DGCollection Proxy => (DGCollection)VirtualView;

        protected override RecyclerView CreatePlatformView()
        {
            var res = base.CreatePlatformView();
            Update(res);
            return res;
        }

        public override void SetVirtualView(IView view)
        {
            base.SetVirtualView(view);

            if (scrollListener == null)
            {
                scrollListener  = new ScrollListener(this);
                PlatformView.AddOnScrollListener(scrollListener);
            }
        }

        public virtual void OnScrollChange()
        {
            var m = tcsList.Where(x => x.TrySetTcs(GetRowFast(x.Index))).ToArray();

            for (int i = m.Length - 1; i >= 0; i--)
            {
                var item = m[i];
                tcsList.Remove(item);
            }
        }

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            var res = base.GetDesiredSize(widthConstraint, heightConstraint);
            var v = VirtualView as DGCollection;
            v?.OnVisibleHeight(res.Height);
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

        private void Update(RecyclerView res)
        {
            if (last != null)
                res.RemoveItemDecoration(last);

            int color = Proxy.BorderColor.ToAndroid().ToArgb();
            int width = (int)(Proxy.BorderThickness * DeviceDisplay.MainDisplayInfo.Density);

            last ??= new DividerItemDecoration(Context, DividerItemDecoration.Vertical);
            var drawable = new GradientDrawable(GradientDrawable.Orientation.BottomTop, new[] {
                color,
                color,
            });
            drawable.SetSize(1, width);
            last.Drawable = drawable;
            res.AddItemDecoration(last);
        }

        public async Task<Row?> GetRowAsync(int index, TimeSpan? timeout)
        {
            var row = GetRowFast(index);
            if (row == null)
            {
                var tsc = new TaskCompletionSource<Row?>();
                var tscItem = new GetRowRequestItem()
                {
                    Index = index,
                    Tsc = tsc,
                };
                tcsList.Add(tscItem);

                if (timeout != null)
                {
                    Proxy.Dispatcher.StartTimer(timeout.Value, () =>
                    {
                        tcsList.Remove(tscItem);
                        tsc.TrySetResult(null);
                        return false;
                    });
                }

                row = await tsc.Task;
            }

            return row;
        }

        public Row? GetRowFast(int index)
        {
            var position = PlatformView.FindViewHolderForAdapterPosition(index);
            if (position != null)
            {
                var item = position as Microsoft.Maui.Controls.Handlers.Items.TemplatedItemViewHolder;
                if (item?.View is Row row)
                    return row;
            }
            return null;
        }

        private struct GetRowRequestItem
        {
            public TaskCompletionSource<Row?> Tsc { get; set; }
            public int Index { get; set; }

            public bool TrySetTcs(Row? row)
            {
                if (row != null)
                {
                    Tsc.TrySetResult(row);
                    return true;    
                }

                return false;
            }
        }
    }

    internal class ScrollListener : RecyclerView.OnScrollListener
    {
        private readonly DGCollectionHandler handler;

        public ScrollListener(DGCollectionHandler dGCollectionHandler)
        {
            this.handler = dGCollectionHandler;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);
            handler.OnScrollChange();
        }
    }
}