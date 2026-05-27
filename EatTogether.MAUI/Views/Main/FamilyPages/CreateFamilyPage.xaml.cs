namespace EatTogether.MAUI.Views.Main.FamilyPages;

using EatTogether.MAUI.ViewModels;
public partial class CreateFamilyPage : ContentPage
{
    public CreateFamilyPage() : this(App.Services.GetService<CreateFamilyViewModel>())
    {
    }

    public CreateFamilyPage(CreateFamilyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}