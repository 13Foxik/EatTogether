using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Views.Main;


namespace EatTogether.MAUI.ViewModels
{
    public partial class JoinFamilyViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _familyId;

        [ObservableProperty]
        private string _joinMessage;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private bool _isRequestSent;

        private readonly IMembershipService _membershipService;
        private readonly CurrentUserService _currentUserService;

        public JoinFamilyViewModel(IMembershipService membershipService, CurrentUserService currentUserService)
        {
            _membershipService = membershipService;
            _currentUserService = currentUserService;
        }

        [RelayCommand]
        private async Task GoBack()
        {
            if (Application.Current?.MainPage is MainPage mainPage)
            {
                var currentNavigation = mainPage.CurrentPage as NavigationPage;
                if (currentNavigation != null)
                {
                    await currentNavigation.Navigation.PopAsync();
                }
            }
        }

        [RelayCommand]
        private async Task SendJoinRequest()
        {
            HasError = false;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FamilyId))
            {
                HasError = true;
                ErrorMessage = "Введите ID семьи";
                return;
            }

            try
            {
                // Передаём сообщение пользователя вместе с заявкой
                await _membershipService.CreateRequest(FamilyId, _currentUserService.GetCurrentUser(), JoinMessage);
                IsRequestSent = true;

                await Task.Delay(1500);

                if (Application.Current?.MainPage is MainPage mainPage)
                {
                    var currentNavigation = mainPage.CurrentPage as NavigationPage;
                    if (currentNavigation != null)
                    {
                        await currentNavigation.Navigation.PopAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
                Console.WriteLine($"Не удалось подать заявку: {ex}");
            }
        }
    }
}
