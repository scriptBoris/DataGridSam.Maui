using DataGridSam;
using Sample.Core;
using Sample.Models;
using Sample.Views.DynamicItems;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Views;

public partial class DataGridDynamicItemsPage : ContentPage
{
	public DataGridDynamicItemsPage()
	{
		InitializeComponent();
        CommandSelectedRow = CommandCollector.GetCommandSelectUser();
        CommandLongSelectedRow = CommandCollector.GetCommandLongSelectUser(OnUserEdited);
        Items = DataCollector.GenerateUsers(200);

        BindingContext = this;
    }

    public ObservableCollection<User> Items { get; private set; }
    public ICommand CommandSelectedRow { get; private set; }
    public ICommand CommandLongSelectedRow { get; private set; }
    public ICommand CommandAddItem => CommandCollector.GetCommandCreateUser(
        () => Items,
        (res) =>
        {
            Items.Insert(res.Index, res.User);
            ScrollTo(res.Index);
        }
    );

    private async void OnUserEdited(User user)
    {
        var row = await dataGird.GetRowAsync(Items.IndexOf(user), TimeSpan.FromSeconds(1));
        if (row != null)
        {
            row.SetRowBackgroundColor(Colors.Orange, 1);
            await row.AnimateBackgroundColorRestore(2000);
        }
    }

    private async void ScrollTo(int index)
    {
        dataGird.ScrollTo(index);
        var row = await dataGird.GetRowAsync(index, TimeSpan.FromSeconds(10));
        if (row != null)
        {
            row.SetRowBackgroundColor(Colors.Orange, 1);
            await row.AnimateBackgroundColorRestore(2000);
        }
    }
}