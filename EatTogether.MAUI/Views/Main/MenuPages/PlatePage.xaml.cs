using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Main.MenuPages;

public partial class PlatePage : ContentPage
{
    private readonly PlateViewModel _viewModel;

    public PlatePage(PlateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Вызываем загрузку данных при появлении страницы
        if (_viewModel != null)
        {
            await _viewModel.OnPageAppearingAsync();
        }
    }
}