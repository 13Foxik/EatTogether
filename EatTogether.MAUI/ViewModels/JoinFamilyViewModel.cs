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
        private bool _hasError;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isRequestSent;

        private readonly IMembershipService _membershipService;
        private readonly CurrentUserService _currentUserService;

        public JoinFamilyViewModel(IMembershipService membershipService, CurrentUserService currentUserService)
        {
            _membershipService = membershipService;
            _currentUserService = currentUserService;
        }

        public JoinFamilyViewModel() : this(
            Application.Current.Handler.MauiContext.Services.GetService<IMembershipService>(),
            Application.Current.Handler.MauiContext.Services.GetService<CurrentUserService>()) { }

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
            IsRequestSent = false;

            if (string.IsNullOrWhiteSpace(FamilyId))
            {
                HasError = true;
                ErrorMessage = "Введите ID семьи";
                return;
            }

            try
            {
                var user = _currentUserService.GetCurrentUser();
                await _membershipService.CreateRequest(FamilyId, user);
                IsRequestSent = true;
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
