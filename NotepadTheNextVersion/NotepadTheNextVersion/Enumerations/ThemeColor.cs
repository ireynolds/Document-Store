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

namespace NotepadTheNextVersion.Enumerations
{
    // represents the background
    public enum ThemeColor
    {
        light,
        dark,
        phone
    }

    public static class Extensions
    {
        public static Color Color(this ThemeColor color)
        {
            switch (color)
            {
                case ThemeColor.light:
                    return Colors.White;
                case ThemeColor.dark:
                    return Colors.Black;
                case ThemeColor.phone:
                    return (Color)App.Current.Resources["PhoneBackgroundColor"];
                default:
                    throw new Exception("Unrecognized enum type.");
            }
        }

        public static String ToString(this ThemeColor color)
        {
            return color.ToString().ToLower();
        }

        public static SolidColorBrush Brush(this ThemeColor color)
        {
            return new SolidColorBrush(color.Color());
        }
    }
}
