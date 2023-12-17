using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AView = Android.Views.View;

namespace DataGridSam.Handlers
{
    public partial class RowHandler : LayoutHandler
    {
        private AView? rippleLayout;

        public static bool IsSdk21 => Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop;
        public Row Proxy => (Row)VirtualView;

        protected override LayoutViewGroup CreatePlatformView()
        {
            var n = new LayoutViewGroupCustom(Context, OnLayoutChanged);
            n.TouchDelegate = new GestureTouch(this, n);
            return n;
        }

        protected override void ConnectHandler(LayoutViewGroup platformView)
        {
            ((LayoutViewGroupCustom)platformView).RowHandler = this;
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(LayoutViewGroup platformView)
        {
            ((LayoutViewGroupCustom) platformView).RowHandler = null;
            base.DisconnectHandler(platformView);
        }

        private void OnLayoutChanged()
        {
            if (rippleLayout == null)
                return;

            rippleLayout.Bottom = PlatformView.Bottom;
            rippleLayout.Right = PlatformView.Right;
        }

        internal void AnimationStart(float x, float y)
        {
            if (IsSdk21)
            {
                RippleStart(x, y);
                Proxy.OnTapStart();
            }
            else
            {
                Proxy.OnTapStart_Common();
            }
        }

        internal void AnimationFinish(bool needTrigger)
        {
            if (IsSdk21)
            {
                RippleEnd();
                Proxy.OnTapFinish(needTrigger ? TapFinishModes.Tap : TapFinishModes.Cancel);
            }
            else
            {
                Proxy.OnTapFinish_Common(needTrigger);
            }
        }

        private void RippleStart(float x, float y)
        {
            if (rippleLayout == null)
            {
                rippleLayout = new LayoutViewGroupCustom(Context, null);
                rippleLayout.Bottom = PlatformView.Bottom;
                rippleLayout.Right = PlatformView.Right;
                rippleLayout.Background = CreateRipple(Proxy.TapColor);
                PlatformView.AddView(rippleLayout);
            }

            rippleLayout.Background?.SetHotspot(x, y);
            PlatformView.Pressed = true;
        }

        private void RippleEnd()
        {
            PlatformView.Pressed = false;
        }

        private RippleDrawable CreateRipple(Color color)
        {
            var mask = new ColorDrawable(Colors.White.ToPlatform());
            return new RippleDrawable(GetRippleColorSelector(color.ToPlatform()), null, mask);
        }

        internal void UpdateTapColor(Color color)
        {
//#if ANDROID21_0_OR_GREATER
            if (rippleLayout?.Background is RippleDrawable ripple)
            {
                var ls = GetRippleColorSelector(color.ToAndroid());
                ripple.SetColor(ls);
            }
//#endif
        }

        private static ColorStateList GetRippleColorSelector(int pressedColor)
        {
            return new ColorStateList
            (
                new int[][] { new int[] { } },
                new int[] { pressedColor, }
            );
        }
    }

    public class LayoutViewGroupCustom : LayoutViewGroup
    {
        private readonly Action? onLayoutChanged;

        public RowHandler? RowHandler { get; set; }

        public LayoutViewGroupCustom(Android.Content.Context context, Action? onLayoutChanged) : base(context)
        {
            this.onLayoutChanged = onLayoutChanged;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);
            onLayoutChanged?.Invoke();
        }
    }

    internal class GestureTouch : TouchDelegate
    {
        private readonly RowHandler _host;
        private readonly int _touchSlop;
        private float startX;
        private float startY;
        private bool isPressedAndIdle;

        public GestureTouch(RowHandler host, Android.Views.View view) : base(null, view)
        {
            _host = host;
            _touchSlop = ViewConfiguration.Get(host.Context)?.ScaledTouchSlop ?? 5;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            float x = e.GetX();
            float y = e.GetY();

            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                    isPressedAndIdle = true;
                    startX = x;
                    startY = y;
                    _host.AnimationStart(x, y);
                    break;

                case MotionEventActions.Move:
                    float deltaX = Math.Abs(startX - x);
                    float deltaY = Math.Abs(startY - y);

                    if (deltaX > _touchSlop || deltaY > _touchSlop)
                    {
                        isPressedAndIdle = false;
                        _host.AnimationFinish(false);
                    }
                    break;

                case MotionEventActions.Up:
                    _host.AnimationFinish(isPressedAndIdle);
                    isPressedAndIdle = false;
                    break;

                case MotionEventActions.Cancel:
                    _host.AnimationFinish(false);
                    isPressedAndIdle = false;
                    break;

                default:
                    break;
            }

            return true;
        }
    }
}
