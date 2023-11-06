//   Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Notifications
{
  /// <summary>
  /// Displays a standard toast notification in the application.  You can customize the title, message and image. 
  /// </summary>
  /// <remarks>
  /// A toast notification is a transient message to the user that contains relevant, time-sensitive  
  /// information and provides quick access to related content in an application. Notifications appear  
  /// in the top right hand corner of the display and last for a few seconds unless the mouse pointer  
  /// is over top them. Up to four notifications can appear at the same time. If more than four notifications  
  /// are sent, each new notification bumps off the oldest one in the queue. 
  /// </remarks>
  internal class ToastNotification : Button
  {
    static int messageNo = 0;

    protected override void OnClick()
    {
      FrameworkApplication.AddNotification(new Notification()
      {
        Title = "Notification Title",
        Message = $@"Notification # {++messageNo}",
        ImageSource = System.Windows.Application.Current.Resources["Success_Toast48"] as ImageSource
      });
    }
  }

  /// <summary>
  /// Displays a custom toast notification in the application.  The custom toast notification provides an example of overriding the
  /// OnClick action providing a acustom action. 
  /// </summary>
  internal class ToastNotificationWithFeedback : Button
  {
    static int messageNo = 0;

    protected override void OnClick()
    {
      FrameworkApplication.AddNotification(new NotificationWithOnClick()
      {
        Title = "With OnClick feedback",
        Message = $@"OnClick Notification # {++messageNo}",
        ImageSource = System.Windows.Application.Current.Resources["Warning_Toast48"] as ImageSource
      });
    }
  }

  /// <summary>
  /// A custom notification. Provides an example of overriding the OnClick action providing a acustom action. 
  /// </summary>
  internal class NotificationWithOnClick : Notification
  {
    protected override void OnClick()
    {
      FrameworkApplication.RemoveNotification(this);

      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This message was just removed.");
    }
  }
}
