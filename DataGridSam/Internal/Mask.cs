using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace DataGridSam.Internal;

internal class Mask : GraphicsView
{
    internal Mask(DataGrid dataGrid)
    {
        InputTransparent = true;
        Drawable = new MaskDrawable(dataGrid, this);
    }

    internal bool HasExternalBorders { get; set; } = true;

    internal Color? BorderColor
    {
        set
        {
            Invalidate();
        }
    }

    internal double BorderWidth
    {
        set
        {
            Invalidate();
        }
    }

    internal void ThrowInvalidateMeasure()
    {
        Invalidate();
    }

    internal void Redraw(ObservableCollection<DataGridColumn>? columns = null)
    {
        Invalidate();
    }

    private class MaskDrawable : IDrawable
    {
        private readonly Mask _mask;
        private readonly DataGrid _dataGrid;

        public MaskDrawable(DataGrid datagrid, Mask mask)
        {
            _dataGrid = datagrid;
            _mask = mask;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Очищаем холст
            canvas.FillColor = Colors.Transparent;
            canvas.FillRectangle(dirtyRect);

            Color borderColor = _dataGrid.BordersColor;
            double borderWidth = _dataGrid.BordersThickness;
            float borderWidthF = (float)borderWidth;
            double width = _mask.Frame.Width;
            double height = _mask.Frame.Height;

            if (_dataGrid.BordersThickness <= 0.01)
                return;

            canvas.StrokeColor = borderColor;
            canvas.StrokeSize = borderWidthF;// + 0.1f;

            if (_mask.HasExternalBorders)
            {
                // Отрисовка внешних границ
                double offset = borderWidth / 2;
                var adjustedRect = new Rect(
                    x: offset,
                    y: offset,
                    width: width - borderWidth,
                    height: height - borderWidth
                );

                canvas.DrawRectangle(adjustedRect);
            }

            // Отрисовка вертикальных линий для колонок (внутренние границы)
            var cols = _dataGrid.Columns;
            if (cols.Count <= 1)
                return;

            float lineOffset = borderWidthF / 2;
            float x = borderWidthF;
            float y0 = 0;
            float y1 = (float)height;

            for (int i = 0; i < cols.Count - 1; i++)
            {
                float pos = (float)_dataGrid.CachedWidths[i];
                x += pos + lineOffset;
                canvas.DrawLine(x, y0, x, y1);
                x += lineOffset;
            }
        }
    }
}
