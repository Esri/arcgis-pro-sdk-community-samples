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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace TransferAttributes
{
  /// <summary>
  /// A map tool which allows users to identify a source and target feature and have the attributes transferred from the source to the target.  The 
  /// EditOperation.TransferAttributes call honors the Field Mapping set between the two layers. These field mappings are 
  /// where a user can specify how attribute field values on a source layer are processed and copied to fields on a target layer. 
  /// </summary>
  /// <remarks>
  /// At 2.4, EditOperation.TransferAttributes method is one of the few functions which honors preset field mapping combinations. 
  /// </remarks>
  internal class TransferAttributes_Features : MapTool
  {
    public TransferAttributes_Features()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Multipoint;   // set sketch to multipoint so we can get multiple point clicks
                                                    // override OnSketchModifiedAsync to limit the amount of points to a maximum of 2
      SketchOutputMode = SketchOutputMode.Map;
    }

    /// <summary>
    /// Force the sketch to complete after we've clicked twice
    /// </summary>
    /// <returns></returns>
    protected override async Task<bool> OnSketchModifiedAsync()
    {
      bool finish = await QueuedTask.Run(async () =>
      {
        // get the current sketch
        var sketchGeometry = await this.GetCurrentSketchAsync();
        // cast to multiPoint (our sketchType)
        Multipoint goo = sketchGeometry as Multipoint;
        // check the point count... if we have two points, return true
        if (goo.PointCount >= 2)
          return true;

        return false;
      });

      // call FinishSketchAsync if we have 2 points
      if (finish)
        finish = await base.FinishSketchAsync();

      return finish;
    }

    /// <summary>
    /// On two-point sketch completion, identify source and target features. Then transfer the attributes from the source to the target using 
    /// EditOperation.TransferAttributes
    /// </summary>
    /// <param name="geometry"></param>
    /// <returns></returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      return QueuedTask.Run(() =>
      {
        // manipulate the geometry into the two mapPoints
        Multipoint clickedPoints = geometry as Multipoint;
        if (clickedPoints == null)
          return false;

        if (clickedPoints.PointCount != 2)
          return false;

        var sourcePt = clickedPoints.Points[0];
        var targetPt = clickedPoints.Points[1];

        // get the source features under the sourcePt
        var sourceFeatures = MapView.Active.GetFeatures(sourcePt);

        // get the target features under the targetPt
        var targetFeatures = MapView.Active.GetFeatures(targetPt);

        // make sure there are actually some features
        if ((sourceFeatures == null) || (sourceFeatures.Keys.Count == 0) || 
                (targetFeatures == null) || (targetFeatures.Keys.Count == 0))
          return false;

        // take the first feature in each 
        var sourcelayer = sourceFeatures.Keys.First();
        var sourceOID = sourceFeatures[sourcelayer][0];

        var targetLayer = targetFeatures.Keys.First();
        var targetOID = targetFeatures[targetLayer][0];

        // create the edit operation
        var op = new EditOperation();
        op.Name = "Transfer attributes between Features";
        // call TransferAttributes
        op.TransferAttributes(sourcelayer, sourceOID, targetLayer, targetOID);
        // execute it
        op.Execute();

        return true;
      });
    }
  }
}
