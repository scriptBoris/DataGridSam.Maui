using Sample.Models;
using Sample.Views.DynamicItems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public static ICommand GetCommandLongSelectUser(Action<User>? onEdited = null)
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
                    onEdited?.Invoke(user);
                }
            });
            return res;
        }

        public static ICommand GetCommandCreateUser(Func<IList> source, Action<AddUserPage.AddUserResult> onCreated = null)
        {
            var res = new Command(async () =>
            {
                var page = new AddUserPage(source());
                await App.Current!.MainPage!.Navigation.PushAsync(page);

                var tsc = new TaskCompletionSource<bool>();
                page.Unloaded += (o, e) =>
                {
                    tsc.TrySetResult(true);
                };

                await tsc.Task;

                var res = page.CreatedResult;
                if (res == null)
                    return;

                await Task.Delay(400);
                onCreated?.Invoke(res);
            });
            return res;
        }

        public static ICommand GetCommandEditUser(Func<IList> source)
        {
            var res = new Command(async (x) =>
            {
                if (x is User user)
                {
                    var list = source();
                    var editPage = new AddUserPage(list, user);
                    await App.Current!.MainPage!.Navigation.PushAsync(editPage);

                    var tsc = new TaskCompletionSource<bool>();
                    editPage.Unloaded += (o, e) =>
                    {
                        tsc.TrySetResult(true);
                    };

                    await tsc.Task;

                    if (editPage.CreatedResult != null)
                    {
                        user.FirstName = editPage.CreatedResult.User.FirstName;
                        user.LastName = editPage.CreatedResult.User.LastName;
                        user.Rank = editPage.CreatedResult.User.Rank;
                        user.BirthDate = editPage.CreatedResult.User.BirthDate;

                        int oldIndex = list.IndexOf(user);
                        int newIndex = editPage.CreatedResult.Index;
                        if (oldIndex != newIndex && list is ObservableCollection<User> obs)
                        {
                            obs.Move(oldIndex, newIndex);
                        }
                    }
                }
            });
            return res;
        }
    }
}
