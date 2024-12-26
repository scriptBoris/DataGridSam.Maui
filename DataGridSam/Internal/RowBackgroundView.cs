using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace DataGridSam.Internal;

internal class RowBackgroundView : GraphicsView
{
    private double _spacing;
    private double[]? _widths;
    private Color?[]? _cellColors;
    private Color? _mainColor;

    public RowBackgroundView()
    {
        Drawable = new RowDrawable(this);
        InputTransparent = true;
    }

    internal void Redraw(double spacing, double[] widths, Color mainColor, Color?[] cellColors)
    {
        _spacing = spacing;
        _widths = widths;
        _mainColor = mainColor;
        _cellColors = cellColors;
        Invalidate();
    }

    private class RowDrawable(RowBackgroundView _rowBackgroundView) : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Transparent;
            canvas.FillRectangle(dirtyRect);

            var widths = _rowBackgroundView._widths;
            var cellColors = _rowBackgroundView._cellColors;
            var mainColor = _rowBackgroundView._mainColor;
            var spacing = (float)_rowBackgroundView._spacing;

            if (widths == null || cellColors == null || mainColor == null)
                return;

            var rect = _rowBackgroundView.Frame;
            canvas.FillColor = mainColor;
            canvas.FillRectangle(rect);

            float x = 0;
            for (int i = 0; i < widths.Length; i++)
            {
                var color = cellColors[i];
                float w = (float)widths[i];
                float h = (float)_rowBackgroundView.Frame.Height;

                if (color != null)
                {
                    canvas.FillColor = color;
                    canvas.FillRectangle(x, 0, w, h);
                }
                x += w + spacing;
            }
        }
    }
}