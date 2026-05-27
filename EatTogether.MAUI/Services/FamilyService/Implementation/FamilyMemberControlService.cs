using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.FamilyService.Implementation
{
    public class FamilyMemberControlService : IFamilyMemberControlService
    {
        private readonly ICloudStoreService _cloudStoreService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICurrentFamilyService _currentFamilyService;

        public FamilyMemberControlService(
            ICloudStoreService cloudStoreService,
            CurrentUserService currentUserService,
            ICurrentFamilyService currentFamilyService)
        {
            _cloudStoreService = cloudStoreService;
            _currentUserService = currentUserService;
            _currentFamilyService = currentFamilyService;
        }

        public async Task<bool> CanPromote(FamilyMember targetMember)
        {
            try
            {
                var currentUser = _currentUserService.GetCurrentUser();
                var currentFamily = _currentFamilyService.GetCurrentFamily();

                if (currentUser == null || currentFamily == null)
                    return false;

                // Находим текущего пользователя в списке участников
                var currentUserMember = currentFamily.Members.FirstOrDefault(m => m.UserId == currentUser.Uid);
                if (currentUserMember == null)
                    return false;

                // 1. Пользователь должен быть админом или главой
                bool isAdminOrOwner = currentUserMember.Role == FamilyRole.Admin ||
                                       currentUserMember.Role == FamilyRole.Owner;
                if (!isAdminOrOwner)
                    return false;

                // 2. Целевой участник не должен быть админом или главой
                bool targetIsNotAdminOrOwner = targetMember.Role != FamilyRole.Admin &&
                                                targetMember.Role != FamilyRole.Owner;
                if (!targetIsNotAdminOrOwner)
                    return false;

                // 3. Целевой участник не должен быть самим пользователем
                bool isNotSelf = targetMember.UserId != currentUser.Uid;
                if (!isNotSelf)
                    return false;

                // 4. Проверяем, что есть куда повышать (не превышает Editor)
                bool canBePromoted = targetMember.Role < FamilyRole.Editor;
                if (!canBePromoted)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке прав повышения: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CanDemote(FamilyMember targetMember)
        {
            try
            {
                var currentUser = _currentUserService.GetCurrentUser();
                var currentFamily = _currentFamilyService.GetCurrentFamily();

                if (currentUser == null || currentFamily == null)
                    return false;

                // Находим текущего пользователя в списке участников
                var currentUserMember = currentFamily.Members.FirstOrDefault(m => m.UserId == currentUser.Uid);
                if (currentUserMember == null)
                    return false;

                // 1. Пользователь должен быть админом или главой
                bool isAdminOrOwner = currentUserMember.Role == FamilyRole.Admin ||
                                       currentUserMember.Role == FamilyRole.Owner;
                if (!isAdminOrOwner)
                    return false;

                // 2. У участника роль не Member
                bool isNotMember = targetMember.Role != FamilyRole.Member;
                if (!isNotMember)
                    return false;

                // 3. Админа понизить может только глава
                if (targetMember.Role == FamilyRole.Admin && currentUserMember.Role != FamilyRole.Owner)
                    return false;

                // 4. Участник это не сам пользователь
                bool isNotSelf = targetMember.UserId != currentUser.Uid;
                if (!isNotSelf)
                    return false;

                // 5. Владельца нельзя понизить
                bool isNotOwner = targetMember.Role != FamilyRole.Owner;
                if (!isNotOwner)
                    return false;

                // 6. Проверяем, что есть куда понижать (не ниже Member)
                bool canBeDemoted = targetMember.Role > FamilyRole.Member;
                if (!canBeDemoted)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке прав понижения: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CanKick(FamilyMember targetMember)
        {
            try
            {
                var currentUser = _currentUserService.GetCurrentUser();
                var currentFamily = _currentFamilyService.GetCurrentFamily();

                if (currentUser == null || currentFamily == null)
                    return false;

                // Находим текущего пользователя в списке участников
                var currentUserMember = currentFamily.Members.FirstOrDefault(m => m.UserId == currentUser.Uid);
                if (currentUserMember == null)
                    return false;

                // 1. Пользователь должен быть админом или главой
                bool isAdminOrOwner = currentUserMember.Role == FamilyRole.Admin ||
                                       currentUserMember.Role == FamilyRole.Owner;
                if (!isAdminOrOwner)
                    return false;

                // 2. Исключить админа может только глава
                if (targetMember.Role == FamilyRole.Admin && currentUserMember.Role != FamilyRole.Owner)
                    return false;

                // 3. Участник это не сам пользователь
                bool isNotSelf = targetMember.UserId != currentUser.Uid;
                if (!isNotSelf)
                    return false;

                // 4. Главу исключить нельзя
                bool isNotOwner = targetMember.Role != FamilyRole.Owner;
                if (!isNotOwner)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке прав исключения: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Promote(string userId, string familyId)
        {
            try
            {
                var currentFamily = _currentFamilyService.GetCurrentFamily();
                if (currentFamily == null)
                    return false;

                // Находим участника для повышения
                var targetMember = currentFamily.Members.FirstOrDefault(m => m.UserId == userId);
                if (targetMember == null)
                    return false;

                // Проверяем права
                if (!await CanPromote(targetMember))
                    return false;

                // Выполняем повышение
                await _cloudStoreService.PermissionUpToDB(userId, familyId);

                // Обновляем локальные данные
                var updatedMember = currentFamily.Members.FirstOrDefault(m => m.UserId == userId);
                if (updatedMember != null && updatedMember.Role < FamilyRole.Admin)
                {
                    updatedMember.Role = updatedMember.Role + 1;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при повышении участника: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Demote(string userId, string familyId)
        {
            try
            {
                var currentFamily = _currentFamilyService.GetCurrentFamily();
                if (currentFamily == null)
                    return false;

                // Находим участника для понижения
                var targetMember = currentFamily.Members.FirstOrDefault(m => m.UserId == userId);
                if (targetMember == null)
                    return false;

                // Проверяем права
                if (!await CanDemote(targetMember))
                    return false;

                // Выполняем понижение
                await _cloudStoreService.PermissionDownToDB(userId, familyId);

                // Обновляем локальные данные
                var updatedMember = currentFamily.Members.FirstOrDefault(m => m.UserId == userId);
                if (updatedMember != null && updatedMember.Role > FamilyRole.Member)
                {
                    updatedMember.Role = updatedMember.Role - 1;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при понижении участника: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Kick(string userId, string familyId)
        {
            try
            {
                var currentFamily = _currentFamilyService.GetCurrentFamily();
                if (currentFamily == null)
                    return false;

                // Находим участника для исключения
                var targetMember = currentFamily.Members.FirstOrDefault(m => m.UserId == userId);
                if (targetMember == null)
                    return false;

                // Проверяем права
                if (!await CanKick(targetMember))
                    return false;

                // Выполняем исключение
                var result = await _cloudStoreService.KickMemberFromDB(userId, familyId);

                if (result)
                {
                    // Удаляем из локальных данных
                    currentFamily.Members.RemoveAll(m => m.UserId == userId);
                    currentFamily.CountUsers = currentFamily.Members.Count;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при исключении участника: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LeaveFamily(string userId, string familyId)
        {
            try
            {
                return await _cloudStoreService.LeaveFamilyFromDB(userId, familyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выходе из семьи: {ex.Message}");
                throw; // Пробрасываем текст ошибки в ViewModel
            }
        }

        public string GetRoleText(FamilyRole role)
        {
            return role switch
            {
                FamilyRole.Owner => "Глава",
                FamilyRole.Admin => "Админ",
                FamilyRole.Editor => "Редактор",
                FamilyRole.Member => "Участник",
                _ => "Участник"
            };
        }

        public Color GetRoleColor(FamilyRole role)
        {
            return role switch
            {
                FamilyRole.Owner => Color.FromArgb("#FF6B00"), // Оранжевый
                FamilyRole.Admin => Color.FromArgb("#4CAF50"), // Зеленый
                FamilyRole.Editor => Color.FromArgb("#2196F3"), // Синий
                FamilyRole.Member => Color.FromArgb("#9E9E9E"), // Серый
                _ => Color.FromArgb("#9E9E9E")
            };
        }
    }
}