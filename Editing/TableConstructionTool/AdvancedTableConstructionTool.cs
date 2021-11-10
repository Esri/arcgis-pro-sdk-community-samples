using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Templates;
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

namespace TableConstructionTool
{
  class AdvancedTableConstructionTool : MapTool
  {
    public AdvancedTableConstructionTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      //Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
      UsesCurrentTemplate = true;
    }
    #region Tool Options
    private ReadOnlyToolOptions ToolOptions => CurrentTemplate?.GetToolOptions(ID);

    private string FeatureLayerToUse
    {
      get
      {
        if (ToolOptions == null)
          return "";

        return ToolOptions.GetProperty(TableConstructionToolOptionsViewModel.SelectedLayerOptionName, "");
      }
    }

    private string FeatureLayerFieldToUse
    {
      get
      {
        if (ToolOptions == null)
          return "";

        return ToolOptions.GetProperty(TableConstructionToolOptionsViewModel.SelectedLayerFieldOptionName, "");
      }
    }

    private string TableFieldToEdit
    {
      get
      {
        if (ToolOptions == null) return "";
        return ToolOptions.GetProperty(TableConstructionToolOptionsViewModel.SelectedTableFieldOptionName, "");
      }
    }
    #endregion
   
    /// <summary>
    /// Called when the "Create" button is clicked. This is where we will create the edit operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch - will be null because SketchType = SketchGeometryType.None</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (CurrentTemplate == null)
        return Task.FromResult(false);

      if (geometry == null) return Task.FromResult(false);

      if ((FeatureLayerToUse == "") || (FeatureLayerFieldToUse == ""))
          return Task.FromResult(false);
      QueuedTask.Run(() => {
        //get the features that intersect the geometry
        var features = MapView.Active.SelectFeatures(geometry, SelectionCombinationMethod.New, true);

        //get the cities layer and oids selected
        var layerOfInterest = features.Keys.FirstOrDefault(f => f.Name == FeatureLayerToUse) as FeatureLayer;
        if (layerOfInterest == null) return;// no features intersect the geometry

        var layerOIDs = features[layerOfInterest];
        this.CurrentTemplateRows = layerOIDs.Count;
        string oidField;
        int idxField = -1;
        using (var table = layerOfInterest.GetTable())
        {
          var def = table.GetDefinition();
          idxField = def.FindField(FeatureLayerFieldToUse);
          oidField = def.GetObjectIDField();
        }

        // Field was not found
        if (idxField == -1)
          return;

        //Query the layer to get the "field" values of the features
        QueryFilter qf = new QueryFilter()
        {
          ObjectIDs = layerOIDs,
          SubFields = FeatureLayerFieldToUse
        };
        List<string> fieldValueList = new List<string>(); //holds all the city names
        using (var rc = layerOfInterest.Search(qf))
        {
          while (rc.MoveNext())
          {
            using (var record = rc.Current)
            {
              fieldValueList.Add(record[FeatureLayerFieldToUse].ToString());
            }
          }
        }
        //Check if the standalone table has the required field
        //The new records added to the table will have the "NAME" field pre-populated with the values from the Cities layer

        var tableToEdit = CurrentTemplate.StandaloneTable;
        if (tableToEdit == null) return;
        int idxTableField = -1;
        using (var table = tableToEdit.GetTable())
        {
          var def = table.GetDefinition();
          idxTableField = def.FindField(TableFieldToEdit);
        }
        // "NAME field was not found
        if (idxTableField == -1)
          return;
        // Create an edit operation
        var createOperation = new EditOperation();
        createOperation.Name = string.Format("Create {0}", tableToEdit.Name);
        createOperation.SelectNewFeatures = false;

        //Iterate through the collection of name and
        //create a record in the standalone table for each.
        foreach (var cityName in fieldValueList)
        {
          // include the attributes via a dictionary
          var atts = new Dictionary<string, object>();
          atts.Add(TableFieldToEdit, cityName); 
          createOperation.Create(tableToEdit, atts);
        }
        createOperation.Execute();
      });

      return base.OnSketchCompleteAsync(geometry);
    }
   
  }
}
