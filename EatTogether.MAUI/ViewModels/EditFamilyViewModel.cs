using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.ViewModels
{
    public partial class EditFamilyViewModel : ObservableObject
    {
        private readonly IFamilyService _familyService;
        private readonly ICurrentFamilyService _currentFamilyService;
        private readonly ICloudStoreService _cloudStoreService;
        private readonly CurrentUserService _currentUserService;

        [ObservableProperty]
        private string _editName = string.Empty;

        [ObservableProperty]
        private string _editDescription = string.Empty;

        [ObservableProperty]
        private string _familyId = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        // Владелец (Owner) не может покинуть семью через этот экран
        [ObservableProperty]
        private bool _canLeave;

        public EditFamilyViewModel(
            IFamilyService familyService,
            ICurrentFamilyService currentFamilyService,
            ICloudStoreService cloudStoreService,
            CurrentUserService currentUserService)
        {
            _familyService = familyService;
            _currentFamilyService = currentFamilyService;
            _cloudStoreService = cloudStoreService;
            _currentUserService = currentUserService;

            // Определяем, может ли текущий пользователь покинуть семью
            var family = _currentFamilyService.GetCurrentFamily();
            var currentUser = _currentUserService.GetCurrentUser();
            if (family != null && currentUser != null)
            {
                var member = family.Members.FirstOrDefault(m => m.UserId == currentUser.Uid);
                // Owner не может покинуть семью
                CanLeave = member != null && member.Role != FamilyRole.Owner;
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            if (Application.Current?.MainPage is Views.Main.MainPage mainPage &&
                mainPage.CurrentPage is NavigationPage nav)
            {
                await nav.Navigation.PopAsync();
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(EditName))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Название семьи не может быть пустым", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                await _familyService.UpdateFamily(FamilyId, EditName.Trim(), EditDescription?.Trim() ?? "");
                await GoBack();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении семьи: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось сохранить изменения", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LeaveFamily()
        {
            var family = _currentFamilyService.GetCurrentFamily();
            var currentUser = _currentUserService.GetCurrentUser();
            if (family == null || currentUser == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Выход из семьи",
                $"Вы уверены, что хотите покинуть семью \"{family.Name}\"?",
                "Выйти", "Отмена");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                bool result = await _cloudStoreService.LeaveFamilyFromDB(currentUser.Uid, family.Id);

                if (result)
                {
                    // Убираем семью из локального кэша пользователя
                    if (currentUser.UserFamilies != null)
                        currentUser.UserFamilies.Remove(family.Id);

                    _currentFamilyService.ClearFamily();

                    // Возвращаемся на главную (pop всего стека)
                    if (Application.Current?.MainPage is Views.Main.MainPage mainPage &&
                        mainPage.CurrentPage is NavigationPage nav)
                    {
                        await nav.Navigation.PopToRootAsync();
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось покинуть семью. Попробуйте ещё раз.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выходе из семьи: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Произошла ошибка при выходе из семьи.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
