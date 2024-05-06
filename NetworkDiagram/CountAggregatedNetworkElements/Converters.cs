/*

   Copyright 2024 Esri

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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace CountAggregatedNetworkElements
{
  /// <summary>
  /// Convert a boolean to Visibility Collapse
  /// </summary>
  /// <remarks>Use in Pane</remarks>
  public class ConvertBoolToVisibilityCollapse : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (Visibility)value == Visibility.Visible;
    }
  }

  /// <summary>
  /// Convert a boolean to Visibility Hidden
  /// </summary>
  /// <remarks>Use in Pane</remarks>
  public class ConvertBoolToVisibilityHidden : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (bool)value ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (Visibility)value == Visibility.Visible;
    }
  }

}
