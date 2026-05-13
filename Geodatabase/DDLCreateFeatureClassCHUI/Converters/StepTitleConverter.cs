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

namespace DDLCreateFeatureClassCHUI.Converters
{
    public class StepTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int step = value is int i ? i : 0;
            return step switch
            {
                0 => "Step 1: Select Target Data Store",
                1 => "Step 2: Define",
                2 => "Step 3: Fields",
                3 => "Step 4: Spatial Reference",
                4 => "Step 5: Tolerance",
                5 => "Step 6: Resolution",
                6 => "Step 7: Storage Configuration",
                _ => ""
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}