using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System.Collections;

namespace DataGridSam.Internal;

internal interface IDGCollection : IView
{
    event EventHandler<ItemsViewScrolledEventArgs> Scrolled;
    double ViewPortHeight { get; }
    DataTemplate ItemTemplate { get; set; }
    Color BorderColor { get; set; }
    double BorderThickness { get; set; }
    IEnumerable? ItemsSource { get; set; }

    void UpdateCellsVisual(bool needRecalcMeasure);

    /// <summary>
    /// Обновить для внутренние отступы
    /// </summary>
    /// <param name="columnId">Если null - все колонки</param>
    void UpdateCellsPadding(int? columnId);

    void UpdateCellsMeasure();
    void Redraw();
    void RestructColumns();
    void RebindColumn(int index);
    void ScrollTo(int index, int groudId, ScrollToPosition position, bool animate);
    void ScrollTo(object item, object? group, ScrollToPosition position, bool animate);
    Task<Row?> GetRowFast(int index);
}