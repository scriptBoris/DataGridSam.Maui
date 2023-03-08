﻿using Android;
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
    // All the code in this file is only included on Android.
    internal partial class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
    {
        private DividerItemDecoration? last;
        private DGCollection View => (DGCollection)VirtualView;

        protected override RecyclerView CreatePlatformView()
        {
            var res = base.CreatePlatformView();
            Update(res);
            return res;
        }

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            var res = base.GetDesiredSize(widthConstraint, heightConstraint);
            var v = VirtualView as DGCollection;
            v?.OnVisibleHeight(res.Height);
            return res;
        }

        private void Update(RecyclerView res)
        {
            if (last != null)
                res.RemoveItemDecoration(last);

            int color = View.BorderColor.ToAndroid().ToArgb();
            int width = (int)(View.BorderThickness * DeviceDisplay.MainDisplayInfo.Density);

            last ??= new DividerItemDecoration(Context, DividerItemDecoration.Vertical);
            var drawable = new GradientDrawable(GradientDrawable.Orientation.BottomTop, new[] {
                color,
                color,
            });
            drawable.SetSize(1, width);
            last.Drawable = drawable;
            res.AddItemDecoration(last);
        }

        public void UpdateBorderColor()
        {
            Update(PlatformView);
        }

        public void UpdateBorderWidth()
        {
            Update(PlatformView);
        }
    }
}