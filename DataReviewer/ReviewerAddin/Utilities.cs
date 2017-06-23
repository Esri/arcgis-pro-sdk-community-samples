//Copyright 2014 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.using System;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReviewerProSDKSamples
{
    /// <summary>
    /// Utility class
    /// Contains helper methods
    /// </summary>
    static class Utilities
    {
        /// <summary>
        /// This method returns the current item for use as a contextItem
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>current item for use as a contextItem</returns>
        public static IEnumerable<T> GetContext<T>() where T : class
        {
            var items = FrameworkApplication.ContextMenuDataContext as IEnumerable<T>;
            if (items != null)
                return items;
            var singleItem = FrameworkApplication.ContextMenuDataContext as T;
            if (singleItem != null)
                return singleItem.Yield();
            else
                return Enumerable.Empty<T>();
        }

    /// <summary>
    /// Wraps this object instance into an IEnumerable&lt;T&gt;, consisting of a single item.
    /// </summary>
    /// <typeparam name="T"> Type of the object. </typeparam>
    /// <param name="item"> The instance that will be wrapped. </param>
    /// <returns> An IEnumerable&lt;T&gt; consisting of a single item. </returns>
    public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
