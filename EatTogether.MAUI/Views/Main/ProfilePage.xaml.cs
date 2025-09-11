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
}