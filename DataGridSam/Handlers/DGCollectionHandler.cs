using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Handlers
{
    public partial class DGCollectionHandler
    {
    }

    public interface IDGCollectionHandler
    {
        void UpdateBorderColor();
        void UpdateBorderWidth();
        Task<Row?> GetRowAsync(int index, TimeSpan? timeout);
    }
}
