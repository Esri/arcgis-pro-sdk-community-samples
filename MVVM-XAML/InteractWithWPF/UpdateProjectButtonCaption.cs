using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace InteractWithWPF
{
  internal class UpdateProjectButtonCaption : Button
  {
    protected override void OnClick()
    {
      try
      {
        // change the caption of the 'Application' button in the main window: appButton
        string newCaption = "New Caption";
        var mainWindow = FrameworkApplication.Current.MainWindow;
        if (mainWindow != null)
        {
          var appButton = mainWindow.FindName("appButton");
          if (appButton != null && appButton is FrameworkElement frameworkElement)
          {
            System.Diagnostics.Trace.WriteLine($@"{appButton.GetType()}");
            PropertyInfo propertyInfo = appButton.GetType().GetProperty("Content");
            propertyInfo.SetValue(appButton, Convert.ChangeType(newCaption, propertyInfo.PropertyType), null);
          }
        }
        //var theButton = mainWindow.FindName("UpdateProjectButton") as Button;
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Control type: {mainWindow.GetType().ToString()}", "'Project' WPF control", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
      }
      catch (Exception ex)
      {
        // Handle any exceptions that occur during the update process
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"An error occurred: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
      }
    }
  }

}
