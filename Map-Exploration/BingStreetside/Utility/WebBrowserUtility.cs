/*

   Copyright 2016 Esri

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
using System.Windows;
using System.Windows.Controls;

namespace BingStreetside.Utility
{
    /// <summary>
    /// Utility dependency property for web browser control used in MVVM
    /// </summary>
    public static class WebBrowserUtility
    {
        private static WebBrowser _webBrowser = null;

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty BindableSourceProperty =
                               DependencyProperty.RegisterAttached("BindableSource", typeof(string),
                               typeof(WebBrowserUtility), new UIPropertyMetadata(null,
                               BindableSourcePropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public static void BindableSourcePropertyChanged(DependencyObject o,
                                                         DependencyPropertyChangedEventArgs e)
        {
            _webBrowser = (WebBrowser)o;
            _webBrowser.NavigateToString((string)e.NewValue);
            // Object used for communication from JS -> WPF
            _webBrowser.ObjectForScripting = new HtmlInterop();
            // for testing with an html source file:
            //webBrowser.Navigate(new Uri(@"file:///C:/Temp/test.html"));
        }

        /// <summary>
        /// Invoke a Javascript function from c# using invoke script
        /// </summary>
        /// <param name="jsFunction"></param>
        /// <param name="jsParams"></param>
        public static void InvokeScript (string jsFunction, Object [] jsParams)
        {
            _webBrowser?.InvokeScript(jsFunction, jsParams);
        }
    }
}