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
            hand.AddHandler(typeof(RowBackgroundView), typeof(SKCanvasViewHandler));
            hand.AddHandler(typeof(Mask), typeof(SKCanvasViewHandler));

#if ANDROID
            hand.AddHandler(typeof(DGCollection_Android), typeof(DataGridSam.Platforms.Android.DGCollectionHandler));
            hand.AddHandler(typeof(Row), typeof(DataGridSam.Platforms.Android.RowHandler));
#elif IOS
            hand.AddHandler(typeof(DGCollection_iOS), typeof(DataGridSam.Platforms.iOS.DGCollectionHandler));
            hand.AddHandler(typeof(Row), typeof(DataGridSam.Platforms.iOS.RowHandler));
#elif WINDOWS
            hand.AddHandler(typeof(DGCollection_Windows), typeof(DataGridSam.Platforms.Windows.DGCollectionHandler));
            hand.AddHandler(typeof(Row), typeof(DataGridSam.Platforms.Windows.RowHandler));
#else
            throw new NotSupportedException("DataGridSam no support current OS");
#endif
        });
        return builder;
    }
}