using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Main.FamilyPages;

public partial class JoinFamilyPage : ContentPage
{
    public JoinFamilyPage() : this(App.Services.GetService<JoinFamilyViewModel>())
    {
    }

    public JoinFamilyPage(JoinFamilyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}