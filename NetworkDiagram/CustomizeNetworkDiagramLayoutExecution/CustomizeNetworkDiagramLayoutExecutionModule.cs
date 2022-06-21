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
using ArcGIS.Core.Data.NetworkDiagrams;
using ArcGIS.Core.Data.UtilityNetwork;
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
using System.Windows.Input;

namespace CustomizeNetworkDiagramLayoutExecution
{
  /// <summary>
  /// With this add-in sample, you will get familiar with the Geometry, NetworkDiagrams, and Mapping API methods that allow you to develop your own diagram layout algorithm (see CustomDispatchLayout source code file).  
  /// You will also learn how to preset parameters for any network diagram core layouts, and execute them through add-in commands instead of the core Apply xx Layout geoprocessing tool(see PredefinedSmartTreeLayout code source file). Applying network diagram layouts to your diagrams in  this way is really fast and easy for users.  
  /// You will also know more about how to extend the Layout gallery with your preset network diagrams and with your own custom layout (see config.daml file).  
  /// Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page. 
  /// &gt; NOTE: The CustomDispatchLayout diagram layout code is a generic code sample and can be applied to any diagram even on other utility network dataset than the one provided with this sample. 
  /// </summary>  
  /// <remarks>
  /// 1. In Visual Studio, click the Build menu. Then select Build Solution.    
  /// 1. Click Start button to open ArcGIS Pro.    
  /// 1. ArcGIS Pro will open.    
  /// 1. Open **C:\Data\UtilityNetwork\UtilityNetworkSamples.aprx**. Make sure the Electric FGDB Editor map is open as the active map.  
  /// 1. Click on the Map tab on the ribbon. Then, in the Navigate group, expand Bookmarks and click Full Extent in the bookmark list.  
  /// 1. In the Utility Network tab in the ribbon, in the Subnetwork group, click Find Subnetwork Features: The Find Subnetworks pane window opens.  
  /// 1. Right-click the row corresponding to the RMT007 subnetwork and click Trace Subnetwork.  
  ///     ![UI](Screenshots/FindSubnetworkPane_TraceRMT007.png)  
  ///     The trace executes and selects 316 network features in the active map.  
  /// 1. Zoom to the selection set.  
  /// 1. In the Utility Network tab on the ribbon, in the Network Diagram group, click the drop-down arrow under New and click CollapseContainers:  
  ///     ![UI](Screenshots/NewDiagram_ClickOnCollapseContainers.png)  
  ///     Starting from the network features selected in the map and using the template clicked in the drop-down list, a new diagram is created.It opens in a new diagram map.  
  /// 1. In the Network Diagram tab in the ribbon, in the Layout gallery, click Custom Dispatch Layout in the Personal Layouts section:  
  ///     ![UI](Screenshots/CustomDispatchLayoutInTheGallery.png)  
  ///     The add-in Custom Dispatch Layout applies to the diagram content and dispatch the diagram features so they are spread out to each other.The diagram map is refreshed to reflect the changes:  
  ///     ![UI](Screenshots/CustomDispatchLayoutExpectedResult.png)  
  /// 1. In the Network Diagram tab in the ribbon, in the Layout gallery, click Predefined Smart Tree Layout in the Personal Layouts section:  
  ///     ![UI](Screenshots/PredefinedSmartTreeLayoutInTheGallery.png)  
  ///     The add-in Predefined Smart Tree Layout applies to the diagram content and execute the core Smart Tree layout with the expected parameters. The diagram map is refreshed to reflect the changes:  
  ///     ![UI](Screenshots/PredefinedSmartTreeLayoutExpectedResult.png)  
  ///     &gt; NOTE: When there is diagram features selected in the diagram map, these two layouts execute on the only selection set.  
  /// </remarks>
  internal class CustomizeNetworkDiagramLayoutExecutionModule : Module
  {
    private static CustomizeNetworkDiagramLayoutExecutionModule _this = null;
    internal static List<DiagramJunctionElement> g_DiagramJunctionElements;
    internal static List<DiagramEdgeElement> g_DiagramEdgeElements;
    internal static List<DiagramContainerElement> g_DiagramContainerElements;
    internal static List<DiagramJunctionElement> g_JunctionsToSave;
    internal static List<DiagramEdgeElement> g_EdgesToSave;


    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static CustomizeNetworkDiagramLayoutExecutionModule Current => _this ??= (CustomizeNetworkDiagramLayoutExecutionModule)FrameworkApplication.FindModule("CustomizeNetworkDiagramLayoutExecution_Module");

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
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
    /// Get all diagram Elements
    /// </summary>
    internal static void LoadDiagramFeatures(NetworkDiagram Diagram, out SelectionSet selection)
    {
      DiagramElementQueryResult deqr;
      selection = MapView.Active.Map.GetSelection();
      if (selection == null || selection.Count == 0)
      {
        DiagramElementQueryByElementTypes query = new DiagramElementQueryByElementTypes
        {
          QueryDiagramContainerElement = true,
          QueryDiagramEdgeElement = true,
          QueryDiagramJunctionElement = true
        };
        deqr = Diagram.QueryDiagramElements(query);
      }
      else
      {
        List<long> junctionIDs = new List<long>();
        List<long> edgeIDs = new List<long>();
        List<long> containerIDs = new List<long>();

        foreach (var v in selection.ToDictionary())
        {
          if (v.Key is FeatureLayer layer)
          {
            if (layer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint)
              junctionIDs.AddRange(v.Value);
            else if (layer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon)
              containerIDs.AddRange(v.Value);
            else if (layer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline)
              edgeIDs.AddRange(v.Value);
          }
        }

        DiagramElementQueryByObjectIDs query1 = new DiagramElementQueryByObjectIDs
        {
          JunctionObjectIDs = junctionIDs,
          ContainerObjectIDs = containerIDs,
          EdgeObjectIDs = edgeIDs
        };
        deqr = Diagram.QueryDiagramElements(query1);
      }


      g_DiagramJunctionElements = deqr.DiagramJunctionElements.ToList();
      g_DiagramEdgeElements = deqr.DiagramEdgeElements.ToList();
      g_DiagramContainerElements = deqr.DiagramContainerElements.ToList();
    }

    /// <summary>
    /// Save diagram in geodatabase
    /// </summary>
    /// <param name="Diagram">Diagram to save</param>
    internal static void SaveDiagram(NetworkDiagram Diagram)
    {
      if (g_JunctionsToSave.Count + g_EdgesToSave.Count > 0)
      {
        NetworkDiagramSubset nds = new NetworkDiagramSubset
        {
          DiagramContainerElements = null,
          DiagramEdgeElements = g_EdgesToSave,
          DiagramJunctionElements = g_JunctionsToSave
        };

        Diagram.SaveLayout(nds, true);

        MapView.Active.Redraw(true);
      }
    }

    /// <summary>
    /// Clean all global fields
    /// </summary>
    internal static void CleanModule()
    {
      g_DiagramJunctionElements = null;
      g_DiagramEdgeElements = null;
      g_DiagramContainerElements = null;
      g_JunctionsToSave = null;
      g_EdgesToSave = null;
    }

    /// <summary>
    /// Show a message box with the description of exception
    /// </summary>
    /// <param name="ex">Exception</param>
    internal static void ShowException(Exception ex)
    {
      string s = FormatException(ex);
      System.Diagnostics.Debug.WriteLine(s);
      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(s);
    }

    /// <summary>
    /// Format exception and InnerException
    /// </summary>
    /// <param name="ex">Exception</param>
    /// <returns>Return exception and InnerException as string</returns>
    private static string FormatException(Exception ex)
    {
      if (ex.InnerException != null)
      {
        string s = FormatException(ex.InnerException);

        if (s.Length > 0)
          return String.Format("{0}\n{1}", ex.Message, s);
      }

      return ex.Message;
    }

    /// <summary>
    /// Get the UtilityNetwork from the active Map
    /// </summary>
    /// <returns>UtilityNetwork</returns>
    internal static UtilityNetwork GetUtilityNetworkFromActiveMap()
    {
      if (MapView.Active != null)
      {
        IReadOnlyList<Layer> SelectedLayer = MapView.Active.Map.GetLayersAsFlattenedList();

        foreach (Layer l in SelectedLayer)
        {
          if ((l is UtilityNetworkLayer) || (l is FeatureLayer) || (l is SubtypeGroupLayer) || l is DiagramLayer)
          {
            UtilityNetwork un = GetUtilityNetworkFromLayer(l);
            if (un != null)
              return un;
          }
        }
      }

      return null;
    }

    /// <summary>
    /// Get the UtilityNetwork from the layer
    /// </summary>
    /// <param name="layer">Layer</param>
    /// <returns>UtilityNetwork</returns>
    private static UtilityNetwork GetUtilityNetworkFromLayer(Layer layer)
    {
      if (layer == null)
        return null;

      if (layer is UtilityNetworkLayer)
      {
        UtilityNetworkLayer utilityNetworkLayer = layer as UtilityNetworkLayer;
        return utilityNetworkLayer.GetUtilityNetwork();
      }

      else if (layer is SubtypeGroupLayer)
      {
        CompositeLayer compositeLayer = layer as CompositeLayer;
        UtilityNetwork un;

        foreach (var v in compositeLayer.Layers)
        {
          un = GetUtilityNetworkFromLayer(v);
          if (un != null)
            return un;
        }
      }

      else if (layer is FeatureLayer)
      {
        FeatureLayer featureLayer = layer as FeatureLayer;
        using FeatureClass featureClass = featureLayer.GetFeatureClass();
        if (featureClass.IsControllerDatasetSupported())
        {
          IReadOnlyList<Dataset> controllerDatasets = featureClass.GetControllerDatasets();
          foreach (Dataset controllerDataset in controllerDatasets)
          {
            if (controllerDataset is UtilityNetwork)
              return controllerDataset as UtilityNetwork;
          }
        }
      }

      else if (layer is DiagramLayer dl)
      {
        NetworkDiagram diagram = dl.GetNetworkDiagram();
        DiagramManager diagramManager = diagram.DiagramManager;

        return diagramManager.GetNetwork<UtilityNetwork>();

      }
      return null;
    }

    /// <summary>
    /// Get the selected features GUID from the active map
    /// </summary>
    /// <returns>List of GUID</returns>
    internal static List<Guid> GetSelectedGuidFromActiveMap()
    {
      List<Guid> listIds = new List<Guid>();

      Map map = MapView.Active.Map;

      SelectionSet selected = map.GetSelection();

      foreach (var v in selected.ToDictionary())
      {
        if (v.Key.GetType() == typeof(FeatureLayer))
        {
          FeatureLayer fl = v.Key as FeatureLayer;
          Selection sel = fl.GetSelection();

          if (sel.SelectionType == ArcGIS.Core.Data.SelectionType.GlobalID)
          {
            listIds.AddRange(sel.GetGlobalIDs());
          }
          else
            listIds.AddRange(GetGuidFromLayer(fl, sel));
        }
        else if (v.Key.GetType() == typeof(BasicFeatureLayer))
        {
          BasicFeatureLayer fl = v.Key as BasicFeatureLayer;
          Selection sel = fl.GetSelection();

          if (sel.SelectionType == ArcGIS.Core.Data.SelectionType.GlobalID)
          {
            listIds.AddRange(sel.GetGlobalIDs());
          }
          else
            listIds.AddRange(GetGuidFromLayer(fl, sel));
        }
      }

      return listIds;
    }

    /// <summary>
    /// Get the list of selected features GUID from the layer
    /// </summary>
    /// <param name="fl">Layer</param>
    /// <param name="selectedFeatures">Selected features</param>
    /// <returns>List of GUID</returns>
    private static List<Guid> GetGuidFromLayer(BasicFeatureLayer fl, Selection selectedFeatures)
    {
      List<Guid> listIds = new List<Guid>();

      // some data have restriction of element number in a clause IN, so we cut the in smaller list
      List<string> lEid = FormatOidToString(selectedFeatures.GetObjectIDs().ToList());

      TableDefinition tbl = fl.GetTable().GetDefinition();
      string FieldName = tbl.GetObjectIDField();

      QueryFilter qf = new QueryFilter
      {
        SubFields = "*"
      };

      foreach (string se in lEid)
      {
        qf.WhereClause = String.Format("{0} IN ({1})", FieldName, se);

        try
        {
          RowCursor rc = fl.Search(qf);

          while (rc.MoveNext())
            listIds.Add(rc.Current.GetGlobalID());
        }
        catch { }
      }

      return listIds;
    }

    /// <summary>
    /// Format the selected features GUID eo string
    /// </summary>
    /// <param name="selectEID">List of selected EID</param>
    /// <returns>List of string</returns>
    /// <remarks>
    /// Some database have number parameter limitation in where clause IN
    /// </remarks>
    private static List<string> FormatOidToString(List<long> selectEID)
    {
      string s = "";
      int i = 0;
      List<string> lEid = new List<string>();
      foreach (long il in selectEID)
      {
        i++;
        s += string.Format("{0},", il);
        if (i > 999)
        {
          s = s[0..^1];
          lEid.Add(s);
          s = "";
          i = 0;
        }
      }
      if (!string.IsNullOrEmpty(s))
      {
        s = s[0..^1];
        lEid.Add(s);
      }

      return lEid;
    }

  }
}

