using Microsoft.Maui.Controls;

namespace EatTogether.MAUI.Behaviors
{
    public class FocusScaleBehavior : Behavior<Entry>
    {
        private double _originalScale = 1.0;
        private const double FocusScale = 0.95;
        private const uint Duration = 100;

        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);
            entry.Focused += OnEntryFocused;
            entry.Unfocused += OnEntryUnfocused;
            _originalScale = entry.Scale;
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            base.OnDetachingFrom(entry);
            entry.Focused -= OnEntryFocused;
            entry.Unfocused -= OnEntryUnfocused;
        }

        private async void OnEntryFocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                await entry.ScaleTo(FocusScale, Duration, Easing.SinInOut);
            }
        }

        private async void OnEntryUnfocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                await entry.ScaleTo(_originalScale, Duration, Easing.SinInOut);
            }
        }
    }
}
