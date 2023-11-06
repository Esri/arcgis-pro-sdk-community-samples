/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ConfigWithMap.UI;

namespace ConfigWithMap
{
  internal class ConfigurationManagerWithMap : ConfigurationManager
  {

    #region Override App Name and Icon

    /// <summary>
    /// Replaces the default ArcGIS Pro application name
    /// </summary>
    protected override string ApplicationName => "ConfigWithMap";

    /// <summary>
    /// Replaces the ArcGIS Pro Main window icon.
    /// </summary>
    protected override ImageSource Icon => new BitmapImage(new Uri(@"pack://application:,,,/ConfigWithMap;component/Images/favicon.ico"));

    #endregion Override App Name and Icon

    #region Override Startup Page

    private StartPageViewModel _startupPageViewModel;
    /// <summary>
    /// Called before ArcGIS Pro starts up. Replaces the default Pro start-up page (Optional)
    /// </summary>
    /// <returns> Implemented UserControl with start-up page functionality. 
    /// Return null if a custom start-up page is not needed. Default ArcGIS Pro start-up page will be displayed.</returns>
    protected override FrameworkElement OnShowStartPage()
    {
      if (_startupPageViewModel == null)
      {
        _startupPageViewModel = new StartPageViewModel();
      }
      var page = new StartPage { DataContext = _startupPageViewModel };
      return page;
    }

    #endregion

    #region Override Initialization to get Portal Information

    protected override void OnApplicationInitializing(CancelEventArgs cancelEventArgs)
    {

      // get the login username from Portal
      var portal = ArcGISPortalManager.Current.GetActivePortal();
      if (portal != null)
      {
        try
        {
          if (portal.SignIn().success)
            ConfigWithMapModule.UserName = portal.GetSignOnUsername();
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.ToString());
        }
      }
      base.OnApplicationInitializing(cancelEventArgs);
    }

    #endregion

    #region Override DAML Database

    protected override void OnUpdateDatabase(XDocument database)
    {
      try
      {
        if (database?.Root != null)
        {
          var nsp = database.Root.Name.Namespace;
          var tabElements = from seg in database.Root.Descendants(nsp + "tab") select seg;
          var elements = new HashSet<XElement>();
          foreach (var tabElement in tabElements)
          {
            if (tabElement.Parent == null
                || tabElement.Parent.Name.LocalName.StartsWith("backstage"))
              continue;
            var id = tabElement.Attribute("id");
            if (id == null) continue;

            if (!id.Value.StartsWith("Acme") && !id.Value.StartsWith("ConfigWithMap"))
              elements.Add(tabElement);
            else
            {
              Debug.WriteLine($@"Keep: {id}");
            }
          }
          foreach (var element in elements)
          {
            element.Remove();
          }
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine($@"Error in update database: {ex}");
      }
    }

    #endregion

    #region Override Splash screen
    /// <summary>
    /// Called while ArcGIS Pro starts up. Replaces the default Pro splash screen. (Optional)
    /// </summary>
    /// <returns>Implemented Window with splash screen functionality. 
    /// Return null if a custom splash screen is not needed. Default ArcGIS Pro splash screen will be displayed.</returns>
    protected override Window OnShowSplashScreen()
    {
      return new SplashScreen();
    }
    #endregion

    #region Override About page
    /// <summary>
    /// Customized UserControl is displayed in ArcGIS Pro About property page. Allows to add information about this specific managed configuration.
    /// </summary>
    /// <returns>Implemented UserControl with about box information. 
    /// Return null if a custom about box is not needed. Default ArcGIS Pro About box will be displayed.</returns>
    protected override FrameworkElement OnShowAboutPage()
    {
      return new AboutPage();
    }
    #endregion
  }
}
