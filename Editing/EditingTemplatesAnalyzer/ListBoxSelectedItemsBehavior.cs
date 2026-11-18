/*

   Copyright 2026 Esri

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

using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace EditingTemplatesAnalyzer
{
	public static class ListBoxSelectedItemsBehavior
	{
		public static readonly DependencyProperty BindableSelectedItemsProperty =
				DependencyProperty.RegisterAttached("BindableSelectedItems", typeof(IList), typeof(ListBoxSelectedItemsBehavior),
						new PropertyMetadata(null, OnBindableSelectedItemsChanged));

		public static IList GetBindableSelectedItems(DependencyObject obj) => (IList)obj.GetValue(BindableSelectedItemsProperty);
		public static void SetBindableSelectedItems(DependencyObject obj, IList value) => obj.SetValue(BindableSelectedItemsProperty, value);

		private static void OnBindableSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is not ListBox listBox)
				return;

			listBox.SelectionChanged -= ListBox_SelectionChanged;

			if (e.NewValue is IList newList)
			{
				newList.Clear();
				foreach (var item in listBox.SelectedItems)
					newList.Add(item);

				listBox.SelectionChanged += ListBox_SelectionChanged;
			}
		}

		private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sender is not ListBox listBox)
				return;

			var boundList = GetBindableSelectedItems(listBox);
			if (boundList == null)
				return;

			boundList.Clear();
			foreach (var item in listBox.SelectedItems)
				boundList.Add(item);
		}
	}
}
