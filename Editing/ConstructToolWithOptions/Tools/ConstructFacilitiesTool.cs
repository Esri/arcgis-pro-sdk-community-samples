/*

   Copyright 2022 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace ConstructToolWithOptions
{
  internal class ConstructFacilitiesTool : MapTool
  {
    public ConstructFacilitiesTool()
    {
      IsSketchTool = true;
      UseSnapping = true;
      // Select the type of construction tool you wish to implement.  
      // Make sure that the tool is correctly registered with the correct component category type in the daml 
      SketchType = SketchGeometryType.Point;
      // SketchType = SketchGeometryType.Line;
      // SketchType = SketchGeometryType.Polygon;
      //Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
      UsesCurrentTemplate = true;
      //Gets or sets whether the tool supports firing sketch events when the map sketch changes. 
      //Default value is false.
      FireSketchEvents = true;
    }

    private ReadOnlyToolOptions ToolOptions => CurrentTemplate?.GetToolOptions(ID);

    private bool SaveLastSubtypeChoiceToDefaults
    {
      get
      {
        if (ToolOptions == null)
          return ConstructFacilitiesToolToolOptionsViewModel.DefaultSaveLastSubtypeChoiceToDefaults;

        return ToolOptions.GetProperty(ConstructFacilitiesToolToolOptionsViewModel.SaveLastSubtypeChoiceToDefaultsName,
          ConstructFacilitiesToolToolOptionsViewModel.DefaultSaveLastSubtypeChoiceToDefaults);
      }
    }

    private bool UseSubtypeChoiceOverride
    {
      get
      {
        if (ToolOptions == null)
          return ConstructFacilitiesToolToolOptionsViewModel.DefaultUseSubtypeChoiceOverride;

        return ToolOptions.GetProperty(ConstructFacilitiesToolToolOptionsViewModel.UseSubtypeChoiceOverrideName,
          ConstructFacilitiesToolToolOptionsViewModel.DefaultUseSubtypeChoiceOverride);
      }
    }

    protected override Task OnToolActivateAsync(bool hasMapViewChanged)
    {
      //turn autogeneration off
      QueuedTask.Run(() => CurrentTemplate.Layer?.AutoGenerateTemplates(false));
      return base.OnToolActivateAsync(hasMapViewChanged);
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      if (!SaveLastSubtypeChoiceToDefaults || CurrentTemplate == null)
      {
        return base.OnToolDeactivateAsync(hasMapViewChanged);
      }

      return QueuedTask.Run(() =>
      {
        var templateDef = this.CurrentTemplate.GetDefinition() as CIMRowTemplate;

        if (templateDef.DefaultValues == null)
          templateDef.DefaultValues = new Dictionary<string, object>();

        var choice = Module1.Current.SelectedSubtypeChoice;
        templateDef.DefaultValues["SUBTYPEFIELD"] = choice.SubtypefieldValue;
        templateDef.DefaultValues["FEATURECODE"] = choice.FeatureCodeValue;

        this.CurrentTemplate.SetDefinition(templateDef);
        Project.Current.SetDirty();//enable save
      });
    }


    /// <summary>
    /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch.</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (CurrentTemplate == null || geometry == null)
        return Task.FromResult(false);

      return QueuedTask.Run(() =>
      {

        //apply overrides
        if (UseSubtypeChoiceOverride)
        {
          var choice = Module1.Current.SelectedSubtypeChoice;
          this.CurrentTemplate.Inspector["SUBTYPEFIELD"] = choice.SubtypefieldValue;
          this.CurrentTemplate.Inspector["FEATURECODE"] = choice.FeatureCodeValue;
        }
        else
        {
          this.CurrentTemplate.Inspector.Cancel();
        }

        // Create an edit operation
        var createOperation = new EditOperation();
        createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
        createOperation.SelectNewFeatures = true;

        // Queue feature creation
        createOperation.Create(CurrentTemplate, geometry);
        return createOperation.Execute();
      });
    }
  }
}
