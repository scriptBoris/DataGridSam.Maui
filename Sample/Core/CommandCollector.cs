using Sample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sample.Core
{
    public static class CommandCollector
    {
        public static ICommand GetCommandSelectUser()
        {
            var res = new Command((x) =>
            {
                if (x is User user)
                {
                    App.Current?.MainPage?.DisplayAlert(
                        "Selected",
                        $"{user.FirstName} {user.LastName}, rank {user.Rank}",
                        "OK");
                }
            });
            return res;
        }

        public static ICommand GetCommandLongSelectUser()
        {
            var res = new Command(async (x) =>
            {
                if (x is User user)
                {
                    const string cancel = "Cancel";

                    string[] items = Enum.GetValues<Ranks>()
                        .Select(x => x.ToString())
                        .ToArray();

                    string? res = await App.Current?.MainPage?.DisplayActionSheet(
                        $"Select rank for\n{user.FirstName} {user.LastName}",
                        cancel,
                        null!,
                        items);
                    if (res == null || res == cancel)
                        return;

                    var newRank = Enum.Parse<Ranks>(res);
                    user.Rank = newRank;
                }
            });
            return res;
        }
    }
}
