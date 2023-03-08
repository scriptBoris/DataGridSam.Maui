using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam.Internal
{
    internal interface IDataTriggerHost
    {
        void Execute(IDataTrigger item, object? value);
    }
}
