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
            Items = DataCollector.GenerateUsers(100);
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

        public ICommand CommandLongSelectedRow => new Command(async x =>
        {
            if (x is User user)
            {
                const string cancel = "Cancel";

                string[] items = Enum.GetValues<Ranks>()
                    .Select(x => x.ToString())
                    .ToArray();

                string? res = await App.Current?.MainPage?.DisplayActionSheet("Select rank", cancel, null!, items);
                if (res == null || res == cancel)
                    return;

                var newRank = Enum.Parse<Ranks>(res);
                user.Rank = newRank;
            }
        });
    }
}
