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
using Microsoft.Live;
using System.Collections.Generic;

namespace NotepadTheNextVersion.Models
{
    public class SkydriveClient
    {
        public SkydriveClient()
        {
            
        }

        public void GetDirectories(EventHandler<LiveOperationCompletedEventArgs> GetCompleted)
        {
            var directoryClient = new LiveConnectClient(App.Session);
            directoryClient.GetCompleted += GetCompleted;
            directoryClient.GetAsync("/me/skydrive/files?filter=folders");
        }
    }
}
