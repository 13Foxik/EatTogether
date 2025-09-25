namespace EatTogether.MAUI.Views.Main;

public partial class MainPage : TabbedPage
{
    public MainPage()
    {
        InitializeComponent();
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetIsSwipePagingEnabled(this, true);
        CurrentPage = Children[1];
    }

    public void SetSwipeEnabled(bool enabled)
    {
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetIsSwipePagingEnabled(this, enabled);

        // Можно добавить визуальную индикацию (опционально)
        if (enabled)
        {
            // Свайп между страницами разрешен
        }
        else
        {
            // Свайп между страницами заблокирован
        }
    }
}