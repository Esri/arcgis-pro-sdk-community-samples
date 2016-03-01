using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivingAtlasOfTheWorld.Common {
    /// <summary>
    /// Class represents extensions to the System.string class
    /// </summary>
    public static class StringExtensions {
        /// <summary>
        /// Returns whether the string is empty
        /// </summary>
        /// <param name="theString"></param>
        /// <returns>True if the string is empty</returns>
        public static bool IsEmpty(this string theString) {
            return (theString == null || theString.Length == 0);
        }
    }
}
