/*

   Copyright 2024 Esri

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
using ArcGIS.Desktop.Mapping;
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


namespace EditorInspectorUI.InspectorUI
{
  /// <summary>
  /// Interaction logic for AttributeControlView.xaml
  /// </summary>
  public partial class AttributeControlView : UserControl
  {
    public AttributeControlView()
    {
      InitializeComponent();
    }
    private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      var vm = this.DataContext as AttributeControlViewModel;

      var layerTreeView = sender as TreeView;

      var currentItem = layerTreeView.Items.CurrentItem;
      var selection = (KeyValuePair<MapMember, List<PermitRecord>>)currentItem;
      var buildingPermitsLayer = selection.Key;

      var selectedTreeviewItem = layerTreeView.SelectedItem;
      //// if an oid is selected
      if (selectedTreeviewItem.GetType() == typeof(PermitRecord))
      {
        // load it
        var permitRecord = selectedTreeviewItem as PermitRecord;
        var selectedOID = permitRecord.OID;
        //Load the selected feature into the inspector
        vm.AttributeInspector?.LoadAsync(buildingPermitsLayer, selectedOID);
      }
      // else layer is selected.  if there's some OIDs, clear the inspector
      else if (selection.Value.Count > 0)
      {
        vm.AttributeInspector.ClearAsync();
      }
      // otherwise the schema should be displayed
    }
  }
}
