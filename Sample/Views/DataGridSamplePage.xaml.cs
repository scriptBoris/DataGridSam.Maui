using Sample.ViewModels;
using System.Collections.ObjectModel;

namespace Sample.Views;

public partial class DataGridSamplePage
{
    public DataGridSamplePage()
    {
        InitializeComponent();
        BindingContext = new DataGridSampleVm();
    }
}

