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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Catalog;

namespace ConfigWithStartWizard.UI
{
    public class OnMouseClick
    {
        public static readonly DependencyProperty MouseLeftClickProperty =
            DependencyProperty.RegisterAttached("MouseLeftClick", typeof(ICommand), typeof(OnMouseClick),
            new FrameworkPropertyMetadata(CallBack));

        public static void SetMouseLeftClick(DependencyObject sender, ICommand value)
        {
            sender.SetValue(MouseLeftClickProperty, value);
        }

        public static ICommand GetMouseLeftClick(DependencyObject sender)
        {
            return sender.GetValue(MouseLeftClickProperty) as ICommand;
        }

        public static readonly DependencyProperty MouseEventParameterProperty =
            DependencyProperty.RegisterAttached(
            "MouseEventParameter",
                typeof(object),
                typeof(OnMouseClick),
                new FrameworkPropertyMetadata((object)null, null));


        public static object GetMouseEventParameter(DependencyObject d)
        {
            return d.GetValue(MouseEventParameterProperty);
        }


        public static void SetMouseEventParameter(DependencyObject d, object value)
        {
            d.SetValue(MouseEventParameterProperty, value);
        }

        private static void CallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender != null)
            {
                UIElement element = sender as UIElement;
                if (element != null)
                {
                    if (e.OldValue != null)
                    {
                        element.RemoveHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(Handler));
                    }
                    if (e.NewValue != null)
                    {
                        element.AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(Handler), true);
                    }
                }
            }
        }
        private static void Handler(object sender, EventArgs e)
        {
            UIElement element = sender as UIElement;
            if (sender != null)
            {
                ICommand cmd = element.GetValue(MouseLeftClickProperty) as ICommand;
                if (cmd != null)
                {
                    RoutedCommand routedCmd = cmd as RoutedCommand;
                    object paramenter = element.GetValue(MouseEventParameterProperty);
                    if (paramenter == null)
                    {
                        paramenter = element;
                    }
                    if (routedCmd != null)
                    {
                        if (routedCmd.CanExecute(paramenter, element))
                        {
                            routedCmd.Execute(paramenter, element);
                        }
                    }
                    else
                    {
                        if (cmd.CanExecute(paramenter))
                        {
                            cmd.Execute(paramenter);
                        }
                    }
                }
            }
        }
    }
}
