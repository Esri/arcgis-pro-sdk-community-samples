/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ProStartPageConfig.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace ProStartPageConfig
{
  internal class ConfigurationManager1 : ConfigurationManager
  {


    /// <summary>
    /// Replaces the default ArcGIS Pro title bar text
    /// </summary>
    protected override string TitleBarText
    {
      get { return "ProStartPageConfig"; }
    }

    /// <summary>
    /// Replaces the ArcGIS Pro Main window icon.
    /// </summary>
    protected override ImageSource Icon
    {
      get
      {
        return new BitmapImage(new Uri(@"pack://application:,,,/ProStartPageConfig;component/Images/favicon.ico"));
      }
    }

    #region Override Startup Page


    /// <summary>
    /// Called before ArcGIS Pro starts up. Replaces the default Pro start-up page (Optional)
    /// </summary>
    /// <returns> Implemented UserControl with start-up page functionality. 
    /// Return null if a custom start-up page is not needed. Default ArcGIS Pro start-up page will be displayed.</returns>
    protected override System.Windows.FrameworkElement OnShowStartPage()
    {
      var page = new StartPage();
      page.DataContext = Module1.Current.ProStartPageViewModel;
      return page;
    }

    #endregion

    #region Override Splash screen
    /// <summary>
    /// Called while ArcGIS Pro starts up. Replaces the default Pro splash screen. (Optional)
    /// </summary>
    /// <returns>Implemented Window with splash screen functionality. 
    /// Return null if a custom splash screen is not needed. Default ArcGIS Pro splash screen will be displayed.</returns>
    protected override System.Windows.Window OnShowSplashScreen()
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
    protected override System.Windows.FrameworkElement OnShowAboutPage()
    {
      return new AboutPage();
    }
    #endregion
  }
}
