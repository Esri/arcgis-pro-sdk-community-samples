using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using LivingAtlasOfTheWorld.Models;

namespace LivingAtlasOfTheWorld.Common {
    
    /// <summary>
    /// Converts OnlineUri to string (returns the Query property)
    /// </summary>
    public class OnlineQueryToString : IValueConverter {
        /// <summary>
        /// Returns the query string property or an empty string if the value is null
        /// </summary>
        /// <returns>The query string</returns>
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) {
            if (value == null)
                return "";
            if ((value as OnlineQuery) == null)
                return "";
            OnlineQuery val = value as OnlineQuery;
            return val.FinalUrl;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
            throw new InvalidOperationException("Converter cannot convert back.");
        }

    }

    /// <summary>
    /// Converts OnlineUri to string (returns the Query property)
    /// </summary>
    public class OnlineUriToString : IValueConverter {
        /// <summary>
        /// Returns the query string property or an empty string if the value is null
        /// </summary>
        /// <returns>The query string</returns>
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) {
            if (value == null)
                return "";
            if ((value as OnlineUri) == null)
                return "";
            OnlineUri val = value as OnlineUri;
            return val.Name;
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
