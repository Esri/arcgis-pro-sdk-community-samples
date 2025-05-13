/*

   Copyright 2025 Esri

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
using ArcGIS.Desktop.Framework.Controls;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GPToolInspector
{
  public static class WebViewBrowserUtility
  {
    // "HtmlString" attached property for a WebView
    public static readonly DependencyProperty HtmlStringProperty =
        DependencyProperty.RegisterAttached("HtmlString", typeof(string), typeof(WebViewBrowserUtility), 
          new PropertyMetadata("", BindableSourcePropertyChanged));
    public static string GetHtmlString(DependencyObject obj)
    {
      return obj.GetValue(HtmlStringProperty).ToString();
    }

    public static void SetHtmlString(DependencyObject obj, string value)
    {
      obj.SetValue(HtmlStringProperty, value);
    }

    private static async void BindableSourcePropertyChanged(DependencyObject d, 
      DependencyPropertyChangedEventArgs e)
    {
      WebView2 wv = d as WebView2;
      await wv.EnsureCoreWebView2Async();
      if (wv != null)
      {
        wv.NavigateToString(e.NewValue.ToString());
      }
    }
  }
}
