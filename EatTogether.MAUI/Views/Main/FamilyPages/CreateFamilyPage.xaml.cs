namespace EatTogether.MAUI.Views.Main.FamilyPages;

using EatTogether.MAUI.ViewModels;
public partial class CreateFamilyPage : ContentPage
{
	public CreateFamilyPage(CreateFamilyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}