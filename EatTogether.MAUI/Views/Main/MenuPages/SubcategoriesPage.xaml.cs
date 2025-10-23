using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Main.MenuPages;

public partial class SubcategoriesPage : ContentPage
{
    public SubcategoriesPage(SubcategoriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}