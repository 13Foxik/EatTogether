using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Main;

public partial class FamilyPage : ContentPage
{
    public FamilyPage() : this(App.Services.GetService<FamilyViewModel>())
    {
    }

    public FamilyPage(FamilyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}