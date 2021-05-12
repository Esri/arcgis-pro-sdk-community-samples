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
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Inspector_AddAttributeAsync
{
  internal class ApplyAttributesTool : MapTool
  {
    private AttributeControlViewModel _attributeVM = null;

    public ApplyAttributesTool()
    {
      // indicate that you need feedback graphics
      IsSketchTool = true;
      // set the type of the feedback graphics
      SketchType = SketchGeometryType.Rectangle;
      // the coordinates of the sketch geometry should be returned in map coordinates
      SketchOutputMode = SketchOutputMode.Map;

      // specify the ID for the embeddable control as declared in the config.daml
      // and specified in the AttributeControl.xaml for the UI and AttributeControlViewModel for the 
      // view model.
      ControlID = "Inspector_AddAttributeAsync_AttributeControl";
    }

    private FeatureLayer _pointLayer = null;

    private Dictionary<string, bool> _attributesValid;
    private Dictionary<string, object> _attributes;

    protected override async Task OnToolActivateAsync(bool active)
    {
      // get the Precincts feature layer in the active map
      _pointLayer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().
                              Where(lyr => lyr.Name == "Police Stations").FirstOrDefault();
      if (_pointLayer == null)
        return;

      // build the attribute dictionaries
      _attributesValid = new Dictionary<string, bool>();
      _attributes = new Dictionary<string, object>();

      // get the embedded control
      if (_attributeVM == null)
      {
        _attributeVM = this.EmbeddableControl as AttributeControlViewModel;
      }

      // these are the fields that we will collect values from the user
      var fieldNames = new List<string>() { "Precinct", "Address" };

      // set up the inspector
      var inspector = new Inspector(false);
      foreach (var fieldName in fieldNames)
      {
        // add the attribute
        ArcGIS.Desktop.Editing.Attributes.Attribute attr = await inspector.AddAttributeAsync(_pointLayer, fieldName, false);
        // set the validity to true
        _attributesValid.Add(fieldName, true);

        // add some validation - in this example we will make each field mandatory
        attr.AddValidate(() =>
        {
          var errors = new List<ArcGIS.Desktop.Editing.Attributes.Attribute.ValidationError>();
          if (string.IsNullOrWhiteSpace(attr.CurrentValue.ToString()))
          {
            // add an error
            errors.Add(ArcGIS.Desktop.Editing.Attributes.Attribute.ValidationError.Create("Value is mandatory", ArcGIS.Desktop.Editing.Attributes.Severity.High));
            // set the validity to false
            _attributesValid[fieldName] = false;
          }
          else
          {
            // store the value
            if (!_attributes.ContainsKey(fieldName))
              _attributes.Add(fieldName, attr.CurrentValue);
            else
              _attributes[fieldName] = attr.CurrentValue;

            // set the validity to true
            _attributesValid[fieldName] = true;
          }
          return errors;
        });
      }

      // create the embedded control and assign the view/viewmodel pair
      var tuple = inspector.CreateEmbeddableControl();
      _attributeVM.InspectorViewModel = tuple.Item1;
      _attributeVM.InspectorView = tuple.Item2;
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      // if we have a valid view model
      if (_attributeVM != null)
      {
        // free the embeddable control resources
        _attributeVM.InspectorView = null;
        _attributeVM.InspectorViewModel.Dispose();
      }
      _attributeVM = null;

      return base.OnToolDeactivateAsync(hasMapViewChanged);
    }

    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (_pointLayer == null)
        return true;

      // execute the select on the MCT
      var result = await QueuedTask.Run(() =>
			{
				// define the spatial query filter
				var spatialQuery = new SpatialQueryFilter() { FilterGeometry = geometry, SpatialRelationship = SpatialRelationship.Contains };

				// gather the selection
				var pointSelection = _pointLayer.Select(spatialQuery);

				// get the list of oids
				List<long> oids = pointSelection.GetObjectIDs().ToList();
				if (oids.Count == 0)
					return false;

				// if some attributes aren't valid
				if (_attributesValid.ContainsValue(false))
					return false;

				// everything is valid - apply to the identified features
				var editOp = new EditOperation();
				editOp.Name = "Apply edits";
				foreach (var oid in oids)
					editOp.Modify(_pointLayer, oid, _attributes);
				editOp.Execute();

				return true;
			});

      return result;
    }
  }
}
