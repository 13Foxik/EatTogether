using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Main.FamilyPages;

public partial class EditFamilyPage : ContentPage
{
    public EditFamilyPage(EditFamilyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
