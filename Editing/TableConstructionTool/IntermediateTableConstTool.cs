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
  class IntermediateTableConstTool : MapTool
  {
    public IntermediateTableConstTool()
    {
      SketchType = SketchGeometryType.Rectangle;
      IsSketchTool = true;
      //Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
      UsesCurrentTemplate = true;
    }

    /// <summary>
    /// Called when the "Create" button is clicked. This is where we will create the edit operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (CurrentTemplate == null)
        return Task.FromResult(false);

      if (geometry == null) return Task.FromResult(false);
     
      QueuedTask.Run(() => {
        //get the features that intersect the geometry
        var features = MapView.Active.SelectFeatures(geometry, SelectionCombinationMethod.New, true);

        //get the cities layer and oids selected
        var cityLayer = features.Keys.FirstOrDefault(f => f.Name == "USCities") as FeatureLayer;
        if (cityLayer == null) return;// no city features intersect the geometry

        var cityFeatureOids = features[cityLayer];

        //Get the "CITY_NAME" field values of the cities layer
        //And the ObjectID field.
        string nameField = "CITY_NAME";

        string oidField;
        int idxField = -1;
        using (var table = cityLayer.GetTable())
        {
          var def = table.GetDefinition();
          idxField = def.FindField(nameField);
          oidField = def.GetObjectIDField();
        }

        // "CITY_NAME" field was not found
        if (idxField == -1)
          return;

        //Query the layer to get the "city name" values of the features
        QueryFilter qf = new QueryFilter()
        {
          ObjectIDs = cityFeatureOids,
          SubFields = nameField
        };
        List<string> cityNamesList = new List<string>(); //holds all the city names
        using (var rc = cityLayer.Search(qf))
        {
          while (rc.MoveNext())
          {
            using (var record = rc.Current)
            {
              cityNamesList.Add(record[nameField].ToString());
            }
          }
        }
        //Check if the standalone table has the required field
        //The new records added to the table will have the "NAME" field pre-populated with the values from the Cities layer
        int idxTableField = -1;
        using (var table = CurrentTemplate.StandaloneTable.GetTable())
        {
          var def = table.GetDefinition();
          idxTableField = def.FindField("Name");
        }
        // "NAME field was not found
        if (idxTableField == -1)
          return;
        // Create an edit operation
        var createOperation = new EditOperation();
        createOperation.Name = string.Format("Create {0}", CurrentTemplate.StandaloneTable.Name);
        createOperation.SelectNewFeatures = false;

        //Iterate through the collection of name and
        //create a record in the standalone table for each.
        foreach (var cityName in cityNamesList)
        {
          // include the attributes via a dictionary
          var atts = new Dictionary<string, object>();
          atts.Add("NAME", cityName); //This one is hard coded too. I have to come up with something for this.
          createOperation.Create(CurrentTemplate.StandaloneTable, atts);
        }
        createOperation.Execute();
      });

      return base.OnSketchCompleteAsync(geometry);

    }

  }
}
