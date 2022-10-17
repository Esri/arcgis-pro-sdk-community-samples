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
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExportDiagramToFeatureClasses
{
  /// <summary>
  /// There are some ArcGIS Pro functions you can use to export diagrams. For example, the Export Diagram Content geoprocessing tool allows you to export a JSON file that describes a network diagram content (see https://pro.arcgis.com/en/pro-app/latest/tool-reference/network-diagram/export-diagram-content.htm). The output JSON file can then be used for network calculation and analysis or to feed external system. 
  /// Starting from an open network diagram map, you can also use the Export Map functions available on the Share tab in the Output group, to export the active diagram map as a file including vector and raster formats.
  /// 
  /// This add-in demonstrates another way to export a network diagram. It applies to any network diagram present in an active diagram map and exports its content into a set of feature classes in a feature dataset in a local geodatabase (File or Mobile). The output feature dataset can then be added to a map, shared with others, and so on.
  /// 
  /// If you want, you can also turn on some extra options to:
  /// - Export the diagram aggregations into a specific table in the output geodatabase
  /// - Add the exported features to a new map under feature layers built with the same layer properties as under the original network diagram layer.
  /// 
  /// > NOTE: The ExportDiagramToFeatureClasses add-in code is a generic code sample that performs any network diagram related to any utility or trace network dataset.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio, click the Build menu. Then select Build Solution.  
  /// 1. Start ArcGIS Pro.  
  /// 1. Open your favorite utility network or trace network ArcGIS Pro project.
  /// 1. Open the network diagram you want to export or generate a new network diagram.
  /// 1. In the Add-In tab on the ribbon, click Export Diagram To Feature Classes:
  /// 
  ///     ![UI](Screenshots/ExportDiagramToFeatureClassesButton.png)
  /// 
  ///     The Export Diagram To Feature Classes pane window opens:
  /// 
  ///     ![UI](Screenshots/ExportDiagramToFeatureClassesPaneWindow1.png)
  /// 
  /// 1. Expand the Output Geodatabase section to specify the output geodatabase into which you want to export your network diagram.
  /// 1. Click the browse button next to the Folder text area. Then, browse to and select the output folder where this geodatabase already exists or will be created.
  /// 1. From the Type drop down list, pick up the type of output geodatabase you want to create—File or Mobile.
  /// 1. In the Name text area, type the name of the output geodatabase.
  /// 1. Then, expand the Options section.
  /// 1. To export the diagram aggregations, make sure the 'Export aggregations' option is checked.
  /// 
  /// 1. If you want the exported diagram features to display in a new map once the export completes, check 'Add to a new map'.
  /// The Export Diagram To Feature Classes pane window should look like as follows:
  /// 
  ///     ![UI](Screenshots/ExportDiagramToFeatureClassesPaneWindow2.png)
  /// 
  /// 1. Make sure the diagram map you want to export is the active map and click Run.
  /// The export process starts. A message displays at the bottom of the pane window when it completes.
  /// 
  ///     ![UI](Screenshots/ExportDiagramToFeatureClassesPaneWindow2.png)
  /// 
  /// 1. In the Catalog pane window, go to the specified output folder and expand the output geodatabase where your diagram should have been exported. The next paragraphs explain how it is exported.
  /// 
  ///     ![UI](Screenshots/ExportDiagramToFeatureClasses_OutputFC.png)
  /// 
  /// In the specified output geodatabase, you should see a new feature dataset whose name corresponds to the exported network diagram layer name. Under this feature dataset, there is a set of feature classes whose names correspond to utility or trace network source class names. There is a feature class with a given network source class name when there is at least one diagram feature representing such a network source feature in the original diagram. For example, if there are structure junctions in the original diagram, the StructureJunction class is created under the exported feature dataset and all the structure diagram junctions are exported in this class. When there is no structure junction in the original diagram, there is no StructureJunction class in the exported feature dataset.
  /// 
  /// Any diagram feature whose geometry type changed in the original diagram regarding to its source network feature geometry is exported in a feature class with a specific suffix in its name. This happens for any point network feature that exists as a diagram polygon container in the original diagram. Such a diagram container is exported in a feature class whose name has the _C suffix. For example, a junction box diagram container is exported in the StructureJunction_C feature class.
  /// 
  /// In the same way, any polygon network feature or line network feature that exists in the original diagram as a diagram junction is exported in a feature class whose name has the _J suffix. For example a Substation diagram junction is exported in the StructureBoundary_C feature class.
  /// Under this feature dataset, you can also see a ReductionEdges line feature class for any exported reduction diagram edge, a SystemJunctions point feature class for the exported system junctions, and an Associations line feature class for the connectivity associations and structural attachments present in the exported diagram.
  /// 
  /// With the 'Export aggregations' option checked, you also get the Aggregations table created in the output geodatabase. This table lists the GlobalIDs of all the network features that are collapsed or reduced in the diagram with the diagram element ID of the diagram feature that aggregates them (AggregationDEID).
  /// You can export various diagrams in the same output geodatabase. In this case, a feature dataset is created for each exported diagram. Since class names are unique inside the entire geodatabase, the names of the newly created feature classes are automatically suffixed with numbers to avoid duplicated names.
  /// 
  /// When the 'Add to a new map' option is checked, the process continues after the diagram export completion. It first creates a new map whose name corresponds to the diagram layer name suffixed with a number. Then, it analyzes the layer definitions on the sublayers under the original network diagram layer and builds new layers with similar layer definitions in the newly open map for the exported diagram features.
  /// 
  /// ![UI](Screenshots/ExportDiagramToFeatureClasses_AddedToAMap.png)
  /// </remarks>

  internal class ExportDiagramToFeatureClassesModule : Module
  {
    private static ExportDiagramToFeatureClassesModule _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static ExportDiagramToFeatureClassesModule Current => _this ??= (ExportDiagramToFeatureClassesModule)FrameworkApplication.FindModule("ExportDiagramToFeatureClasses_Module");

    #region Overrides
    /// <summary>
    /// Called by the framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides


    /// <summary>
    /// Show a message
    /// </summary>
    /// <param name="Message">Message</param>
    /// <param name="Title">Title</param>
    internal static void ShowMessage(string Message, string Title = "")
    {
      if (string.IsNullOrEmpty(Title))
      { ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Message); }
      else
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Message, Title);
      }
    }

    /// <summary>
    /// Format the message return by the SchemaBuilder
    /// </summary>
    /// <param name="ErrorMessages">List of error messages</param>
    /// <returns>string</returns>
    internal static string FormatErrors(IReadOnlyList<string> ErrorMessages)
    {
      if (ErrorMessages.Count == 0)
        return "";

      StringBuilder sb = new();
      foreach (string errorMessage in ErrorMessages)
        sb.AppendLine(errorMessage);

      return sb.ToString();
    }

    /// <summary>
    /// Format exception and InnerException
    /// </summary>
    /// <param name="ex">Exception</param>
    /// <returns>Return exception and InnerException as string</returns>
    internal static string ExceptionFormat(Exception ex)
    {
      if (ex.InnerException != null)
      {
        string s = ExceptionFormat(ex.InnerException);

        if (s.Length > 0)
          return string.Format("{0}\n{1}\n{2}", ex.Message, s, ex.StackTrace);
      }

      string msg = string.Format("{0}\n{1}", ex.Message, ex.StackTrace);
      System.Diagnostics.Debug.WriteLine(msg);
      return msg;
    }

    /// <summary>
    /// Open a dataset
    /// </summary>
    /// <typeparam name="T">Type of dataset</typeparam>
    /// <param name="Name">Dataset name</param>
    /// <returns>A dataset of T type</returns>
    internal static T OpenDataset<T>(Geodatabase Database, string Name) where T : Dataset
    {
      if (Database == null)
        return default;

      GeodatabaseType geodatabaseType = Database.GetGeodatabaseType();
      if (geodatabaseType == GeodatabaseType.Service)
      {
        Type t = typeof(T);

        if (t == typeof(Table))
        {
          IReadOnlyList<TableDefinition> allTables = Database.GetDefinitions<TableDefinition>();
          foreach (TableDefinition tableDef in allTables)
          {
            string aliasName = tableDef.GetAliasName();
            if (aliasName.Replace(" ", "").Equals(Name, StringComparison.OrdinalIgnoreCase))
            {
              return Database.OpenDataset<T>(tableDef.GetName());
            }
          }
        }
        else if (t == typeof(FeatureClass))
        {
          IReadOnlyList<FeatureClassDefinition> allFeatureClasses = Database.GetDefinitions<FeatureClassDefinition>();
          foreach (FeatureClassDefinition fcDef in allFeatureClasses)
          {
            string aliasName = fcDef.GetAliasName();
            if (!String.IsNullOrEmpty(aliasName))
            {
              if (aliasName == Name || aliasName.Replace(" ", "").Equals(Name, StringComparison.OrdinalIgnoreCase))
              {
                return Database.OpenDataset<T>(fcDef.GetName());
              }
            }
            else
            {
              //Weird case, where the alias name is empty (Error Tables, Dirty Areas)
              string fullName = fcDef.GetName();
              if (fullName.Replace("_", "").Contains(Name)) //the Point errors, line errors, polygon errors and dirty areas come back as  Dirty_Areas (want to compare to DirtyAreas). 
              {
                return Database.OpenDataset<T>(fullName);
              }
            }
          }
        }
        else if (t == typeof(RelationshipClass))
        {
          IReadOnlyList<RelationshipClassDefinition> allRelationshipClasses = Database.GetDefinitions<RelationshipClassDefinition>();
          foreach (RelationshipClassDefinition rcDef in allRelationshipClasses)
          {
            string aliasName = rcDef.GetAliasName();
            if (aliasName.Replace(" ", "").Equals(Name, StringComparison.OrdinalIgnoreCase))
            {
              return Database.OpenDataset<T>(rcDef.GetName());
            }
          }
        }
        else if (t == typeof(UtilityNetwork))
        {
          IReadOnlyList<UtilityNetworkDefinition> unDefinition = Database.GetDefinitions<UtilityNetworkDefinition>();
          foreach (UtilityNetworkDefinition unDef in unDefinition)
          {
            return Database.OpenDataset<T>(unDef.GetName());
          }
        }
        else
        {
          //There is no type supported in the Feature Service DB, have to return null
          return null;
        }
      }
      else if (geodatabaseType == GeodatabaseType.LocalDatabase)
      {
        return Database.OpenDataset<T>(Name);
      }
      else
      {
        if (Name.Contains('.'))
        {
          Name = Name[(Name.LastIndexOf(".") + 1)..];
        }

        return Database.OpenDataset<T>(Name);
      }
      return null;
    }

    /// <summary>
    /// Get the diagram layer in the active map
    /// </summary>
    /// <param name="ActiveMap">Diagram map where the diagram layer is</param>
    /// <returns>DiagramLayer</returns>
    internal static DiagramLayer GetDiagramLayerFromMap(Map ActiveMap)
    {
      if (ActiveMap == null || ActiveMap.MapType != MapType.NetworkDiagram)
        return null;

      IReadOnlyList<Layer> myLayers = ActiveMap.Layers;
      if (myLayers == null)
        return null;

      foreach (Layer l in myLayers)
      {
        if (l.GetType() == typeof(DiagramLayer))
          return l as DiagramLayer;
      }

      return null;
    }

    /// <summary>
    /// Create a new output map for the exported diagram
    /// </summary>
    /// <param name="Name">Map name</param>
    /// <param name="ShowMap">Shown after its creation or not</param>
    /// <returns>Map</returns>
    internal static async Task<Map> CreateNewMap(string Name, bool ShowMap = false)
    {
      Map newMap = MapFactory.Instance.CreateMap(Name, MapType.Map, MapViewingMode.Map, Basemap.ProjectDefault);
      if (newMap == null)
        return null;

      if (ShowMap)
      {
        await FrameworkApplication.Panes.CreateMapPaneAsync(newMap, MapViewingMode.Map);
      }
      return newMap;
    }

    /// <summary>
    /// Show the new output map
    /// </summary>
    /// <param name="MapToShow">Map</param>
    /// <returns>Pane</returns>
    internal static Task<IMapPane> ShowMap(Map MapToShow)
    {
      if (MapToShow != null)
      {
        return FrameworkApplication.Panes.CreateMapPaneAsync(MapToShow, MapViewingMode.Map);
      }

      return null;
    }

    /// <summary>
    /// Create a new group layer in the output map
    /// </summary>
    /// <param name="MapToAdd">Map</param>
    /// <param name="GroupName">New group layer name</param>
    /// <returns>GroupLayer</returns>
    internal static GroupLayer CreateGroupLayerInMap(Map MapToAdd, string GroupName)
    {
      int index = MapToAdd.Layers.Count - 2;
      if (index < 0)
      {
        index = 0;
      }

      return LayerFactory.Instance.CreateGroupLayer(MapToAdd, index, GroupName);
    }

    /// <summary>
    /// Add a layer to a layer container
    /// </summary>
    /// <param name="ParentLayer">Layer container</param>
    /// <param name="Name">Layer name</param>
    /// <param name="AssociatedFeatureClass">Associated feature class to show</param>
    /// <param name="WhereClause">Definition query</param>
    /// <param name="FilterName">Filter name</param>
    /// <param name="Renderer">Layer renderer</param>
    /// <returns>FeatureLayer</returns>
    internal static FeatureLayer AddFeatureLayerToGroup(ILayerContainerEdit ParentLayer, string Name, FeatureClass AssociatedFeatureClass, string WhereClause, string FilterName, RendererDefinition Renderer)
    {
      if (Name.Contains('.'))
      {
        Name = Name.Substring(Name.LastIndexOf('.') + 1);
      }
      FeatureLayerCreationParams flyrCreatnParam = new(AssociatedFeatureClass)
      {
        Name = Name,
        IsVisible = true,
        MinimumScale = 0,
        MaximumScale = 0,
        DefinitionQuery = new DefinitionQuery(whereClause: WhereClause, name: FilterName),
        RendererDefinition = Renderer
      };

      return LayerFactory.Instance.CreateLayer<FeatureLayer>(flyrCreatnParam, ParentLayer);
    }

    /// <summary>
    /// Add a table to a parent container
    /// </summary>
    /// <param name="ParentLayer">Parent container</param>
    /// <param name="Name">Table name</param>
    /// <param name="AssociatedTable">Associated table to show</param>
    /// <param name="WhereClause">Definition query</param>
    /// <param name="FilterName">Filter name</param>
    internal static void AddTableLayerToMap(IStandaloneTableContainerEdit ParentLayer, string Name, Table AssociatedTable, string WhereClause, string FilterName)
    {
      DefinitionQuery definitionQuery = new(whereClause: WhereClause, name: FilterName);
      StandaloneTableCreationParams flyrCreatnParam = new(AssociatedTable)
      {
        Name = Name
      };

      StandaloneTable standaloneTable = StandaloneTableFactory.Instance.CreateStandaloneTable(flyrCreatnParam, ParentLayer);

      if (standaloneTable != null && string.IsNullOrEmpty(standaloneTable.DefinitionQuery))
      {
        standaloneTable.SetDefinitionQuery(WhereClause);
      }
    }
  }
}
