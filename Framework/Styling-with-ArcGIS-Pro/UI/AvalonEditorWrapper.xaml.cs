/*

   Copyright 2017 Esri

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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace ControlStyles.UI
{/// <summary>
 /// Interaction logic for AvalonEditorWrapper.xaml
 /// </summary>
    public partial class AvalonEditorWrapper : UserControl
    {
        public AvalonEditorWrapper()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Dependency property to be used for binding to the Avalon Editor
        /// </summary>
        public static DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AvalonEditorWrapper),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(TextPropertyChanged)));

        /// <summary>
        /// Gets and sets the Avalon Editor text content
        /// </summary>
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var _this = sender as AvalonEditorWrapper;
            _this.AvalonXML.Text = FormatXml((string)args.NewValue);
        }

        private static string FormatXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return "";
            var doc = new XmlDocument();
            var sb = new StringBuilder();
            try
            {
                doc.LoadXml(xml);
                var xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
                doc.Save(XmlWriter.Create(sb, xmlWriterSettings));
            }
            catch (System.Xml.XmlException xmle)
            {
                System.Diagnostics.Debug.WriteLine("FormatXml Exception: {0}", xmle.ToString());
                sb.Append(xml);
            }
            return sb.ToString();
        }
    }
}
