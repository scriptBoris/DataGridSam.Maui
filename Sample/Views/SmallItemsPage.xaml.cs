using Sample.Core;
using Sample.Models;
using Sample.Views.DynamicItems;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Views;

public partial class SmallItemsPage : ContentPage
{
    public SmallItemsPage()
    {
        InitializeComponent();
        Items = DataCollector.GenerateUsers(10);
        CommandLongSelectedRow = CommandCollector.GetCommandEditUser(() => Items);
        BindingContext = this;
    }

    public ObservableCollection<User> Items { get; private set; }
    public ICommand CommandSelectedRow { get; private set; } = CommandCollector.GetCommandSelectUser();
    public ICommand CommandLongSelectedRow { get; private set; }

    public ICommand CommandCreateUser => CommandCollector.GetCommandCreateUser(
        () => Items,
        (res) =>
        {
            Items.Insert(res.Index, res.User);
            dataGrid.ScrollTo(res.Index);
        }
    );
}