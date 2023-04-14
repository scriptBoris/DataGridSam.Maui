using DataGridSam.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;

namespace DataGridSam.Handlers
{
    public partial class DGCollectionHandler : CollectionViewHandler, IDGCollectionHandler
    {
        public void UpdateBorderColor()
        {
        }

        public void UpdateBorderWidth()
        {
        }

        public async Task<Row?> GetRowAsync(int index, TimeSpan? timeout)
        {
            return null;
        }
    }
}