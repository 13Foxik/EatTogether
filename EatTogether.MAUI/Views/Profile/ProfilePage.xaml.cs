using EatTogether.MAUI.ViewModels;

namespace EatTogether.MAUI.Views.Profile;

public partial class ProfilePage : ContentPage
{
	public ProfilePage(ProfileViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}