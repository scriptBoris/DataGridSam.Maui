using Sample.ViewModels;

namespace Sample.Views;

public partial class DefaultCollectionPage : ContentPage
{
	public DefaultCollectionPage()
	{
		InitializeComponent();
		BindingContext = new DefaultCollectionVm();
	}
}