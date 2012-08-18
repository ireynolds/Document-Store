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
        public static Storyboard SwoopSelected(int millis, UIElement target = null)
        {
            var swoop = new Storyboard();
            swoop.Children.Add(TranslateY(0, 80, millis, new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 3 }, target));
            swoop.Children.Add(TranslateX(0, 350, millis, new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 4 }, target));
            return swoop;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="millis"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DoubleAnimation TranslateY(double from, double to, double millis, UIElement target = null)
        {
            return TranslateY(from, to, millis, new LinearEase(), target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="millis"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DoubleAnimation RotateY(double from, double to, double millis, UIElement target = null)
        {
            return RotateY(from, to, millis, new LinearEase(), target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="millis"></param>
        /// <param name="easingFunction"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DoubleAnimation RotateY(double from, double to, double millis, IEasingFunction easingFunction, UIElement target = null)
        {
            return GetGenericAnimation(from, to, millis, easingFunction, target).SetTargetProperty("(UIElement.Projection).(PlaneProjection.RotationY)");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="millis"></param>
        /// <param name="easingFunction"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DoubleAnimation TranslateY(double from, double to, double millis, IEasingFunction easingFunction, UIElement target = null)
        {
            return GetGenericAnimation(from, to, millis, easingFunction, target).SetTargetProperty("(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="millis"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DoubleAnimation TranslateX(double from, double to, double millis, UIElement target = null)
        {
            return TranslateX(from, to, millis, new LinearEase(), target);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="millis"></param>
        /// <param name="easingFunction"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DoubleAnimation TranslateX(double from, double to, double millis, IEasingFunction easingFunction, UIElement target = null)
        {
            return GetGenericAnimation(from, to, millis, easingFunction, target).SetTargetProperty("(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="millis"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DoubleAnimation ChangeOpacity(double from, double to, double millis, UIElement target = null)
        {
            return ChangeOpacity(from, to, millis, new LinearEase(), target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="millis"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DoubleAnimation FadeIn(double millis, UIElement target = null)
        {
            return ChangeOpacity(0, 1, millis, new LinearEase(), target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="millis"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DoubleAnimation FadeOut(double millis, UIElement target = null)
        {
            return ChangeOpacity(1, 0, millis, new LinearEase(), target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="millis"></param>
        /// <param name="easingFunction"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static DoubleAnimation ChangeOpacity(double from, double to, double millis, IEasingFunction easingFunction, UIElement target = null)
        {
            return GetGenericAnimation(from, to, millis, easingFunction, target).SetTargetProperty("(UIElement.Opacity)");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="millis"></param>
        /// <param name="easingFunction"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static DoubleAnimation GetGenericAnimation(double from, double to, double millis, IEasingFunction easingFunction, UIElement target = null)
        {
            DoubleAnimation d = new DoubleAnimation();
            d.EasingFunction = easingFunction;
            d.Duration = TimeSpan.FromMilliseconds(millis);
            d.From = from;
            d.To = to;
            d.SetTarget(target);
            return d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static DoubleAnimation SetTarget(this DoubleAnimation timeline, UIElement target)
        {
            if (target != null)
                Storyboard.SetTarget(timeline, target);
            return timeline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="dependencyProperty"></param>
        /// <returns></returns>
        private static DoubleAnimation SetTargetProperty(this DoubleAnimation timeline, string dependencyProperty)
        {
            Storyboard.SetTargetProperty(timeline, new PropertyPath(dependencyProperty));
            return timeline;
        }
    }
}
