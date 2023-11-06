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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using ArcGIS.Desktop.Internal.Catalog;
using static ArcGIS.Core.Data.NetworkDiagrams.AngleDirectedDiagramLayoutParameters;
using ArcGIS.Desktop.Core.UnitFormats;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Media;

namespace TableViewerTest
{
  /// <summary>
  /// Value Converter for WPF: formats the display format for type of double properties
  /// </summary>
  internal class DoubleStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture)
    {
      if (targetType != typeof(string))
        throw new InvalidOperationException("The type of the binding target property must be of type String");
      var dValue = (double)value;
      if (double.IsNaN(dValue)) return string.Empty;
      return dValue.ToString("F2");
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
    {
      if (targetType != typeof(double))
        throw new InvalidOperationException("The type of the binding target property must be of type double");

      if (string.IsNullOrEmpty(value.ToString()))
        return double.NaN;

      if (double.TryParse(value.ToString(), out double dValue))
        return dValue;

      return double.NaN;
    }
  }

  /// <summary>
  /// Value Converter for WPF: formats the display format for directions getting the data from the direction column
  /// </summary>
  internal class DirectionStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture)
    {
      if (targetType != typeof(string))
        throw new InvalidOperationException("The type of the binding target property must be of type String");
      var text = (string)value;
      if (string.IsNullOrEmpty(text)) return string.Empty;
      var parts = text.Split(',');      
      return parts[0];
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  internal class DiscrepancyAlertBackgroundColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (targetType != typeof(System.Windows.Media.Brush))
        throw new InvalidOperationException("The type of the binding target property must be of type System.Windows.Media.Brush");

      var discrepancyValue = (double)value;
      if (discrepancyValue > Module1.DistanceDiscrepanyAlert)
      {
        return Brushes.Yellow;
      }
      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  internal class DiscrepancyAlertForegroundColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (targetType != typeof(System.Windows.Media.Brush))
        throw new InvalidOperationException("The type of the binding target property must be of type System.Windows.Media.Brush");

      var discrepancyValue = (double)value;
      if (discrepancyValue > Module1.DistanceDiscrepanyAlert)
      {
        return Brushes.OrangeRed;
      }
      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

}
