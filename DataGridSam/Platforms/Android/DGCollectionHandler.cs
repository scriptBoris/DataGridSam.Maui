using Android;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using DataGridSam.Internal;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Handlers.Items;

namespace DataGridSam.Handlers
{
    public partial class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
    {
        private readonly List<GetRowRequestItem> tcsList = new();
        private DividerItemDecoration? itemDecorator;
        private ScrollListener? scrollListener;

        private DGCollection? Proxy => VirtualView as DGCollection;

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
                scrollListener = new ScrollListener(this);
                PlatformView.AddOnScrollListener(scrollListener);
            }
        }

        public virtual void OnScrolled()
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
            UpdateItemDecorator();
        }

        public void UpdateBorderWidth()
        {
            UpdateItemDecorator();
        }

        private void Update(RecyclerView res)
        {
            UpdateItemDecorator(res);
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
                    Proxy?.Dispatcher.StartTimer(timeout.Value, () =>
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

        // TODO Нужно ли это?
        public IEnumerable<Row> GetVisibleRows()
        {
            var list = new List<Row>();
            var layoutManager = PlatformView.GetLayoutManager();

            int i = 0;
            while(true)
            {
                var aview = layoutManager?.GetChildAt(i);
                if (aview is Microsoft.Maui.Controls.Handlers.Items.ItemContentView icv)
                {
                    var custom = icv.GetChildAt(0);
                    if (custom is LayoutViewGroupCustom pl && pl.RowHandler != null)
                    {
                        var row = (Row)pl.RowHandler.VirtualView;
                        list.Add(row);
                    }
                }
                else
                {
                    break;
                }
                i++;
            }

            return list;
        }

        private void UpdateItemDecorator(RecyclerView? platformView = null)
        {
            if (Proxy == null)
                return;

            platformView ??= PlatformView;

            if (itemDecorator != null)
            {
                platformView.RemoveItemDecoration(itemDecorator);
                itemDecorator.Dispose();
            }

            var color = Proxy.BorderColor.ToAndroid();
            int width = (int)(Proxy.BorderThickness * DeviceDisplay.MainDisplayInfo.Density);

            itemDecorator = new DividerItemDecoration(width, color);

            platformView.AddItemDecoration(itemDecorator);
        }

        internal void UpdateTapSelectColor(Microsoft.Maui.Graphics.Color color)
        {
            if (Proxy == null)
                return;

            var rows = GetVisibleRows();
            foreach (var item in rows)
            {
                if (item.Handler is RowHandler row)
                    row.UpdateTapColor(color);
            }
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

    public class ScrollListener : RecyclerView.OnScrollListener
    {
        private readonly DGCollectionHandler handler;

        public ScrollListener(DGCollectionHandler dGCollectionHandler)
        {
            this.handler = dGCollectionHandler;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);
            handler.OnScrolled();
        }
    }

    public class DividerItemDecoration : RecyclerView.ItemDecoration
    {
        private readonly Android.Graphics.Paint _paint;

        public DividerItemDecoration(int width, Android.Graphics.Color color)
        {
            Width = width;
            Color = color;
            WidthDel2 = (int)(width * 0.3f);

            _paint = new()
            {
                Color = color,
            };
        }

        public int Width { get; private set; }
        public int WidthDel2 { get; private set; }
        public Android.Graphics.Color Color { get; private set; }

        public override void OnDrawOver(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            int x = 0;// parent.PaddingLeft;
            int right = parent.Width;// - parent.PaddingRight;

            int childCount = parent.ChildCount;
            for (int i = 0; i < childCount - 1; i++)
            {
                var child = parent.GetChildAt(i);
                var parameters = (RecyclerView.LayoutParams)child.LayoutParameters;

                //int top = child.Bottom + parameters.BottomMargin;
                //_drawable.SetBounds(0, top, right, bottom);
                //_drawable.Draw(c);

                int top = child.Bottom + parameters.BottomMargin;
                int bottom = top + Width;
                c.DrawRect(0, top, right, bottom, _paint);
                //c.DrawLine(0, top, right, top, _paint2);
            }
        }

        public override void GetItemOffsets(Android.Graphics.Rect outRect, Android.Views.View view, RecyclerView parent, RecyclerView.State state)
        {
            // Добавляем отступ только между элементами, а не после последнего элемента
            outRect.Bottom = Width;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _paint.Dispose();
        }

        private class LineDrawable : Drawable
        {
            private Android.Graphics.Paint paint;

            public LineDrawable(Android.Graphics.Paint paint)
            {
                this.paint = paint;
            }

            public override void Draw(Canvas canvas)
            {
                //canvas.DrawLine(Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Bottom, paint);
                canvas.DrawRect(Bounds, paint);
            }

            public override void SetAlpha(int alpha)
            {
                paint.Alpha = alpha;
            }

            public override void SetColorFilter(Android.Graphics.ColorFilter? colorFilter)
            {
                paint.SetColorFilter(colorFilter);
            }

            public override int Opacity => paint.Alpha;
        }
    }
}