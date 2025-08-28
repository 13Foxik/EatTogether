using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Views;
using System.Diagnostics;

namespace EatTogether.MAUI.ViewModels
{
    public partial class SignInViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _isPasswordVisible = true;

        [RelayCommand]
        private async Task SignIn()
        {
            
        }
        [RelayCommand]
        private async Task GoToSignUp()
        {
            await Shell.Current.GoToAsync("//SignUpPage");
        }

        [RelayCommand]
        private void TogglePassword()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }
    }
}
