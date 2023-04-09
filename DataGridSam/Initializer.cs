using DataGridSam.Handlers;
using DataGridSam.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam
{
    public static class DataGridExt
    {
        public static bool IsInitialized { get; internal set; }

        public static MauiAppBuilder UseDataGridSam(this MauiAppBuilder builder)
        {
            IsInitialized = true;
            builder.ConfigureMauiHandlers(hand =>
            {
                hand.AddHandler(typeof(DGCollection), typeof(DGCollectionHandler));
            });
            return builder;
        }
    }
}
