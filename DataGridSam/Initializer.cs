using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGridSam.Internal;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Handlers;

namespace DataGridSam;

public static class DataGridExt
{
    public static bool IsInitialized { get; internal set; }

    public static MauiAppBuilder UseDataGridSam(this MauiAppBuilder builder)
    {
        IsInitialized = true;
        builder.ConfigureMauiHandlers(hand =>
        {
#if ANDROID
            hand.AddHandler(typeof(DGCollection), typeof(DataGridSam.Platforms.Android.DGCollectionHandler));
            hand.AddHandler(typeof(Row), typeof(DataGridSam.Platforms.Android.RowHandler));
            hand.AddHandler(typeof(RowBackgroundView), typeof(SKCanvasViewHandler));
#elif IOS
            hand.AddHandler(typeof(DGCollection), typeof(DataGridSam.Platforms.iOS.DGCollectionHandler));
            hand.AddHandler(typeof(Row), typeof(DataGridSam.Platforms.iOS.RowHandler));
            hand.AddHandler(typeof(RowBackgroundView), typeof(SKCanvasViewHandler));
#elif WINDOWS
            hand.AddHandler(typeof(DGCollection), typeof(DataGridSam.Platforms.Windows.DGCollectionHandler));
            hand.AddHandler(typeof(Row), typeof(DataGridSam.Platforms.Windows.RowHandler));
            hand.AddHandler(typeof(RowBackgroundView), typeof(SKCanvasViewHandler));
#else
            throw new NotSupportedException("DataGridSam no support current OS");
#endif
        });
        return builder;
    }
}