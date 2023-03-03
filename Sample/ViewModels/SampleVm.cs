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
    public class SampleVm : BaseNotify
    {
        public SampleVm()
        {
            Items = DataCollector.GetUsers2();
        }

        public ObservableCollection<User> Items { get; set; }
        public ICommand CommandSelectedRow => new Command(x =>
        {
            if (x is User user)
            {
                App.Current?.MainPage?.DisplayAlert(
                    "Selected", 
                    $"{user.FirstName} {user.LastName}, rank {user.Rank}", 
                    "OK");
            }
        });

        public ICommand CommandLongSelectedRow => new Command(x =>
        {
            if (x is User user)
            {
                App.Current?.MainPage?.DisplayAlert(
                    "Long selected",
                    $"{user.FirstName} {user.LastName}, rank {user.Rank}",
                    "OK");
            }
        });
    }
}
