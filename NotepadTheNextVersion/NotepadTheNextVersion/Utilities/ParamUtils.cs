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
using System.Collections.Generic;

namespace NotepadTheNextVersion.Utilities
{
    public static class ParamUtils
    {
        public static void CheckForNull(object param, string paramName)
        {
            if (param == null)
                throw new ArgumentException("The parameter " + paramName + " was null.");
        }

        public static void CheckForNullOrWhitespace(string param, string paramName)
        {
            CheckForNull(param, paramName);
            if (param.Trim().Equals(string.Empty))
                throw new ArgumentException("The parameter " + paramName + " was whitespace.");
        }

        public static void CheckForExists(IActionable param, string paramName)
        {
            CheckForNull(param, paramName);
            if (!param.Exists())
                throw new ArgumentException("The parameter " + paramName + " did not exist.");
        }

        // Sets the arguments to prepare for navigation
        public static void SetArguments(params object[] list)
        {
            if ((App.Current as App).Arguments != null)
                throw new InvalidOperationException("Arguments already non-null");

            (App.Current as App).Arguments = list;
        }

        // Retrieves the arguments from the current navigation
        public static IList<object> GetArguments()
        {
            if ((App.Current as App).Arguments == null)
                throw new InvalidOperationException("Arguments null");

            IList<object> Arguments = (App.Current as App).Arguments;
            (App.Current as App).Arguments = null;
            return Arguments;
        }
    }
}
