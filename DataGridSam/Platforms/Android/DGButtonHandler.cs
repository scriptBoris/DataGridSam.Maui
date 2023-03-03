using DataGridSam.Internal;
using Google.Android.Material.Button;
using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Handlers
{
    internal partial class DGButtonHandler : ButtonHandler
    {
        private DGButton? Proxy => VirtualView as DGButton;

        protected override void ConnectHandler(MaterialButton platformView)
        {
            base.ConnectHandler(platformView);
            platformView.LongClickable = true;
            platformView.LongClick += PlatformView_LongClick;
        }

        protected override void DisconnectHandler(MaterialButton platformView)
        {
            base.DisconnectHandler(platformView);
            platformView.LongClick -= PlatformView_LongClick;
        }

        private void PlatformView_LongClick(object? sender, Android.Views.View.LongClickEventArgs e)
        {
            Proxy?.CommandLongClick?.Execute(Proxy.CommandParameter);
        }
    }
}
