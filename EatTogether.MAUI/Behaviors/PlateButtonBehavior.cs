using System;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Models;
using Microsoft.Maui.Controls;

namespace EatTogether.MAUI.Behaviors
{
    public class PlateButtonBehavior : Behavior<Frame>
    {
        public static readonly BindableProperty IsInPlateProperty =
            BindableProperty.Create(nameof(IsInPlate), typeof(bool), typeof(PlateButtonBehavior), false,
                propertyChanged: OnIsInPlateChanged);

        public static readonly BindableProperty AddCommandProperty =
            BindableProperty.Create(nameof(AddCommand), typeof(IAsyncRelayCommand<Dish>), typeof(PlateButtonBehavior));

        public static readonly BindableProperty RemoveCommandProperty =
            BindableProperty.Create(nameof(RemoveCommand), typeof(IAsyncRelayCommand<Dish>), typeof(PlateButtonBehavior));

        public static readonly BindableProperty DishProperty =
            BindableProperty.Create(nameof(Dish), typeof(Dish), typeof(PlateButtonBehavior));

        public bool IsInPlate
        {
            get => (bool)GetValue(IsInPlateProperty);
            set => SetValue(IsInPlateProperty, value);
        }

        public IAsyncRelayCommand<Dish> AddCommand
        {
            get => (IAsyncRelayCommand<Dish>)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        public IAsyncRelayCommand<Dish> RemoveCommand
        {
            get => (IAsyncRelayCommand<Dish>)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public Dish Dish
        {
            get => (Dish)GetValue(DishProperty);
            set => SetValue(DishProperty, value);
        }

        private TapGestureRecognizer _tapGestureRecognizer;
        private Frame _associatedFrame;

        protected override void OnAttachedTo(Frame bindable)
        {
            base.OnAttachedTo(bindable);
            _associatedFrame = bindable;
            SetupGestureRecognizer();
        }

        protected override void OnDetachingFrom(Frame bindable)
        {
            if (_tapGestureRecognizer != null && _associatedFrame != null)
            {
                _associatedFrame.GestureRecognizers.Remove(_tapGestureRecognizer);
            }
            base.OnDetachingFrom(bindable);
        }

        private void SetupGestureRecognizer()
        {
            if (_associatedFrame == null) return;

            // Удаляем старый распознаватель жестов
            if (_tapGestureRecognizer != null)
            {
                _associatedFrame.GestureRecognizers.Remove(_tapGestureRecognizer);
            }

            // Создаем новый распознаватель жестов
            _tapGestureRecognizer = new TapGestureRecognizer();

            if (IsInPlate)
            {
                _tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, new Binding
                {
                    Source = this,
                    Path = nameof(RemoveCommand)
                });
            }
            else
            {
                _tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, new Binding
                {
                    Source = this,
                    Path = nameof(AddCommand)
                });
            }

            _tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandParameterProperty, new Binding
            {
                Source = this,
                Path = nameof(Dish)
            });

            _associatedFrame.GestureRecognizers.Add(_tapGestureRecognizer);
        }

        private static void OnIsInPlateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PlateButtonBehavior behavior && behavior._associatedFrame != null)
            {
                behavior.SetupGestureRecognizer();
            }
        }
    }
}