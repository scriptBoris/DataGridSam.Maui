using DataGridSam;
using Microsoft.Maui.Layouts;
using Sample.Core;
using Sample.Models;
using Sample.Views.DynamicItems;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Views;

public partial class DataGridDynamicWidthPage : ContentPage
{
	public DataGridDynamicWidthPage()
	{
		InitializeComponent();
        CommandSelectedRow = CommandCollector.GetCommandSelectUser();
        CommandLongSelectedRow = CommandCollector.GetCommandLongSelectUser(OnUserEdited);
        Items = DataCollector.GenerateUsers(200);

        BindingContext = this;
    }

    public ObservableCollection<User> Items { get; private set; }
    public ICommand CommandSelectedRow { get; private set; }
    public ICommand CommandLongSelectedRow { get; private set; }
    public ICommand CommandAddItem => CommandCollector.GetCommandCreateUser(
        () => Items,
        (res) =>
        {
            Items.Insert(res.Index, res.User);
            ScrollTo(res.Index);
        }
    );

    private async void OnUserEdited(User user)
    {
        var row = await dataGird.GetRowAsync(Items.IndexOf(user), TimeSpan.FromSeconds(1));
        if (row != null)
        {
            row.SetRowBackgroundColor(Colors.Orange, 1);
            await row.AnimateBackgroundColorRestore(2000);
        }
    }

    private async void ScrollTo(int index)
    {
        dataGird.ScrollTo(index);
        var row = await dataGird.GetRowAsync(index, TimeSpan.FromSeconds(10));
        if (row != null)
        {
            row.SetRowBackgroundColor(Colors.Orange, 1);
            await row.AnimateBackgroundColorRestore(2000);
        }
    }
}

public class AdjustableWidthLayout : Layout, ILayoutManager
{
    public static readonly BindableProperty WidthScaleProperty = BindableProperty.Create(
        nameof(WidthScale),
        typeof(double),
        typeof(AdjustableWidthLayout),
        1.0,
        propertyChanged: (b,o,n) =>
        {
            if (b is IView self)
            {
                self.InvalidateMeasure();
            }
        }
    );
    public double WidthScale
    {
        get => (double)GetValue(WidthScaleProperty);
        set => SetValue(WidthScaleProperty, value);
    }

    public Size ArrangeChildren(Rect bounds)
    {
        double w = bounds.Width * WidthScale;
        if (Children.Count == 1)
        {
            var children = Children[0];
            return children.Arrange(new Rect
            {
                X = 0,
                Y = 0,
                Width = w,
                Height = bounds.Height,
            });
        }

        return bounds.Size;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        double w = widthConstraint * WidthScale;
        if (Children.Count == 1)
        {
            var children = Children[0];
            return children.Measure(w, heightConstraint);
        }
        else
        {
            return base.Measure(widthConstraint, heightConstraint);
        }
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }
}