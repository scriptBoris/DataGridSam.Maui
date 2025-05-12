using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataGridSam.Internal;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Devices;
using AView = Android.Views.View;
using AColor = Android.Graphics.Color;
using ARect = Android.Graphics.Rect;
using APaint = Android.Graphics.Paint;
using Android.Graphics.Drawables;
using Microsoft.Maui.Platform;
using System.Runtime.Versioning;

namespace DataGridSam.Platforms.Android;

public class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
{
    private readonly List<GetRowRequestItem> tcsList = new();
    private ScrollListener? scrollListener;
    private SpacingItemDecoration? _itemDecoration;

    private DGCollection_Android? Proxy => VirtualView as DGCollection_Android;

    [SupportedOSPlatformGuard("android29.0")]
    private bool IsDroid29_OrAbove => (int)global::Android.OS.Build.VERSION.SdkInt >= 29;

    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        if (scrollListener == null)
        {
            scrollListener = new ScrollListener(this);
            PlatformView.AddOnScrollListener(scrollListener);
        }

        if (view is IDGCollection collection)
        {
            if (collection.BorderThickness > 0)
            {
                _itemDecoration = new SpacingItemDecoration(collection.BorderThickness);
                PlatformView.AddItemDecoration(_itemDecoration);
            }
        }
    }

    protected override void ConnectHandler(RecyclerView platformView)
    {
        base.ConnectHandler(platformView);

        if (IsDroid29_OrAbove)
        {
            platformView.VerticalScrollBarEnabled = true;
            platformView.ScrollBarStyle = global::Android.Views.ScrollbarStyles.InsideOverlay;
            platformView.ScrollbarFadingEnabled = false;
            platformView.ScrollBarSize = 10;

            platformView.VerticalScrollbarThumbDrawable =
                AndroidX.AppCompat.Content.Res.AppCompatResources.GetDrawable(
                    Microsoft.Maui.ApplicationModel.Platform.AppContext,
                    Microsoft.Maui.Resource.Drawable.datagrid_scrollview_thumb);
        }
    }

    protected override void DisconnectHandler(RecyclerView platformView)
    {
        if (_itemDecoration != null)
            platformView.RemoveItemDecoration(_itemDecoration);

        base.DisconnectHandler(platformView);
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
        if (Proxy != null)
            Proxy.ViewPortHeight = res.Height;
        return res;
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

    public void UpdateNativeTapColor(Color color)
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

    internal void UpdateItemSpacing(double value)
    {
        if (value > 0)
        {
            if (_itemDecoration == null)
            {
                _itemDecoration = new SpacingItemDecoration(value);
                PlatformView.AddItemDecoration(_itemDecoration);
            }
            else
            {
                _itemDecoration.Spacing = value;
            }
        }
        else if (_itemDecoration != null)
        {
            PlatformView.RemoveItemDecoration(_itemDecoration);
            _itemDecoration = null;
        }
        PlatformView.InvalidateItemDecorations();
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

public class SpacingItemDecoration : RecyclerView.ItemDecoration
{
    private int _verticalSpacing;
    private double _spacing;

    public SpacingItemDecoration(double spacing)
    {
        Spacing = spacing;
    }

    public double Spacing
    {
        get => _spacing;
        set
        {
            _spacing = value;

            var den = global::Android.App.Application.Context.Resources?.DisplayMetrics?.Density ?? 0;
            if (den == 0)
                _verticalSpacing = 0;
            else
                _verticalSpacing = (int)(_spacing * den);
        }
    }

    public override void GetItemOffsets(ARect outRect, AView view, RecyclerView parent, RecyclerView.State state)
    {
        int position = parent.GetChildAdapterPosition(view);
        if (position == RecyclerView.NoPosition) 
            return;

        if (position > 0)
        {
            // Отступ сверху для всех, кроме первого
            outRect.Top = _verticalSpacing; 
            outRect.Bottom = 0;
        }
    }
}
