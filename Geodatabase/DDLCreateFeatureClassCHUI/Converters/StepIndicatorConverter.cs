/*

   Copyright 2026 Esri

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
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DDLCreateFeatureClassCHUI.Converters
{
    public class StepIndicatorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return Brushes.LightGray;
            if (values[0] is int currentStepIndex && values[1] is System.Collections.IEnumerable steps)
            {
                int index = -1;
                int i = 0;
                foreach (var item in steps)
                {
                    if (item == values[2])
                    {
                        index = i;
                        break;
                    }
                    i++;
                }
                return index == currentStepIndex
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D5B96"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0C4DE"));
            }
            return Brushes.LightGray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}