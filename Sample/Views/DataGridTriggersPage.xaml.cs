using Sample.ViewModels;

namespace Sample.Views;

public partial class DataGridTriggersPage : ContentPage
{
	public DataGridTriggersPage()
	{
		InitializeComponent();
		BindingContext = new DataGridTriggersVm();
	}
}