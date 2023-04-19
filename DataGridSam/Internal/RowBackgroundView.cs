using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal
{
    internal class RowBackgroundView : SKCanvasView
    {
        private float spacing;
        private float[]? widths;
        private Color?[]? cellColors;
        private Color? mainColor;

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            var info = e.Info;
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            if (widths == null || cellColors == null || mainColor == null)
                return;

            using var paintMain = new SKPaint
            {
                Color = mainColor.ToSKColor(),
            };
            canvas.DrawRect(0, 0, info.Width, info.Height, paintMain);
            canvas.Save();

            float x = 0;
            for(int i = 0; i < widths.Length; i++)
            {
                var color = cellColors[i];
                float w = widths[i];
                float h = info.Height;

                if (color != null)
                {
                    using var paint = new SKPaint
                    {
                        Color = color.ToSKColor(),
                    };
                    canvas.DrawRect(x, 0, w, h, paint);
                }
                x += w + spacing;
            }
        }

        internal void Redraw(float spacing, float[] widths, Color mainColor, Color?[] cellColors)
        {
            this.spacing = spacing;
            this.widths = widths;
            this.mainColor = mainColor;
            this.cellColors = cellColors;
            this.InvalidateSurface();
        }
    }
}
