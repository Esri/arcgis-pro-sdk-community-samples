/*

   Copyright 2017 Esri

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
using System.Windows;
using System.Windows.Data;

namespace ConstructionToolWithOptions
{
  internal class NotBoolToVisibleConverter : IValueConverter
  {
    public NotBoolToVisibleConverter() { }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(Visibility))
        throw new InvalidOperationException("The target must be of type Visibility");

      return (bool)value ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(bool))
        throw new InvalidOperationException("The target must be of type bool");

      if (value == null)
        return true;

      Visibility visiblity = (Visibility)System.Convert.ToInt32(value);

      return visiblity == Visibility.Visible ? false : true;
    }
  }


  internal class BoolToVisibleConverter : IValueConverter
  {
    public BoolToVisibleConverter() { }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(Visibility))
        throw new InvalidOperationException("The target must be of type " + nameof(Visibility));

      return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(bool))
        throw new InvalidOperationException("The target must be of type bool");

      if (value == null)
        return true;

      Visibility visiblity = (Visibility)System.Convert.ToInt32(value);

      return visiblity == Visibility.Visible ? true : false;
    }
  }
}
