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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MenuViewModel vm)
            vm.OnAppearing();
    }

    private async void OnGoToFamilyClicked(object sender, EventArgs e)
    {
        if (Application.Current?.MainPage is MainPage mainPage)
        {
            mainPage.SelectTab(0); // Предполагая, что вкладка семьи имеет индекс 2
        }
    }
}