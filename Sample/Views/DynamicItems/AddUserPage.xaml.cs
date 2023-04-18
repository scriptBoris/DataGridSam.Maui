using Sample.Models;
using System.Windows.Input;

namespace Sample.Views.DynamicItems;

public partial class AddUserPage : ContentPage
{
	private User? userResult;
    private TaskCompletionSource<User?> result { get; set; } = new();

    public AddUserPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	public string? FirstName { get; set; } = "Adam";
	public string? LastName { get; set; } = "Wood";
	public DateTime Birthday { get; set; } = new DateTime(DateTime.Now.Year - 18, 1, 1);
	public Ranks Rank { get; set; } = Ranks.OfficePlankton;
	public int Index { get; set; } = -1;

	public ICommand CommandSelectRank => new Command(async () =>
	{
        const string cancel = "Cancel";

        string[] items = Enum.GetValues<Ranks>()
            .Select(x => x.ToString())
            .ToArray();

        string? res = await DisplayActionSheet(
            $"Select rank",
            cancel,
            null!,
            items);
        if (res == null || res == cancel)
            return;

        var newRank = Enum.Parse<Ranks>(res);
		Rank = newRank;
    });

    public ICommand CommandCreate => new Command(() =>
	{
		if (string.IsNullOrWhiteSpace(FirstName))
			return;

		if (string.IsNullOrWhiteSpace(LastName))
			return;

        userResult = new User
		{
			FirstName = FirstName,
			LastName = LastName,
			BirthDate = Birthday,
			Rank = Rank,
		};
		Navigation.PopAsync();
	});

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
		result.TrySetResult(userResult);
    }

	public async Task<AddUserResult?> GetResult(IList<User> source)
	{
		var res = await result.Task;
		if (res == null)
			return null;


        int index = Index;
        if (index < 0 || index > source.Count - 1)
            index = source.Count;

		return new AddUserResult
		{
			Index = index,
			User = res,
		};
    }

	public class AddUserResult
	{
		public required int Index { get; init; }
		public required User User { get; init; }
	}

    private void Button_Clicked(object sender, EventArgs e)
    {

    }
}