/*

   Copyright 2020 Esri

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TableControlsDockpane
{
    public static class Extensions
    {

        /// <summary>
        /// Call: var foundElement = visualRootElement.GetChildOfType&lt;ComboBox&gt;();
        /// </summary>
        /// <typeparam name="T">type you specify in GetChildOfType&lt;type&gt;</typeparam>
        /// <param name="depObj">root node from where to search from</param>
        /// <returns>null or found dependency object of the type T</returns>
        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                System.Diagnostics.Debug.WriteLine(child.GetType());
                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
