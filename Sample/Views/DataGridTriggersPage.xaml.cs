using Sample.Core;
using Sample.Models;
using System.Collections.ObjectModel;

namespace Sample.Views;

public partial class DataGridTriggersPage : ContentPage
{
	public DataGridTriggersPage()
	{
		InitializeComponent();
		Items = DataCollector.GenerateUsers(200);
		BindingContext = this;
	}

	public ObservableCollection<User> Items { get; set; }
}