using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services
{
    public class UserSerivce : IUserService
    {
        private readonly ICloudStoreService _cloudStoreService;
        public UserSerivce(ICloudStoreService cloudStoreService)
        {
            _cloudStoreService = cloudStoreService;
        }
        public async Task UpdateUser(User user)
        {
            await _cloudStoreService.InsertUserModel(user);
        }
    }
}
