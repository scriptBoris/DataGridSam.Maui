using DataGridSam;
using Sample.Core;
using Sample.Models;
using Sample.Views.DynamicItems;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Views;

public partial class DataGridCustomCellsPage
{
	public DataGridCustomCellsPage()
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
    public ICommand CommandAddItem => new Command(async () =>
    {
        var page = new AddUserPage();
        await Navigation.PushAsync(page);

        var res = await page.GetResult(Items);
        if (res == null)
            return;

        Items.Insert(res.Index, res.User);
        ScrollTo(res.Index);
    });

    private async void OnUserEdited(User user)
    {
        var row = await dataGrid.GetRowAsync(Items.IndexOf(user), TimeSpan.FromSeconds(1));
        if (row != null)
        {
            row.SetRowBackgroundColor(Colors.Orange, 1);
            await row.AnimateBackgroundColorRestore(2000);
        }
    }

    private async void ScrollTo(int index)
    {
        dataGrid.ScrollTo(index, animate: false);
        var row = await dataGrid.GetRowAsync(index, TimeSpan.FromSeconds(10));
        if (row != null)
        {
            row.SetRowBackgroundColor(Colors.Orange, 1);
            await row.AnimateBackgroundColorRestore(2000);
        }
    }
}