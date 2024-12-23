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
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace DataGridSam.Internal;

internal class Mask : SKCanvasView
{
    private readonly DataGrid _dataGrid;
    private SKPaint _externalBordersPaint;
    private SKPaint _columnsPaint;

    internal Mask(DataGrid dataGrid)
    {
        _dataGrid = dataGrid;
        InputTransparent = true;
        UpdatePaints();
    }

    internal bool HasExternalBorders { get; set; } = true;

    internal Color? BorderColor
    {
        set
        {
            UpdatePaints();
            InvalidateSurface();
        }
    }

    internal double BorderWidth
    {
        set
        {
            UpdatePaints();
            InvalidateSurface();
        }
    }

    [MemberNotNull(nameof(_externalBordersPaint))]
    [MemberNotNull(nameof(_columnsPaint))]
    private void UpdatePaints()
    {
        double den = DeviceDisplay.Current.MainDisplayInfo.Density;
        float width = (float)(_dataGrid.BordersThickness * den);

        _externalBordersPaint?.Dispose();
        _columnsPaint?.Dispose();

        _externalBordersPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = width,
            Color = _dataGrid.BordersColor.ToSKColor(),
        };

        _columnsPaint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            Color = _dataGrid.BordersColor.ToSKColor(),
            StrokeWidth = width,
        };
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var cv = e.Surface.Canvas;
        cv.Clear();

        var rect = e.Info.Rect;

        if (HasExternalBorders)
        {
            // external border
            var offset = _externalBordersPaint.StrokeWidth / 2;
            var adjustedRect = new SKRect(
                rect.Left + offset,
                rect.Top + offset,
                rect.Right - offset,
                rect.Bottom - offset
            );

            cv.DrawRect(adjustedRect, _externalBordersPaint);
        }

        // columns vertical lines (internal borders)
        var cols = _dataGrid.Columns;
        if (cols.Count <= 1)
            return;

        float den = (float)DeviceDisplay.Current.MainDisplayInfo.Density;
        float lineWidth = _columnsPaint.StrokeWidth;
        float lineOffset = lineWidth / 2;
        float x = lineWidth;
        float y0 = 0;
        float y1 = rect.Bottom;
        for (int i = 0; i < cols.Count - 1; i++)
        {
            float pos = (float)_dataGrid.CachedWidths[i] * den;
            x += pos + lineOffset;
            cv.DrawLine(x, y0, x, y1, _columnsPaint);
            x += lineOffset;
        }
    }

    internal void ThrowInvalidateMeasure()
    {
        UpdatePaints();
        InvalidateSurface();
    }

    internal void Redraw(ObservableCollection<DataGridColumn> columns = null)
    {
        InvalidateSurface();
    }
}
