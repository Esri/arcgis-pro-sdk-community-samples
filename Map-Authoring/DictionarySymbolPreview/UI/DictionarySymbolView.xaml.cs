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
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Mil2525.Fields;

namespace DictionarySymbolPreview.UI {
    /// <summary>
    /// Interaction logic for DictionarySymbolView.xaml
    /// </summary>
    public partial class DictionarySymbolView : UserControl, INotifyPropertyChanged {

        private SymbolSet _currentSet = null;
        private string _codeSource = "";
        private BitmapImage _img = null;
        private BasicFeatureLayer _selectedLayer = null;
        private long _selectedOid = -1;
        private bool _ignoreEvents = false;

        public DictionarySymbolView() {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
            this.PropertyChanged += DictionarySymbolView_PropertyChanged;
            CurrentSymbolSet = SymbolSet.SymbolSets["Air"];
            this.SymbolSetPropertyGrid.PropertyValueChanged += SymbolSetPropertyGrid_PropertyValueChanged;
            this.SymbolSetPropertyGrid.SelectedObjectChanged += SymbolSetPropertyGrid_SelectedObjectChanged;
            this.SymbolSetPropertyGrid.ShowTitle = true;
        }

        public IReadOnlyList<SymbolSet> SymbolSets => SymbolSet.SymbolSets.Select(k => k.Value).ToList();

        public SymbolSet CurrentSymbolSet {
            get {
                return _currentSet;
            }
            set {
                _currentSet = value;
                OnPropertyChanged();
            }
        }

        public string CodeSource {
            get {
                return _codeSource;
            }
            set {
                _codeSource = value;
                OnPropertyChanged();
            }
        }

        public ImageSource ImageSource {
            get {
                return _img;
            }
            set {
                _img = (BitmapImage)value;
                OnPropertyChanged();
            }
        }

        public Tuple<BasicFeatureLayer, long> SelectedFeature {
            get {
                return new Tuple<BasicFeatureLayer, long>(_selectedLayer, _selectedOid);
            }
            set {
                _selectedLayer = value.Item1;
                _selectedOid = value.Item2;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged([CallerMemberName] string propName = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        private async void SymbolSetPropertyGrid_SelectedObjectChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            this.CodeSource = "";
            if (SymbolSetPropertyGrid.SelectedObject is Fields_Base) {
                var fldsBase = (Fields_Base)SymbolSetPropertyGrid.SelectedObject;
                this.ImageSource = await GenerateBitmapImageAsync(fldsBase.ChangedAttributeValues);
            }
            else {
                this.ImageSource = null;
            }
        }

        private void SymbolSetPropertyGrid_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e) {
            if (this.SymbolSetPropertyGrid.SelectedObject is Fields_Base && !_ignoreEvents) {
                UpdateSymbolAndCode();
            }
        }

        private string ConvertAttributesToCode(Dictionary<string, object> formattedAttributes) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("");
            string indent = "\t";
            sb.AppendLine(string.Format(
                    "{0}Dictionary <string, object> values = new Dictionary<string, object>();", indent));

            foreach (var kvp in formattedAttributes) {
                sb.AppendLine(string.Format("{0}values.Add({1}, {2});", indent, kvp.Key, kvp.Value));
            }
            sb.AppendLine(string.Format("\r\n{0}CIMSymbol symbol = await QueuedTask.Run(() => ", indent));
            sb.AppendLine(string.Format("{0}ArcGIS.Desktop.Mapping.SymbolFactory.GetDictionarySymbol(\"mil2525d\",values));", indent));
            return sb.ToString();
        }

        private Task<ImageSource> GenerateBitmapImageAsync(Dictionary<string, object> attributes) {
            return QueuedTask.Run(() => {
                CIMSymbol symbol = ArcGIS.Desktop.Mapping.SymbolFactory.Instance.GetDictionarySymbol("mil2525d", attributes);
                
                //At 1.3, 64 is the max pixel size we can scale to. This will be enhanced at 1.4 to support
                //scaling (eg up to 256, the preferred review size for military symbols)
                var si = new SymbolStyleItem() {
                    Symbol = symbol,
                    PatchHeight = 64,
                    PatchWidth = 64
                };
                return si.PreviewImage;
            });
        }

        private async void DictionarySymbolView_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "CurrentSymbolSet" && !_ignoreEvents) {
                UpdatePropertyGridSelectedObject();
            }
            else if (e.PropertyName == "SelectedFeature" && !_ignoreEvents) {
                _ignoreEvents = true;
                if (this._selectedLayer == null || this._selectedOid == -1)
                    return;

                Inspector inspector = new Inspector();
                await inspector.LoadAsync(this._selectedLayer, this._selectedOid);
                var attributes = inspector.ToDictionary(a => a.FieldName.ToLower(),
                    a => a.CurrentValue.GetType() != typeof(System.DBNull) ? a.CurrentValue : null);

                //change the symbology combobox
                var symbolSetID = Convert.ToInt32(attributes["symbolset"]);
                var symbolSets = SymbolSet.SymbolSets.Where(
                    kvp => kvp.Value.SymbolSetID == symbolSetID).Select(kvp => kvp.Value).ToList();
                if (symbolSets.Count() > 1) {
                    //There is more than one choice
                    if (this._selectedLayer.ShapeType == esriGeometryType.esriGeometryPoint) {
                        this.CurrentSymbolSet = symbolSets.First(s => s.SchemaNameAlias.Contains("Points"));
                    }
                    else if (this._selectedLayer.ShapeType == esriGeometryType.esriGeometryPolyline) {
                        this.CurrentSymbolSet = symbolSets.First(s => s.SchemaNameAlias.Contains("Lines"));
                    }
                    else { //Must be poly
                        this.CurrentSymbolSet = symbolSets.First(s => s.SchemaNameAlias.Contains("Areas"));
                    }
                }
                else {
                    this.CurrentSymbolSet = symbolSets[0];
                }
                UpdatePropertyGridSelectedObject();

                if (SymbolSetPropertyGrid.SelectedObject != null) {
                    ((Fields_Base)SymbolSetPropertyGrid.SelectedObject).ChangeAttributeValues(attributes);
                }
                UpdateSymbolAndCode();
                _ignoreEvents = false;
            }
        }

        private void UpdatePropertyGridSelectedObject() {
            if (CurrentSymbolSet != null) {
                var fieldsClass = Fields_Base.FieldClasses[CurrentSymbolSet.SchemaName];
                SymbolSetPropertyGrid.SelectedObject = fieldsClass;
                SymbolSetPropertyGrid.SelectedObjectTypeName = string.Format("{0}:", fieldsClass.SchemaName);
                SymbolSetPropertyGrid.SelectedObjectName = fieldsClass.SymbolSetName;
            }
        }

        private async void UpdateSymbolAndCode() {
            var fldsBase = (Fields_Base)this.SymbolSetPropertyGrid.SelectedObject;
            this.CodeSource =
                ConvertAttributesToCode(fldsBase.ChangedAttributeFormattedValues);
            this.ImageSource = await GenerateBitmapImageAsync(fldsBase.ChangedAttributeValues);
        }
    }
}
