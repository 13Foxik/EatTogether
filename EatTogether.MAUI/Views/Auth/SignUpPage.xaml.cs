using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Auth;

public partial class SignUpPage : ContentPage
{
    public SignUpPage(SignUpViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

    }
    protected override bool OnBackButtonPressed()
    {
        // �������� ������� �������� �� ViewModel
        if (BindingContext is SignUpViewModel viewModel)
        {
            viewModel.GoToSignInCommand.Execute(null);
            return true; // �������� ����������� ���������
        }

        return base.OnBackButtonPressed();
    }


}