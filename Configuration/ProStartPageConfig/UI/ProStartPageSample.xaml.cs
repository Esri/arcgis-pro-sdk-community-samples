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
using ArcGIS.Core;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProStartPageConfig.UI
{
  /// <summary>
  /// Interaction logic for StartPage.xaml
  /// </summary>
  public partial class StartPage : UserControl
  {
    public StartPage()
    {
      InitializeComponent();

      if (!IsDesignMode)
        InitResources();
    }

    private bool IsDesignMode
    {
      //http://stackoverflow.com/questions/12917566/detecting-design-mode-using-wpf-in-a-static-method
      get { return DesignerProperties.GetIsInDesignMode(this); }
    }

    private void InitResources()
    {
      var resources = this.Resources;
      resources.BeginInit();

      if (FrameworkApplication.ApplicationTheme == ApplicationTheme.Dark ||
          FrameworkApplication.ApplicationTheme == ApplicationTheme.HighContrast)
      {
        resources.MergedDictionaries.Add(
           new ResourceDictionary()
           {
             Source = new Uri(@"pack://application:,,,/ProStartPageConfig;component\Styles\DarkTheme.xaml")
           });
      }
      else
      {
        resources.MergedDictionaries.Add(
           new ResourceDictionary()
           {
             Source = new Uri(@"pack://application:,,,/ProStartPageConfig;component\Styles\LightTheme.xaml")
           });
      }
      resources.EndInit();
    }
  }
}
