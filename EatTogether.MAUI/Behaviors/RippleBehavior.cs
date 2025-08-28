using Microsoft.Maui.Controls;

namespace EatTogether.MAUI.Behaviors
{
    public class RippleBehavior : Behavior<Button>
    {
        public static readonly BindableProperty RippleColorProperty =
            BindableProperty.Create(nameof(RippleColor), typeof(Color), typeof(RippleBehavior), null);

        public static readonly BindableProperty RippleOpacityProperty =
            BindableProperty.Create(nameof(RippleOpacity), typeof(double), typeof(RippleBehavior), 0.3);

        public static readonly BindableProperty ScaleFactorProperty =
            BindableProperty.Create(nameof(ScaleFactor), typeof(double), typeof(RippleBehavior), 0.95);

        public static readonly BindableProperty AnimationDurationProperty =
            BindableProperty.Create(nameof(AnimationDuration), typeof(uint), typeof(RippleBehavior), 100u);

        public Color RippleColor
        {
            get => (Color)GetValue(RippleColorProperty);
            set => SetValue(RippleColorProperty, value);
        }

        public double RippleOpacity
        {
            get => (double)GetValue(RippleOpacityProperty);
            set => SetValue(RippleOpacityProperty, value);
        }

        public double ScaleFactor
        {
            get => (double)GetValue(ScaleFactorProperty);
            set => SetValue(ScaleFactorProperty, value);
        }

        public uint AnimationDuration
        {
            get => (uint)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        private Color _originalBackgroundColor;
        private Color _originalTextColor;
        private Color _originalBorderColor;
        private bool _shouldChangeColor;

        protected override void OnAttachedTo(Button button)
        {
            base.OnAttachedTo(button);
            button.Pressed += OnButtonPressed;
            button.Released += OnButtonReleased;
        }

        protected override void OnDetachingFrom(Button button)
        {
            base.OnDetachingFrom(button);
            button.Pressed -= OnButtonPressed;
            button.Released -= OnButtonReleased;
        }

        private async void OnButtonPressed(object sender, System.EventArgs e)
        {
            if (sender is Button button)
            {
                // Сохраняем оригинальные цвета
                _originalBackgroundColor = button.BackgroundColor;
                _originalTextColor = button.TextColor;
                _originalBorderColor = button.BorderColor;

                // Проверяем, нужно ли менять цвет (только для белых кнопок)
                _shouldChangeColor = IsWhiteColor(button.BackgroundColor);

                if (_shouldChangeColor)
                {
                    // Определяем цвет ripple для белой кнопки
                    var rippleColor = GetRippleColor(button);
                    button.BackgroundColor = rippleColor.WithAlpha((float)RippleOpacity);
                }

                // Опционально: затемняем текст для контраста
                if (button.TextColor != null)
                {
                    button.TextColor = button.TextColor.WithAlpha(0.8f);
                }

                await button.ScaleTo(ScaleFactor, AnimationDuration, Easing.SinInOut);
            }
        }

        private async void OnButtonReleased(object sender, System.EventArgs e)
        {
            if (sender is Button button)
            {
                // Возвращаем оригинальные цвета
                button.BackgroundColor = _originalBackgroundColor;
                button.TextColor = _originalTextColor;
                button.BorderColor = _originalBorderColor;

                await button.ScaleTo(1.0, AnimationDuration, Easing.SinInOut);
            }
        }

        private Color GetRippleColor(Button button)
        {
            // Если цвет задан явно - используем его
            if (RippleColor != null && !RippleColor.IsDefault())
                return RippleColor;

            // Для белых кнопок используем серый цвет
            return Colors.Gray;
        }

        private bool IsWhiteColor(Color color)
        {
            if (color == null || color.IsDefault())
                return false;

            // Проверяем, является ли цвет белым или очень светлым
            return color.Red > 0.9f && color.Green > 0.9f && color.Blue > 0.9f;
        }
    }
}