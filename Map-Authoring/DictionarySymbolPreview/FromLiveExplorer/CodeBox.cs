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
/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2015 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;
using Xceed.Wpf.Toolkit.LiveExplorer.Core;
using ThicknessConverter = Xceed.Wpf.DataGrid.Converters.ThicknessConverter;

namespace Xceed.Wpf.Toolkit.LiveExplorer {
    public abstract class CodeBox : Xceed.Wpf.Toolkit.RichTextBox, INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected CodeBox() {
            this.IsReadOnly = true;
            this.FontFamily = new FontFamily("Courier New");
            this.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
            this.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
            this.Document.PageWidth = 2500;
        }

        /// <summary>
        /// This is not the original implementation of CodeSource
        /// </summary>
        /// <remarks>Changed to be a dependency property with both setter and getter</remarks>
        public static readonly DependencyProperty CodeSourceProperty = 
            DependencyProperty.Register("CodeSource",typeof(string),typeof(CodeBox),
                new FrameworkPropertyMetadata(string.Empty,
                    new PropertyChangedCallback(CodeSourcePropertyChanged)));

        private static void CodeSourcePropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e) {
            CodeBox _this = sender as CodeBox;
            if (e.NewValue == null) {
                _this.Text = "";
            }
            else {
                _this.Text = (string)e.NewValue;
            }
            _this.OnPropertyChanged("Text");
        }

        public string CodeSource
        {
            get
            {
                return (string) GetValue(CodeSourceProperty);
            }
            set
            {
                SetValue(CodeSourceProperty, value);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propName = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private string GetDataFromResource(string uriString) {
            Uri uri = new Uri(uriString, UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(uri);

            StreamReader reader = new StreamReader(info.Stream);
            string data = reader.ReadToEnd();
            reader.Close();

            return data;
        }

    }

    public class XamlBox : CodeBox {
        public XamlBox() { this.TextFormatter = new Core.XamlFormatter(); }
    }

    public class CSharpBox : CodeBox {
        public CSharpBox() { this.TextFormatter = new Core.CSharpFormatter(); }
    }
}
