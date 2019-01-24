/*

   Copyright 2019 Esri

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CodingContext
{
    [TemplatePart(Name = "PART_List", Type = typeof(ListBox))]
    public class RecordNavigator : Control
    {
        #region - Constructors -
        static RecordNavigator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RecordNavigator), new FrameworkPropertyMetadata(typeof(RecordNavigator)));
            InitializeDependencyProperties();
            InitializeCommands();
        }
        #endregion

        #region - Static Methods -
        private static void InitializeDependencyProperties()
        {
            ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(RecordNavigator), new PropertyMetadata(null));
            IsSynchronizedWithCurrentItemProperty = DependencyProperty.Register("IsSynchronizedWithCurrentItem", typeof(bool), typeof(RecordNavigator));
        }
        private static void InitializeCommands()
        {
            _MoveFirstCommand = new RoutedUICommand("Move First", "MoveFirstCommand", typeof(RecordNavigator));
            CommandManager.RegisterClassCommandBinding(typeof(RecordNavigator), new CommandBinding(_MoveFirstCommand, MoveFirstCommand_Executed, MoveFirstCommand_CanExecute));

            _MovePreviousCommand = new RoutedUICommand("Move Previous", "MovePreviousCommand", typeof(RecordNavigator));
            CommandManager.RegisterClassCommandBinding(typeof(RecordNavigator), new CommandBinding(_MovePreviousCommand, MovePreviousCommand_Executed, MovePreviousCommand_CanExecute));

            _MoveLastCommand = new RoutedUICommand("Move Last", "MoveLastCommand", typeof(RecordNavigator));
            CommandManager.RegisterClassCommandBinding(typeof(RecordNavigator), new CommandBinding(_MoveLastCommand, MoveLastCommand_Executed, MoveLastCommand_CanExecute));

            _MoveNextCommand = new RoutedUICommand("Move Next", "MoveNextCommand", typeof(RecordNavigator));
            CommandManager.RegisterClassCommandBinding(typeof(RecordNavigator), new CommandBinding(_MoveNextCommand, MoveNextCommand_Executed, MoveNextCommand_CanExecute));
        }
        #endregion

        #region - Dependency Properties -
        public static DependencyProperty ItemsSourceProperty;
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static DependencyProperty IsSynchronizedWithCurrentItemProperty;
        public bool IsSynchronizedWithCurrentItem
        {
            get { return (bool)GetValue(IsSynchronizedWithCurrentItemProperty); }
            set { SetValue(IsSynchronizedWithCurrentItemProperty, value); }
        }
        #endregion

        #region - Routed Commands -
        private static RoutedUICommand _MoveFirstCommand;
        public static RoutedUICommand MoveFirstCommand
        {
            get { return _MoveFirstCommand; }
        }

        public static RoutedUICommand _MoveLastCommand;
        public static RoutedUICommand MoveLastCommand
        {
            get { return _MoveLastCommand; }
        }

        public static RoutedUICommand _MovePreviousCommand;
        public static RoutedUICommand MovePreviousCommand
        {
            get { return _MovePreviousCommand; }
        }

        public static RoutedUICommand _MoveNextCommand;
        public static RoutedUICommand MoveNextCommand
        {
            get { return _MoveNextCommand; }
        }
        #endregion

        #region - Routed Command Callbacks -
        private static void MoveFirstCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RecordNavigator navigator = sender as RecordNavigator;
            if (navigator != null && navigator._lstItems != null && navigator._lstItems.Items.Count > 0)
            {
                navigator._lstItems.Items.MoveCurrentToFirst();
            }
        }
        private static void MoveFirstCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RecordNavigator navigator = sender as RecordNavigator;
            if (navigator != null && navigator._lstItems != null && navigator._lstItems.Items.Count > 0)
            {
                e.CanExecute = navigator._lstItems.Items.CurrentPosition > 0;
            }
        }
        private static void MovePreviousCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RecordNavigator navigator = sender as RecordNavigator;
            if (navigator != null && navigator._lstItems != null && navigator._lstItems.Items.Count > 0)
            {
                navigator._lstItems.Items.MoveCurrentToPrevious();
            }
        }
        private static void MovePreviousCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RecordNavigator navigator = sender as RecordNavigator;
            if (navigator != null && navigator._lstItems != null && navigator._lstItems.Items.Count > 0)
            {
                e.CanExecute = navigator._lstItems.Items.CurrentPosition > 0;
            }
        }
        private static void MoveLastCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RecordNavigator navigator = sender as RecordNavigator;
            if (navigator != null && navigator._lstItems != null && navigator._lstItems.Items.Count > 0)
            {
                navigator._lstItems.Items.MoveCurrentToLast();
            }
        }
        private static void MoveLastCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RecordNavigator navigator = sender as RecordNavigator;
            if (navigator != null && navigator._lstItems != null && navigator._lstItems.Items.Count > 0)
            {
                e.CanExecute = navigator._lstItems.Items.CurrentPosition < navigator._lstItems.Items.Count - 1;
            }
        }
        private static void MoveNextCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RecordNavigator navigator = sender as RecordNavigator;
            if (navigator != null && navigator._lstItems != null && navigator._lstItems.Items.Count > 0)
            {
                navigator._lstItems.Items.MoveCurrentToNext();
            }
        }
        private static void MoveNextCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RecordNavigator navigator = sender as RecordNavigator;
            if (navigator != null && navigator._lstItems != null && navigator._lstItems.Items.Count > 0)
            {
                e.CanExecute = navigator._lstItems.Items.CurrentPosition < navigator._lstItems.Items.Count - 1;
            }
        }
        #endregion

        #region - Private Fields -
        private ListBox _lstItems;
        #endregion

        #region - Overrides -
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.Template != null)
            {
                _lstItems = this.Template.FindName("PART_List", this) as ListBox;
                if (_lstItems == null)
                {
                    throw new InvalidOperationException("RecordNavigator.Template must contain a named element \"Part_List\" of type ListBox.");
                }
            }
            else
            {
                throw new InvalidOperationException("RecordNavigator.Template cannot be null.");
            }
        }
        #endregion
    }
}
