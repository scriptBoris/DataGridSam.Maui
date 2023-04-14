using Sample.Core;
using Sample.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Views;

public partial class DataGridColorizedColumnsAndTriggersPage : ContentPage
{
	public DataGridColorizedColumnsAndTriggersPage()
	{
		InitializeComponent();
		CommandSelectItem = CommandCollector.GetCommandSelectUser();
		CommandLongSelectItem = CommandCollector.GetCommandLongSelectUser();
        Items = DataCollector.GenerateUsers(200);

		BindingContext = this;
    }

	public ObservableCollection<User> Items { get; private set; }
	public ICommand CommandSelectItem { get; private set; }
	public ICommand CommandLongSelectItem { get; private set; }
}