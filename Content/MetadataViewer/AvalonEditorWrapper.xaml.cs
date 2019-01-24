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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace MetadataViewer
{
    /// <summary>
    /// Interaction logic for AvalonEditorWrapper.xaml
    /// </summary>
    public partial class AvalonEditorWrapper : UserControl, INotifyPropertyChanged
    {
        public AvalonEditorWrapper()
        {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }
        /// <summary>
        /// Dependency property to be used for binding to the Avalon Editor
        /// </summary>
        public static DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AvalonEditorWrapper),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(TextPropertyChanged)));
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        protected virtual void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region properties
        private string _validationText = "";
        /// <summary>
        /// Sets the Validation text for the XML
        /// </summary>
        public string ValidationText => _validationText;
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
        #endregion

        #region private methods

        /// <summary>
        /// Validates the XML
        /// </summary>
        private void Validate()
        {

            try
            {
                var document = new XmlDocument { XmlResolver = null };
                document.LoadXml(this.AvalonXML.Text);
                _validationText = "No errors";
            }
            catch (XmlException ex)
            {
                _validationText = string.Format("Error: {0}\r\n", ex.Message);
                DisplayValidationError(ex.Message, ex.LinePosition, ex.LineNumber);
            }
            this.Validation.IsExpanded = true;
            OnPropertyChanged("ValidationText");
        }
        /// <summary>
        /// Highlights\Selects the invalid XML
        /// </summary>
        /// <param name="message"></param>
        /// <param name="linePosition"></param>
        /// <param name="lineNumber"></param>
        private void DisplayValidationError(string message, int linePosition, int lineNumber)
        {
            if (lineNumber >= 1 && lineNumber <= this.AvalonXML.Document.LineCount)
            {
                int index = message.ToLower().IndexOf(" line ");
                int index2 = -1;
                int beginLine = -1;
                int beginPos = -1;
                int offset1 = -1;
                //Example error message:
                //"The 'VerticalExaggeration' start tag on line 24 position 29 does not match the end tag of 'Layer3DProperties'.
                if (index >= 0)
                {
                    index2 = message.Substring(index + 6).ToLower().IndexOf(" position ");
                    string line = message.Substring(index + 6, index2).Replace(",", "").Trim();

                    //now position
                    string remainder = message.Substring(index + 6 + index2 + 10);
                    string position = remainder.Substring(0, remainder.IndexOf(" ")).Replace(",", "").Trim();

                    if (Int32.TryParse(line, out beginLine) && Int32.TryParse(position, out beginPos))
                    {
                        offset1 = this.AvalonXML.Document.GetOffset(new TextLocation(beginLine, beginPos - 1));
                    }
                }
                int offset2 = this.AvalonXML.Document.GetOffset(new TextLocation(lineNumber, linePosition));
                if (offset1 > 0)
                {
                    this.AvalonXML.Select(offset1, (offset2 - offset1));
                }
                else
                {
                    offset1 = this.AvalonXML.Document.GetOffset(new TextLocation(lineNumber, 0));
                    this.AvalonXML.Select(offset2, (offset2 - offset1));
                }
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
        /// <summary>
        /// Checks if the XML is valid
        /// </summary>
        /// <returns></returns>
        private bool IsValidated()
        {
            bool isValidated = true;
            try
            {
                var document = new XmlDocument { XmlResolver = null };
                document.LoadXml(this.AvalonXML.Text);
                _validationText = "No errors";
            }
            catch (XmlException ex)
            {
                isValidated = false;
                _validationText = string.Format("Error: {0}\r\n", ex.Message);
                DisplayValidationError(ex.Message, ex.LinePosition, ex.LineNumber);
            }
            this.Validation.IsExpanded = true;
            OnPropertyChanged("ValidationText");
            return isValidated;
        }
        /// <summary>
        /// Saves the edited xml
        /// </summary>
        private async void SaveXML()
        {
            MetadataViewerViewModel vm = FrameworkApplication.DockPaneManager.Find("MetadataViewer_MetadataViewer") as MetadataViewerViewModel;
            if (vm != null)
            {
                var selectedItem = vm.ItemInformation?.ProPrjItem; //get the selected project item
                var xml = this.AvalonXML.Text;
                bool IsXMLValid = IsValidated();

                if (!IsXMLValid)
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Metadata XML not valid. See the Validation section for more info.");
                await QueuedTask.Run(() =>
                {
                    if (selectedItem.CanEdit() && IsXMLValid) //check if metadata is editable
                        selectedItem?.SetXml(xml);
                });  
            }  
        }

        #endregion

        #region Commands
        ICommand _validateCommand;
        public ICommand ValidateCommand
        {
            get
            {
                return _validateCommand ?? (_validateCommand = new RelayCommand(() => Validate()));
            }
        }

        ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(() => SaveXML()));
            }
        }

        #endregion        
    }
}
