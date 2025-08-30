using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Auth;

public partial class SignInPage : ContentPage
{
	public SignInPage(SignInViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}