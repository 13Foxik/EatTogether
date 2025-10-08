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

        private readonly IMembershipService _membershipService;
        private readonly CurrentUserService _currentUserService;

        public JoinFamilyViewModel(IMembershipService membershipService, CurrentUserService currentUserService)
        {
            _membershipService = membershipService;
            _currentUserService = currentUserService;
        }

        public JoinFamilyViewModel() : this(Application.Current.Handler.MauiContext.Services.GetService<IMembershipService>(), 
                                            Application.Current.Handler.MauiContext.Services.GetService<CurrentUserService>()) { }

        [RelayCommand]
        private async Task GoBack()
        {
            if (Application.Current?.MainPage is MainPage mainPage)
            {
                var currentNavigation = mainPage.CurrentPage as NavigationPage;
                if (currentNavigation != null)
                {
                    await currentNavigation.Navigation.PushAsync(new FamilyPage());
                }
            }
        }

        [RelayCommand]
        private async Task SendJoinRequest()
        {
            try
            {
                await _membershipService.CreateRequest(FamilyId, _currentUserService.GetCurrentUser());

                if (Application.Current?.MainPage is MainPage mainPage)
                {
                    var currentNavigation = mainPage.CurrentPage as NavigationPage;
                    if (currentNavigation != null)
                    {
                        await currentNavigation.Navigation.PushAsync(new FamilyPage());
                    }
                }
            }
            catch(Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
                Console.WriteLine($"Не удалось подать заявку: {ex}");
            }

        }
    }
}
