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

using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace TransferAttributes
{
  /// <summary>
  /// Use this mapTool to transfer attributes from a template to an identified feature.  
  /// The tool presents a UI via the embedded control "TransferAttributes_ChooseTemplate" for the user to choose the appropriate layer, template combination.
  /// Use the burger button on the UI to view and define field mappings between source and target layers. These field mappings are 
  /// where a user can specify how attribute field values on a source layer are processed and copied to fields on a target layer. 
  /// </summary>
  /// <remarks>
  /// At 2.4, EditOperation.TransferAttributes method is one of the few functions which honors preset field mapping combinations. 
  /// 
  /// The only method signature for EditOperation.TransferAttributes requires a source and target feature
  /// So our workflow will be 
  ///   Create a temporary feature using the chosen template and empty geometry
  ///   Call TransferAttributes from the temporary feature to the target feature
  ///   Delete the temporary feature
  /// </remarks>
  internal class TransferAttributes_Template : MapTool
  {
    public TransferAttributes_Template()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;      // allow only a single click
      SketchOutputMode = SketchOutputMode.Map;
      ControlID = "TransferAttributes_ChooseTemplate";
    }

    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      // get the embedded control
      var vm = this.EmbeddableControl as ChooseTemplateViewModel;
      if (vm == null)
        return false;

      // ensure there's a template chosen
      if (vm.SelectedTemplate == null)
      {
        vm.ShowMessage("Please choose a layer and template");
        return false;
      }
      // clear any message
      vm.ClearMessage();

      var template = vm.SelectedTemplate;
      BasicFeatureLayer templateLayer = template.Layer as BasicFeatureLayer;
      if (templateLayer == null)
        return false;

      bool result = await QueuedTask.Run(() =>
      {
        // find the target feature from the geometry click
        var targetFeatures = MapView.Active.GetFeatures(geometry);
        if ((targetFeatures == null) || (targetFeatures.Keys.Count == 0))
          return false;

        // we will use the first feature returned
        var targetLayer = targetFeatures.Keys.First();
        var targetOID = targetFeatures[targetLayer][0];

        // At 2.4, EditOperation.TransferAttributes method is one of the few functions which honors the field mapping.
        // The only method signature for EditOperation.TransferAttributes requires a source and target feature
        // So our workflow will be 
        // 1.  Create a temporary feature using the chosen template and empty geometry
        // 2.  Call TransferAttributes from the temporary feature to the target feature
        // 3.  Delete the temporary feature

        // build an empty geometry according to the correct template layer type
        Geometry emptyGeometry = null;
        switch (templateLayer.ShapeType)
        {
          case esriGeometryType.esriGeometryPoint:
            emptyGeometry = MapPointBuilder.CreateMapPoint();
            break;
          case esriGeometryType.esriGeometryPolyline:
            emptyGeometry = PolylineBuilder.CreatePolyline();
            break;
          case esriGeometryType.esriGeometryPolygon:
            emptyGeometry = PolygonBuilder.CreatePolygon();
            break;
        }

        // some other geometry type
        if (emptyGeometry == null)
          return false;

        long newObjectID = -1;

        // create the temporary feature using the empty geometry
        var op = new EditOperation();
        op.Name = "Transfer attributes from template";

        // note Create signature.. we are interested in the new ObjectID
        op.Create(template, emptyGeometry, object_id => newObjectID = object_id);
        // execute
        var opResult = op.Execute();

        // if create was successful
        if (opResult)
        {
          // chain to create a new operation
          var opChain = op.CreateChainedOperation();
          // transfer the attributes between the temporary feature and the target feature
          opChain.TransferAttributes(templateLayer, newObjectID, targetLayer, targetOID);
          // and now delete the temporary feature
          opChain.Delete(templateLayer, newObjectID);
          opResult = opChain.Execute();
        }

        return opResult;
      });

      return result;
    }
  }
}
