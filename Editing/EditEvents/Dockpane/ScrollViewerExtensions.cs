/*

   Copyright 2019 Esri

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
using System.Windows.Controls;

namespace EditEventsSample.Dockpane
{
  //From stack overflow
  //https://stackoverflow.com/questions/2984803/how-to-automatically-scroll-scrollviewer-only-if-the-user-did-not-change-scrol
  public class ScrollViewerExtensions
  {
    public static readonly DependencyProperty AlwaysScrollToEndProperty = 
      DependencyProperty.RegisterAttached("AlwaysScrollToEnd", typeof(bool), 
        typeof(ScrollViewerExtensions), new PropertyMetadata(false, AlwaysScrollToEndChanged));
    private static bool _autoScroll;

    private static void AlwaysScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      ScrollViewer scroll = sender as ScrollViewer;
      if (scroll != null)
      {
        bool alwaysScrollToEnd = (e.NewValue != null) && (bool)e.NewValue;
        if (alwaysScrollToEnd)
        {
          scroll.ScrollToEnd();
          scroll.ScrollChanged += ScrollChanged;
        }
        else { scroll.ScrollChanged -= ScrollChanged; }
      }
      else { throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances."); }
    }

    public static bool GetAlwaysScrollToEnd(ScrollViewer scroll)
    {
      if (scroll == null) { throw new ArgumentNullException("scroll"); }
      return (bool)scroll.GetValue(AlwaysScrollToEndProperty);
    }

    public static void SetAlwaysScrollToEnd(ScrollViewer scroll, bool alwaysScrollToEnd)
    {
      if (scroll == null) { throw new ArgumentNullException("scroll"); }
      scroll.SetValue(AlwaysScrollToEndProperty, alwaysScrollToEnd);
    }

    private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      ScrollViewer scroll = sender as ScrollViewer;
      if (scroll == null) { throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances."); }

      // User scroll event : set or unset autoscroll mode
      if (e.ExtentHeightChange == 0) { _autoScroll = scroll.VerticalOffset == scroll.ScrollableHeight; }

      // Content scroll event : autoscroll eventually
      if (_autoScroll && e.ExtentHeightChange != 0) { scroll.ScrollToVerticalOffset(scroll.ExtentHeight); }
    }
  }
}
