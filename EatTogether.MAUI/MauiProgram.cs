using CommunityToolkit.Maui;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.ViewModels;
using EatTogether.MAUI.Views.Auth;
using EatTogether.MAUI.Views.Main;
using EatTogether.MAUI.Views.Main.FamilyPages;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.FamilyService.Implementation;
using Microsoft.Extensions.Logging;

namespace EatTogether.MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FontAwesome.ttf", "FontAwesome");
                });

            //Services
            builder.Services.AddSingleton<CurrentUserService>();
            builder.Services.AddSingleton<IAuthProviderFactory, AuthProviderFactory>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IEmailAuth, EmailAuthService>();
            builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
            builder.Services.AddSingleton<ICloudStoreService, FirestoreService>();
            builder.Services.AddSingleton<ICurrentFamilyService, CurrentFamilyService>();
            builder.Services.AddSingleton<IFamilyService, FamilyService>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IMembershipService, MembershipService>();

            //ViewModels
            builder.Services.AddTransient<SignInViewModel>();
            builder.Services.AddTransient<SignUpViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<FamilyViewModel>();
            builder.Services.AddTransient<CreateFamilyViewModel>();
            builder.Services.AddTransient<JoinFamilyViewModel>();

            //Pages
            builder.Services.AddTransient<SignInPage>();
            builder.Services.AddTransient<SignUpPage>();
            builder.Services.AddTransient<MenuPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<FamilyPage>();
            builder.Services.AddTransient<CreateFamilyPage>();
            builder.Services.AddTransient<JoinFamilyPage>();

            builder.Services.AddSingleton<AppShell>();


#if DEBUG
            builder.Logging.AddDebug();
#endif
            

            return builder.Build();
        }
    }
}
