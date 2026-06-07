using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Services.MenuService.Interfaces;

namespace EatTogether.MAUI.Services.MenuService.Implementation
{
    public class CurrentPlateService : ICurrentPlateService
    {
        private readonly CurrentUserService _currentUserService;
        private readonly ICurrentFamilyService _currentFamilyService;
        private readonly ICloudStoreService _cloudStoreService;

        // Тарелка должна быть статической или правильно инициализированной
        public Plate CurrentPlate { get; set; }

        public CurrentPlateService(CurrentUserService currentUserService, ICloudStoreService cloudStoreService, ICurrentFamilyService currentFamilyService)
        {
            _currentUserService = currentUserService;
            _cloudStoreService = cloudStoreService;
            _currentFamilyService = currentFamilyService;

            _currentUserService.UserChanged += OnUserChanged;
            _currentFamilyService.FamilyChanged += OnFamilyChanged;

            // Проверяем, не создана ли уже тарелка
            if (CurrentPlate == null)
            {
                CreatePlate();
            }
        }

        private void OnUserChanged(object sender, UserChangedEventArgs e)
        {
            CreatePlate();
        }

        private void OnFamilyChanged(object sender, FamilyChangedEventArgs e)
        {
            CreatePlate();
        }

        public void CreatePlate()
        {
            CurrentPlate = new Plate
            {
                UserId = _currentUserService.GetCurrentUser()?.Uid ?? string.Empty,
                DishesId = new List<string>(),
                FamilyId = _currentFamilyService.GetCurrentFamily()?.Id ?? string.Empty
            };
        }

        public void ClearPlate()
        {
            if (CurrentPlate == null)
            {
                CreatePlate();
                return;
            }

            CurrentPlate.DishesId.Clear();
        }

        public void AddDish(string dishId)
        {
            if (CurrentPlate == null)
            {
                CreatePlate();
            }

            var currentUserId = _currentUserService.GetCurrentUser()?.Uid ?? string.Empty;
            var currentFamilyId = _currentFamilyService.GetCurrentFamily()?.Id ?? string.Empty;

            if (CurrentPlate.UserId != currentUserId || CurrentPlate.FamilyId != currentFamilyId)
            {
                CreatePlate();
            }

            if (!CurrentPlate.DishesId.Contains(dishId))
            {
                CurrentPlate.DishesId.Add(dishId);
            }
        }

        public void RemoveDish(string dishId)
        {
            CurrentPlate?.DishesId.Remove(dishId);
        }

        public int GetDishCount()
        {
            return CurrentPlate?.DishesId?.Count ?? 0;
        }

        public bool HasDish(string dishId)
        {
            return CurrentPlate?.DishesId?.Contains(dishId) == true;
        }

        public List<string> GetDishIds()
        {
            return CurrentPlate?.DishesId ?? new List<string>();
        }

        public async Task WriteToDB()
        {
            if (CurrentPlate == null)
            {
                return;
            }

            var currentUserId = _currentUserService.GetCurrentUser()?.Uid ?? string.Empty;
            var currentFamilyId = _currentFamilyService.GetCurrentFamily()?.Id ?? string.Empty;

            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(currentFamilyId))
            {
                return;
            }

            CurrentPlate.UserId = currentUserId;
            CurrentPlate.FamilyId = currentFamilyId;
            CurrentPlate.CreatedAt = DateTime.UtcNow;
            CurrentPlate.Status = RequestStatus.Pending;
            CurrentPlate.ProcessedAt = null;

            await _cloudStoreService.AddPlateToDB(CurrentPlate);

            foreach (string dishId in CurrentPlate.DishesId)
            {
                DishOnPlate dish = new DishOnPlate(CurrentPlate.Id, dishId);
                await _cloudStoreService.AddDishOnPlateToDB(dish);
            }

            CreatePlate();
        }
    }
}
