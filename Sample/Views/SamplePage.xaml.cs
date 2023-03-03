using Sample.ViewModels;
using System.Collections.ObjectModel;

namespace Sample.Views;

public partial class SamplePage
{
    public SamplePage()
    {
        InitializeComponent();
        BindingContext = new SampleVm();
    }
}

