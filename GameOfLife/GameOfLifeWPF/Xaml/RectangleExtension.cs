using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace GameOfLifeWPF.Xaml
{
    internal class RectangleExtension
    {
        #region Public Fields

        // Using a DependencyProperty as the backing store for AliveBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AliveBackgroundProperty =
            DependencyProperty.RegisterAttached("AliveBackground", typeof(Brush), typeof(RectangleExtension), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(100, 100, 100))));

        // Using a DependencyProperty as the backing store for DeadBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeadBackgroundProperty =
            DependencyProperty.RegisterAttached("DeadBackground", typeof(Brush), typeof(RectangleExtension), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 255, 0))));

        // Using a DependencyProperty as the backing store for IsAlive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAliveProperty =
            DependencyProperty.RegisterAttached("IsAlive", typeof(bool?), typeof(RectangleExtension), new PropertyMetadata(null));

        #endregion Public Fields

        #region Public Methods

        public static Brush GetAliveBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(AliveBackgroundProperty);
        }

        public static Brush GetDeadBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(DeadBackgroundProperty);
        }

        public static bool? GetIsAlive(DependencyObject obj)
        {
            return (bool?)obj.GetValue(IsAliveProperty);
        }

        public static void SetAliveBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(AliveBackgroundProperty, value);
        }

        public static void SetDeadBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(DeadBackgroundProperty, value);
        }

        public static void SetIsAlive(DependencyObject obj, bool? value)
        {
            obj.SetValue(IsAliveProperty, value);
        }

        #endregion Public Methods
    }
}