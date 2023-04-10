using Sample.ViewModels;

namespace Sample.Views;

public partial class DataGridColorizedColumnsAndTriggersPage : ContentPage
{
	public DataGridColorizedColumnsAndTriggersPage()
	{
		InitializeComponent();
		BindingContext = new DataGridColorizedColumnsAndTriggersVm();
    }
}