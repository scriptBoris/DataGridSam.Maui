using Sample.Core;
using Sample.Models;
using Sample.Views.DynamicItems;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Views;

public partial class DefaultCollectionPage : ContentPage
{
	public DefaultCollectionPage()
	{
		InitializeComponent();
		Items = DataCollector.GenerateUsers(200);
		BindingContext = this;
	}

	public ObservableCollection<User> Items { get; set; }
    public ICommand CommandAddItem => new Command(async () =>
    {
        var page = new AddUserPage();
        await Navigation.PushAsync(page);

        var res = await page.GetResult(Items);
        if (res == null)
            return;

        Items.Insert(res.Index, res.User);
        await Task.Delay(100);
        ScrollTo(res.Index);
    });

    public void ScrollTo(int index)
    {
        collectionView.ScrollTo(index);
    }
}