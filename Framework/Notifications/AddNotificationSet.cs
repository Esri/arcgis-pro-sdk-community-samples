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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Notifications
{
  /// <summary>
  /// Displays notifications in the notification dockpane.  You can add either application or project notifications.  Project
  /// notifications disappear from the notification pane when the project is closed.  You can also set the notification type 
  /// which customizes the image displayed with the notification. 
  /// You can also add detail text, custom context menu or action handlers. 
  /// </summary>
  internal class AddNotificationSet : Button
  {
    protected override void OnClick()
    {
      Module1.ShowNotificationDockpane();

      // an application information notification
      var notification = new NotificationItem("id_app_info", true, 
                    "application information notification ", NotificationType.Information);
      NotificationManager.AddNotification(notification);

      // a project warning notification - disappears when the project is closed
      notification = new NotificationItem("id_project_warning", false, 
                    "project warning notification ", NotificationType.Warning);
      NotificationManager.AddNotification(notification);

      // project error notification with details
      notification = new NotificationItem("id_project_error_details", false, 
                    "project error notification with details", NotificationType.Error, 
                    details: "here are some details to demonstrate what it can look like");
      NotificationManager.AddNotification(notification);

      // project custom notification with custom image
      //   note the following is correct with regards to syntax and how to define the customImage. There is however a 
      //   bug in ArcGIS Pro 2.1 which causes no image to be displayed for custom notification items. 
      notification = new NotificationItem("id_project_custom", false,
                    "project custom notification with custom image", NotificationType.Custom,
                    customImage: "pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/ToastLicensing16.png");
      NotificationManager.AddNotification(notification);

      // project confirmation notification with custom context menu
      myNotificationDelegate myDelegate = new myNotificationDelegate(NotificationCallback);

      notification = new NotificationItem("id_project_confirmation_contextMenu", false, 
                    "project confirmation notification with custom context menu item", NotificationType.Confirmation, 
                    customContextMenuItemText:"Cllck me", customContextMenuItemDelegate:myDelegate, 
                    contextMenuItemArgs: new object[] { "id_project_confirmation_contextMenu" } );
      NotificationManager.AddNotification(notification);


      // Application notification with action and no context menu
      myEmptyDelegate ClearAllDelegate = new myEmptyDelegate(ClearAll);

      notification = new NotificationItem("id_app_action", true,
                    "application notification with action and no context menu", NotificationType.Warning,
                    actionText: "Clear All", action: ClearAllDelegate, actionArgs: null, showContextMenu:false);
      NotificationManager.AddNotification(notification);

      // application notification with action and context menu (uses lambda form rather than delegate form)
      notification = new NotificationItem("id_app_action_lambda", true,
              "application notification with action and context menu", NotificationType.Warning,
              "Clear All", (Action)(() => ClearAll()), null);
      NotificationManager.AddNotification(notification);
    }

    private delegate void myNotificationDelegate(string id);
    private void NotificationCallback(string id)
    {
      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("hello world");
    }

    private delegate void myEmptyDelegate();

    /// <summary>
    /// Clears all the notification (both application and project) from the notification dock pane. 
    /// </summary>
    private void ClearAll()
    {
      NotificationManager.ClearNotification();
    }
  }
}
