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
using NotepadTheNextVersion.Utilities;

namespace NotepadTheNextVersion.Models
{
    public class WatermarkedTextBox : TextBox
    {
        private bool _hasUserSetText;
        private bool _hasFocus;
        private string _watermark;

        public bool HasUserSetText
        {
            get
            {
                return _hasUserSetText;
            }
        }

        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        public WatermarkedTextBox(string watermark)
        {
            _hasUserSetText = false;
            _watermark = watermark.ToLower();
            SetWatermark();

            this.GotFocus += new RoutedEventHandler(This_GotFocus);
            this.LostFocus += new RoutedEventHandler(This_LostFocus);
            this.KeyUp += new KeyEventHandler(This_KeyUp);
        }

        public void SetText(string text)
        {
            this.Text = text;
            _hasUserSetText = true;
            this.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void This_KeyUp(object sender, KeyEventArgs e)
        {
            _hasUserSetText = !StringUtils.IsNullOrWhitespace(this.Text);
            if (!_hasUserSetText && !_hasFocus)
                SetWatermark();
        }

        private void This_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!_hasUserSetText)
                SetWatermark();
            _hasFocus = false;
        }

        private void This_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!_hasUserSetText)
                RemoveWatermark();
            _hasFocus = true;
            this.SelectAll();
        }

        private void SetWatermark()
        {
            this.Text = _watermark;
            this.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void RemoveWatermark()
        {
            this.Text = string.Empty;
            this.Foreground = new SolidColorBrush(Colors.Black);
        }
    }
}
