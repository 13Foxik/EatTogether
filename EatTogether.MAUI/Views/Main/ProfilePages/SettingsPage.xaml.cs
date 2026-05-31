using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Main.ProfilePages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
