namespace EatTogether.MAUI.Views.Main;

public partial class MainPage : TabbedPage
{
    public MainPage()
    {
        InitializeComponent();
        CurrentPage = Children[1];
    }

    private void OnSwiped(object sender, SwipedEventArgs e)
    {
        var currentIndex = Children.IndexOf(CurrentPage);

        switch (e.Direction)
        {
            case SwipeDirection.Left:
                if (currentIndex < Children.Count - 1)
                    CurrentPage = Children[currentIndex + 1];
                break;
            case SwipeDirection.Right:
                if (currentIndex > 0)
                    CurrentPage = Children[currentIndex - 1];
                break;
        }
    }
}
