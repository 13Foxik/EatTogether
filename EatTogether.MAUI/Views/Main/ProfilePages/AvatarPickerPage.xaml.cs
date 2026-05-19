using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Main.ProfilePages;

public partial class AvatarPickerPage : ContentPage
{
    public AvatarPickerPage() : this(App.Services.GetService<AvatarPickerViewModel>())
    {
    }

    public AvatarPickerPage(AvatarPickerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
