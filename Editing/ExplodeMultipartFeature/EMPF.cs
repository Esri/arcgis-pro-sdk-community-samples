//   Copyright 2015 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ExplodeMultipartFeature
{
  internal class EMPF : Button
  {
    /// <summary>
    /// Separate a selected multipart feature into individual features.
    /// </summary>
    protected override void OnClick()
    {
      //check for one selected feature
      if (MapView.Active.Map.SelectionCount != 1)
      {
        MessageBox.Show("Please select one multipart feature to explode", "Explode Multipart Feature");
        return;
      }

      //run on MCT
      QueuedTask.Run(() =>
      {
        //get selected feature geometry
        var selectedFeatures = MapView.Active.Map.GetSelection();
        var insp = new Inspector();
        insp.Load(selectedFeatures.Keys.First(), selectedFeatures.Values.First());
        var selGeom = insp.Shape;
        var selGeomType = selGeom.GeometryType;

        //early checks for geometry type and single point in a multipoint
        if ( !(selGeomType == GeometryType.Multipoint || selGeomType == GeometryType.Polygon || selGeomType == GeometryType.Polyline) || selGeom.PointCount == 1)
        {
          MessageBox.Show("Please select a multipart feature to explode", "Explode Multipart Feature");
          return;
        }

        //check if selected feature has multiple parts
        var mpGeom = selGeom as Multipart;
        if (mpGeom != null)
          if (mpGeom.PartCount < 2)
          {
            MessageBox.Show("Please select a multipart feature to explode","Explode Multipart Feature");
            return;
          }

        //setup the edit operation
        var op = new EditOperation();
        op.Name = "Explode Multipart Feature";

        //handle geometry types
        switch(selGeomType)
        {
          case GeometryType.Multipoint:
            //create a new feature for each pointcount
            var mpoint = selGeom as Multipoint;
            for (var i = 0; i < mpoint.PointCount; i++)
            {
              //copy the original feature into a dictionary and update the shape.
              var newFeature = insp.ToDictionary(a => a.FieldName, a => a.CurrentValue);
              newFeature[insp.GeometryAttribute.FieldName] = new MultipointBuilder(mpoint.Points[i]).ToGeometry();
              op.Create(insp.MapMember, newFeature);
            }
            break;

          case GeometryType.Polyline:
            //create a new feature for each polyline part
            for (var i = 0; i < mpGeom.PartCount; i++)
            {
              //copy the original feature into a dictionary and update the shape.
              var newFeature = insp.ToDictionary(a => a.FieldName, a => a.CurrentValue);
              newFeature[insp.GeometryAttribute.FieldName] = new PolylineBuilder(mpGeom.Parts[i]).ToGeometry();
              op.Create(insp.MapMember, newFeature);
            }
          break;

          case GeometryType.Polygon:
            //ignore donut features for now
            //check if any part area is negative
            for (var i = 0; i < mpGeom.PartCount; i++)
            {
              if ((new PolygonBuilder(mpGeom.Parts[i]).ToGeometry()).Area < 0)
              {
                MessageBox.Show("Please select a non-donut polygon to explode", "Explode Mutltpart Feature");
                return;
              }
            }

            //create a new feature for each polygon part
            for (var i = 0; i < mpGeom.PartCount; i++)
            {
              //copy the original feature into a dictionary and update the shape.
              var newFeature = insp.ToDictionary(a => a.FieldName, a => a.CurrentValue);
              newFeature[insp.GeometryAttribute.FieldName] = new PolygonBuilder(mpGeom.Parts[i]).ToGeometry();
              op.Create(insp.MapMember, newFeature);
            }
            break;
        } //switch

        //delete the original feature and execute the creates
        op.Delete(insp.MapMember, (long)(int)insp.ObjectIDAttribute.CurrentValue);
        op.Execute();
      });
    }
  }
}
