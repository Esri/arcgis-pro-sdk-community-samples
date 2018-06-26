/*

   Copyright 2018 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ConfigWithStartWizard.Common {
    /// <summary>
    ///If the string length &gt; 0 is "true" then true is returned, otherwise false.
    /// </summary>
    /// <remarks>If the string value "true" is passed as the parameter the returned bool is flipped</remarks>
    public class StringToBoolConverter : IValueConverter {
        /// <summary>
        /// Converts OnlineUri query string length to a bool. A query string length &gt; 0 returns true
        /// </summary>
        /// <returns>Pass "true" as the parameter to flip (reverse) the returned bool (i.e. !true or !false)</returns>
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) {

            string content = value == null ? "" : (string)value;
            var enable = content.Length > 0;

            if (parameter != null) {
                if (parameter.ToString().ToLower().CompareTo("true") == 0) {
                    //invert the enabled
                    enable = !enable;
                }
            }

            return enable;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
            throw new InvalidOperationException("Converter cannot convert back.");
        }

    }

    public class ReverseBoolConverter : IValueConverter {

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) {
            return !(bool)value;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
            throw new InvalidOperationException("Converter cannot convert back.");
        }

    }

    /// <summary>
    /// Converts the given bool to a Visibility
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter {

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) {
            if (value == null)
                return Visibility.Hidden;
            bool val = (bool)value;
            return val ? Visibility.Visible : Visibility.Hidden;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
            throw new InvalidOperationException("Converter cannot convert back.");
        }

    }

    //private class ImageUtilities {
    //    BitmapImage BitmapToImageSource(Bitmap bitmap) {
    //        using (MemoryStream memory = new MemoryStream()) {
    //            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
    //            memory.Position = 0;
    //            BitmapImage bitmapimage = new BitmapImage();
    //            bitmapimage.BeginInit();
    //            bitmapimage.StreamSource = memory;
    //            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
    //            bitmapimage.EndInit();

    //            return bitmapimage;
    //        }
    //    }
    //}
}
