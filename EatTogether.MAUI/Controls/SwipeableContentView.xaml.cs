using System.Windows.Input;

namespace EatTogether.MAUI.Controls;

public partial class SwipeableContentView : ContentView
{
    private double _startX, _startY;
    private bool _isSwiping;

    public static readonly BindableProperty SwipeLeftCommandProperty =
        BindableProperty.Create(nameof(SwipeLeftCommand), typeof(ICommand), typeof(SwipeableContentView));

    public static readonly BindableProperty SwipeRightCommandProperty =
        BindableProperty.Create(nameof(SwipeRightCommand), typeof(ICommand), typeof(SwipeableContentView));

    public static readonly BindableProperty SwipeStartedCommandProperty =
        BindableProperty.Create(nameof(SwipeStartedCommand), typeof(ICommand), typeof(SwipeableContentView));

    public ICommand SwipeLeftCommand
    {
        get => (ICommand)GetValue(SwipeLeftCommandProperty);
        set => SetValue(SwipeLeftCommandProperty, value);
    }

    public ICommand SwipeRightCommand
    {
        get => (ICommand)GetValue(SwipeRightCommandProperty);
        set => SetValue(SwipeRightCommandProperty, value);
    }

    public ICommand SwipeStartedCommand
    {
        get => (ICommand)GetValue(SwipeStartedCommandProperty);
        set => SetValue(SwipeStartedCommandProperty, value);
    }

    public SwipeableContentView()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler != null)
        {
            // Добавляем обработчики жестов
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGesture);
        }
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startX = 0;
                _startY = e.TotalY;
                _isSwiping = true;

                // Отправляем позицию начала свайпа
                SwipeStartedCommand?.Execute(e.TotalY);
                break;

            case GestureStatus.Running:
                if (_isSwiping)
                {
                    var deltaX = e.TotalX - _startX;

                    // Если горизонтальное движение значительное
                    if (Math.Abs(deltaX) > 50)
                    {
                        if (deltaX > 0) // Свайп вправо
                        {
                            SwipeRightCommand?.Execute(null);
                            _isSwiping = false;
                        }
                        else // Свайп влево
                        {
                            SwipeLeftCommand?.Execute(null);
                            _isSwiping = false;
                        }
                    }
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isSwiping = false;
                break;
        }
    }
}