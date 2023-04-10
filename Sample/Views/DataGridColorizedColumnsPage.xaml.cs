using Sample.ViewModels;

namespace Sample.Views;

public partial class DataGridColorizedColumnsPage : ContentPage
{
	public DataGridColorizedColumnsPage()
	{
		InitializeComponent();
		BindingContext = new DataGridColorizedColumnsVm();
    }
}