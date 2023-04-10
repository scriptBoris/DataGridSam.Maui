using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Handlers
{
    public partial class RowHandler : LayoutHandler
    {
        private bool isPressed;

        public Row Proxy => (Row)VirtualView;

        protected override LayoutPanel CreatePlatformView()
        {
            var n = base.CreatePlatformView();
            n.PointerCanceled += N_PointerCanceled;
            n.PointerPressed += N_PointerPressed;
            n.PointerReleased += N_PointerReleased;
            n.PointerExited += N_PointerExited;
            return n;
        }

        private void N_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (isPressed)
                Proxy.OnTapFinish_Common(false);

            isPressed = false;
        }

        private void N_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (isPressed)
            {
                var prop = e.GetCurrentPoint(PlatformView).Properties;
                if (prop.PointerUpdateKind == Microsoft.UI.Input.PointerUpdateKind.RightButtonReleased)
                    Proxy.OnTapFinish_Common(true, true);
                else
                    Proxy.OnTapFinish_Common(true);
            }

            isPressed = false;
        }

        private void N_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            isPressed = true;
            Proxy.OnTapStart_Common();
        }

        private void N_PointerCanceled(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (isPressed)
                Proxy.OnTapFinish_Common(false);

            isPressed = false;
        }
    }
}
