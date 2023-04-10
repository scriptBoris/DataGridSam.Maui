using Sample.Core;
using Sample.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.ViewModels
{
    public class DataGridColorizedColumnsVm : BaseNotify
    {
        public ObservableCollection<User> Users => DataCollector.GenerateUsers(300);
    }
}
