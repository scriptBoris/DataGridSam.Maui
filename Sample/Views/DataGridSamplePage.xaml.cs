using Sample.Core;
using Sample.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Views;

public partial class DataGridSamplePage
{
    public DataGridSamplePage()
    {
        InitializeComponent();
        CommandSelectedRow = CommandCollector.GetCommandSelectUser();
        CommandLongSelectedRow = CommandCollector.GetCommandLongSelectUser();
        Items = DataCollector.GenerateUsers(200);

        BindingContext = this;
    }

    public ObservableCollection<User> Items { get; private set; }
    public ICommand CommandSelectedRow { get; private set; }    
    public ICommand CommandLongSelectedRow { get; private set; }
}

