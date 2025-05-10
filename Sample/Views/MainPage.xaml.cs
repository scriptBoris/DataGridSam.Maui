namespace Sample.Views;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}
    
    private void Button_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DataGridSamplePage());
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DefaultCollectionPage());
    }

    private void Button_Clicked_2(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DataGridTriggersPage());
    }

    private void Button_Clicked_3(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DataGridColorizedColumnsPage());
    }

    private void Button_Clicked_4(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DataGridColorizedColumnsAndTriggersPage());
    }

    private void Button_Clicked_5(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DataGridDynamicItemsPage());
    }

    private void Button_Clicked_6(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DataGridCustomCellsPage());
    }

    private void Button_Clicked_7(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DataGridDynamicColumnsPage());
    }

    private void Button_Clicked_8(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SmallItemsPage());
    }

    private void Button_Clicked_9(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DataGridDynamicWidthPage());
    }
}