using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Main;

public partial class FamilyPage : ContentPage
{
    public FamilyPage()
    {
        InitializeComponent();
    }

    private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        // Передаем позицию скролла в ViewModel
        if (BindingContext is FamilyViewModel viewModel)
        {
            viewModel.ScrollPositionChangedCommand.Execute(e.ScrollY);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // При появлении страницы обновляем состояние скролла
        if (BindingContext is FamilyViewModel viewModel)
        {
            viewModel.ScrollPositionChangedCommand.Execute(MainScrollView.ScrollY);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // При уходе со страницы разблокируем свайп
        if (Parent is MainPage mainPage)
        {
            mainPage.SetSwipeEnabled(true);
        }
    }
}