/*

   Copyright 2023 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ProStartPageConfig.Helpers
{

  /// <summary>
  /// Captures and forwards MouseWheel events so that a nested ListBox does not
  /// prevent an outer scrollable control from scrolling.
  /// </summary>  
  public sealed class PropagateMouseWheelBehavior : Behavior<UIElement>
  {
    protected override void OnAttached()
    {
      base.OnAttached();
      AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
    }

    protected override void OnDetaching()
    {
      AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
      base.OnDetaching();
    }

    void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      e.Handled = true;

      var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
      e2.RoutedEvent = UIElement.MouseWheelEvent;

      AssociatedObject.RaiseEvent(e2);
    }
  }
}
