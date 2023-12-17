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
    public ICommand CommandAddItem => CommandCollector.GetCommandCreateUser(
        () => Items,
        async (res) =>
        {
            Items.Insert(res.Index, res.User);
            await Task.Delay(100);
            ScrollTo(res.Index);
        }
    );

    public void ScrollTo(int index)
    {
        collectionView.ScrollTo(index);
    }
}