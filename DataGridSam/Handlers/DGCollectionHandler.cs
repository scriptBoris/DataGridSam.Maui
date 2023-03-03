using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Handlers
{
    internal partial class DGCollectionHandler
    {
    }

    internal interface IDGCollectionHandler
    {
        void UpdateBorderColor();
        void UpdateBorderWidth();
    }
}
