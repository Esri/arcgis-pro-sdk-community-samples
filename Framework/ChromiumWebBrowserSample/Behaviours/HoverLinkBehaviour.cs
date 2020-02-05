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
using CefSharp;
using CefSharp.Wpf;
using System.Windows;
using System.Windows.Interactivity;
using System;


namespace ChromiumWebBrowserSample.Behaviours
{
	/// <summary>
	/// From https://github.com/cefsharp/CefSharp.MinimalExample/blob/master/CefSharp.MinimalExample.Wpf/Behaviours/HoverLinkBehaviour.cs
	/// </summary>
	public class HoverLinkBehaviour : Behavior<ChromiumWebBrowser>
	{
		// Using a DependencyProperty as the backing store for HoverLink. This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HoverLinkProperty = DependencyProperty.Register("HoverLink", typeof(string), typeof(HoverLinkBehaviour), new PropertyMetadata(string.Empty));

		public string HoverLink
		{
			get { return (string)GetValue(HoverLinkProperty); }
			set { SetValue(HoverLinkProperty, value); }
		}

		protected override void OnAttached()
		{
			AssociatedObject.StatusMessage += OnStatusMessageChanged;
		}

		protected override void OnDetaching()
		{
			AssociatedObject.StatusMessage -= OnStatusMessageChanged;
		}

		private void OnStatusMessageChanged(object sender, StatusMessageEventArgs e)
		{
			var chromiumWebBrowser = sender as ChromiumWebBrowser;
			chromiumWebBrowser.Dispatcher.BeginInvoke((Action)(() => HoverLink = e.Value));
		}
	}
}
