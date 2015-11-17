using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CoordinateSystemAddin.Common {
    
    /// <summary>
    /// Returns the opposite or 'Not' of the input - True for False and False for True
    /// </summary>
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
}
