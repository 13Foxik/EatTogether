using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views;

public partial class SignUpPage : ContentPage
{
	public SignUpPage()
	{
		InitializeComponent();

        BindingContext = new SignUpViewModel();

	}
    protected override bool OnBackButtonPressed()
    {
        // Вызываем команду возврата из ViewModel
        if (BindingContext is SignUpViewModel viewModel)
        {
            viewModel.GoToSignInCommand.Execute(null);
            return true; // Отменяем стандартное поведение
        }

        return base.OnBackButtonPressed();
    }

    
}