using Sample.Core;
using Sample.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Views;

public partial class DataGridDynamicColumnsPage : ContentPage
{
    public DataGridDynamicColumnsPage()
    {
        InitializeComponent();
        Items = DataCollector.GenerateUsers(200);
        BindingContext = this;
    }

    public ObservableCollection<User> Items { get; private set; }

    public ICommand CommandAddColumnBirthday => new Command(async () =>
    {
        string? res = await DisplayPromptAsync("", "Please, input INDEX for new column", 
            keyboard: Keyboard.Numeric, 
            initialValue: dataGrid.Columns.Count.ToString());
        if (res == null)
            return;

        if (!int.TryParse(res, out int id))
        {
            await DisplayAlert("Error", "Fail parse input text", "OK");
            return;
        }

        var col = new DataGridSam.DataGridColumn
        {
            Title = "Birthday",
            PropertyName = "BirthDate",
            Width = new GridLength(100),
            CellHorizontalTextAlignment = TextAlignment.End,
        };

        if (id >= 0 && id <= dataGrid.Columns.Count)
        {
            dataGrid.Columns.Insert(id, col);
        }
        else
        {
            dataGrid.Columns.Add(col);
        }
    });

    public ICommand CommandRemoveColumn => new Command(async () =>
    {
        string? res = await DisplayPromptAsync("", "Please, input INDEX for delete column", keyboard: Keyboard.Numeric, initialValue: "0");
        if (res == null)
            return;

        if (!int.TryParse(res, out int id))
        {
            await DisplayAlert("Error", "Fail parse input text", "OK");
            return;
        }

        if (id >= 0 && id <= dataGrid.Columns.Count)
        {
            dataGrid.Columns.RemoveAt(id);
        }
        else
        {
            await DisplayAlert("Error", "The input ID is too large", "OK");
        }
    });
}