using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ViewAndEditData
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
