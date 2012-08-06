using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using NotepadTheNextVersion.Models;

namespace NotepadTheNextVersion.Utilities
{
    public static class AnimationUtils
    {

        public static DoubleAnimation TranslateY(double from, double to, int millis)
        {
            return TranslateY(from, to, millis, new LinearEase());
        }

        public static DoubleAnimation TranslateY(double from, double to, int millis, IEasingFunction easingFunction)
        {
            DoubleAnimation d = GetGenericAnimation(from, to, millis, easingFunction);
            Storyboard.SetTargetProperty(d, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            return d;
        }

        public static DoubleAnimation TranslateX(double from, double to, int millis)
        {
            return TranslateX(from, to, millis, new LinearEase());
        }

        public static DoubleAnimation TranslateX(double from, double to, int millis, IEasingFunction easingFunction)
        {
            DoubleAnimation d = GetGenericAnimation(from, to, millis, easingFunction);
            Storyboard.SetTargetProperty(d, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            return d;
        }

        public static DoubleAnimation ChangeOpacity(double from, double to, int millis)
        {
            return ChangeOpacity(from, to, millis, new LinearEase());
        }

        public static DoubleAnimation FadeIn(int millis)
        {
            return ChangeOpacity(0, 1, millis, new LinearEase());
        }

        public static DoubleAnimation FadeOut(int millis)
        {
            return ChangeOpacity(1, 0, millis, new LinearEase());
        }

        private static DoubleAnimation ChangeOpacity(double from, double to, int millis, IEasingFunction easingFunction)
        {
            DoubleAnimation d = GetGenericAnimation(from, to, millis, easingFunction);
            Storyboard.SetTargetProperty(d, new PropertyPath("(UIElement.Opacity)"));
            return d;
        }

        //public static DoubleAnimation TranslateYOnNavigateIn()
        //{
        //    return TranslateY(250, 350, 0, new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 4 });
        //}

        //public static DoubleAnimation ChangeOpacityOnNavigateIn()
        //{
        //    return ChangeOpacity(0, 250, 1, new ExponentialEase() { EasingMode = EasingMode.EaseOut });
        //}

        private static DoubleAnimation GetGenericAnimation(double from, double to, int millis, IEasingFunction easingFunction)
        {
            DoubleAnimation d = new DoubleAnimation();
            d.EasingFunction = easingFunction;
            d.Duration = TimeSpan.FromMilliseconds(millis);
            d.From = from;
            d.To = to;
            return d;
        }
    }
}
