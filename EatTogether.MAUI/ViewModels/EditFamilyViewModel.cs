using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services.FamilyService.Interfaces;

namespace EatTogether.MAUI.ViewModels
{
    public partial class EditFamilyViewModel : ObservableObject
    {
        private readonly IFamilyService _familyService;
        private readonly ICurrentFamilyService _currentFamilyService;

        [ObservableProperty]
        private string _editName = string.Empty;

        [ObservableProperty]
        private string _editDescription = string.Empty;

        [ObservableProperty]
        private string _familyId = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public EditFamilyViewModel(IFamilyService familyService, ICurrentFamilyService currentFamilyService)
        {
            _familyService = familyService;
            _currentFamilyService = currentFamilyService;
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
    }
}
