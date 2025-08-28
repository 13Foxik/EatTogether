using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views;

public partial class SignInPage : ContentPage
{
	public SignInPage()
	{
		InitializeComponent();
        BindingContext = new SignInViewModel();
    }

}