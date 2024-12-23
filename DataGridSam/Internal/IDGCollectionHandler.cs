using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal;

public interface IDGCollectionHandler
{
    void UpdateNativeTapColor(Color color);
    Task<Row?> GetRowAsync(int index, TimeSpan? timeout);
}