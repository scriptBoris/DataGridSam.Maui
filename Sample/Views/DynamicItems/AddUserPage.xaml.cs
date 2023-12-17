using Sample.Models;
using System.Collections;
using System.Windows.Input;

namespace Sample.Views.DynamicItems;

public partial class AddUserPage : ContentPage
{
    private readonly IList _context;
    private readonly User? _editUser;

    public AddUserPage(IList context)
    {
        _context = context;

        FirstName = "Adam";
        LastName = "Wood";
        Birthday = new DateTime(DateTime.Now.Year - 18, 1, 1);
        Rank = Ranks.OfficePlankton;
        Index = -1;

        InitializeComponent();
        BindingContext = this;
    }

    public AddUserPage(IList context, User editUser)
    {
        _context = context;
        _editUser = editUser;
        IsEditMode = true;

        FirstName = editUser.FirstName;
        LastName = editUser.LastName;
        Birthday = editUser.BirthDate;
        Rank = editUser.Rank;
        Index = context.IndexOf(editUser);

        InitializeComponent();
        BindingContext = this;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Birthday { get; set; }
    public Ranks Rank { get; set; }
    public int Index { get; set; }
    public bool IsEditMode { get; }

    public AddUserResult? CreatedResult { get; private set; }

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

        int index = Index;
        if (index < 0 || index > _context.Count - 1)
            index = _context.Count;

        CreatedResult = new AddUserResult
        {
            Index = index,
            User = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                BirthDate = Birthday,
                Rank = Rank,
            }
        };
        Navigation.PopAsync();
    });

    public class AddUserResult
    {
        public required int Index { get; init; }
        public required User User { get; init; }
    }
}