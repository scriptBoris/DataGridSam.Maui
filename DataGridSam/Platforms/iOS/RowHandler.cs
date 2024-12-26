using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Foundation;
using UIKit;

namespace DataGridSam.Platforms.iOS;

public class RowHandler : LayoutHandler
{
    private bool isPressed;

    public Row Proxy => (Row)VirtualView;

    protected override LayoutView CreatePlatformView()
    {
        var view = new RowTouchView(this);
        return view;
    }

    private void OnTouchBegan()
    {
        isPressed = true;
        Proxy.OnTapStart_Common();
    }

    private void OnTouchEnded(bool isInside, bool isRightClick = false)
    {
        if (isPressed)
        {
            Proxy.OnTapFinish_Common(isInside, isRightClick);
        }
        isPressed = false;
    }

    private void OnTouchCancelled()
    {
        if (isPressed)
            Proxy.OnTapFinish_Common(false);

        isPressed = false;
    }

    private class RowTouchView : LayoutView
    {
        private readonly RowHandler _handler;

        public RowTouchView(RowHandler handler)
        {
            _handler = handler;
            UserInteractionEnabled = true;
        }

        public override void TouchesBegan(NSSet touches, UIEvent? evt)
        {
            base.TouchesBegan(touches, evt);
            _handler.OnTouchBegan();
        }

        public override void TouchesEnded(NSSet touches, UIEvent? evt)
        {
            base.TouchesEnded(touches, evt);
            if (touches.AnyObject is UITouch touch)
            {
                var location = touch.LocationInView(this);
                var isInside = Bounds.Contains(location);
                _handler.OnTouchEnded(isInside);
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent? evt)
        {
            base.TouchesMoved(touches, evt);
            if (touches.AnyObject is UITouch touch)
            {
                var location = touch.LocationInView(this);
                var isInside = Bounds.Contains(location);
                if (!isInside)
                    _handler.OnTouchCancelled();
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent? evt)
        {
            base.TouchesCancelled(touches, evt);
            _handler.OnTouchCancelled();
        }
    }
}