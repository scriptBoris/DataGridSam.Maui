using Sample.Core;
using Sample.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sample.ViewModels
{
    public class DataGridColorizedColumnsAndTriggersVm : BaseNotify
    {
        public ObservableCollection<User> Items => DataCollector.GenerateUsers(300);

        public ICommand CommandSelectItem => new Command((x) =>
        {
            if (x is User user)
            {
                App.Current?.MainPage?.DisplayAlert("Selected", $"{user.FirstName} {user.LastName}", "OK");
            }
        });
    }
}
