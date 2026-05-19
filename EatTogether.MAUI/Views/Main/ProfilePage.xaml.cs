using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Main;

public partial class ProfilePage : ContentPage
{
    public ProfilePage() : this(App.Services.GetService<ProfileViewModel>())
    {
    }

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnAvatarTapped(object sender, TappedEventArgs e)
    {
        if (AvatarBorder == null) return;
        // Анимация: пружинный эффект при нажатии
        await AvatarBorder.ScaleTo(0.88, 80, Easing.CubicIn);
        await AvatarBorder.ScaleTo(1.08, 100, Easing.CubicOut);
        await AvatarBorder.ScaleTo(1.0, 80, Easing.CubicInOut);
    }
}
