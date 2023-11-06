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
using ArcGIS.Desktop.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ProStartPageConfig.Helpers
{
  internal class ProImageProvider
  {
    private ResourceDictionary _resXaml;

    #region Singleton
    private static readonly ProImageProvider _instance = new ProImageProvider();

    //Private constructor is required
    private ProImageProvider()
    {
      //new ResourceDictionary() { Source = new Uri("pack://application:,,,/ArcGIS.Desktop.Resources;component/Images.xaml") };
      //new ResourceDictionary() { Source = new Uri("pack://application:,,,/ArcGIS.Desktop.Resources;component/ImagesDark.xaml") };

      if (FrameworkApplication.ApplicationTheme == ApplicationTheme.Default)
        _resXaml = new ResourceDictionary() { Source = new Uri("pack://application:,,,/ArcGIS.Desktop.Resources;component/XamlImages.xaml") };
      else
        _resXaml = new ResourceDictionary() { Source = new Uri("pack://application:,,,/ArcGIS.Desktop.Resources;component/DarkXamlImages.xaml") };
    }

    static ProImageProvider()
    {
    }

    /// <summary>
    /// Gets the singleton instance of ProImageProvider
    /// </summary>
    public static ProImageProvider Instance => _instance;
    #endregion

    /// <summary>
    /// Get the ImageSource associated with the given resource key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ImageSource this[string key]
    {
      get
      {
        if (!_resXaml.Contains(key))
          return null;
        return _resXaml[key] as ImageSource;
      }
    }
  }
}
