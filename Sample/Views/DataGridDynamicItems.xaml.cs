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
        CommandLongSelectedRow = CommandCollector.GetCommandLongSelectUser();
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

    private async void ScrollTo(int index)
    {
        dataGird.ScrollTo(index);
        var row = await dataGird.GetRowAsync(index);
        if (row != null)
        {
            row.SetRowBackgroundColor(Colors.Orange, 1);
            await row.AnimateBackgroundColorRestore(2000);
        }
    }
}