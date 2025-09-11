using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Main;

public partial class MenuPage : ContentPage
{
    public MenuPage() : this(App.Services.GetService<MenuViewModel>())
    {
    }

    public MenuPage(MenuViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}