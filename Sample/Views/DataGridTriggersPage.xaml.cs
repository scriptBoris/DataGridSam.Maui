using Sample.Core;
using Sample.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Views;

public partial class DataGridTriggersPage : ContentPage
{
	public DataGridTriggersPage()
	{
		InitializeComponent();
		Items = DataCollector.GenerateUsers(200);
		CommandSelectedRow = CommandCollector.GetCommandSelectUser();
		CommandLongSelectedRow = CommandCollector.GetCommandLongSelectUser();

		BindingContext = this;
	}

	public ObservableCollection<User> Items { get; private set; }
    public ICommand CommandSelectedRow { get; private set; }
    public ICommand CommandLongSelectedRow { get; private set; }
}