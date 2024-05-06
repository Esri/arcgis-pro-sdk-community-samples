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
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CreateDiagramWithACustomLayout
{
  /// <summary>
  /// With this add-in sample, you will get familiar with the Geometry, NetworkDiagrams and Mapping API methods that allow you to develop your own diagram layout algorithm (see EnclosureLayout source code file) and learn how to extend the Layout gallery with your own custom layout (see config.daml file).  
  /// You will also learn how to develop an add-in command to generate diagrams and execute your own layout to them before their display.  
  /// &gt; NOTE: The EnclosureLayout diagram layout is developed as a sample add-in to use with the particular sample UN dataset provided with it.Moreover, the code sample is very tied to the content of diagrams based on the Enclosure Diagram template.  
  /// Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [Releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page.  
  /// </summary> 
  /// <remarks>
  /// 1. In Visual Studio, click the Build menu.  Then select Build Solution.    
  /// 1. Click Start button to open ArcGIS Pro.  
  /// 1. ArcGIS Pro will open.  
  /// 1. Open **C:\Data\NetworkDiagrams\CreateDiagramWithACustomLayout\CreateDiagramWithACustomLayout.aprx**. Make sure the Communications Network map is open as the active map.add a data reference (what data does the user need to run this sample)  
  /// ### Generate a sample diagram and manually apply the add-in Custom Layout available from the Layout gallery to arrange its contents  
  /// 
  /// 1. Click on the Map tab on the ribbon.Then, in the Navigate group, expand Bookmarks and click **Distr_Cbl_3** in the bookmark list.
  ///
  ///     ![UI](Screenshots/Distr-Cbl-3BookmarkIntheBookmarkList.png)  
  /// 
  /// 1. The map zooms-in on a hub terminator distribution cable component device represented by a salmon circle with 3 dots at its center.  
  /// 1. Use the Selection tool and click this hub terminator device so it becomes selected in the network map.Then, on the Utility Network tab in the ribbon, in the Diagram group, click the drop down arrow under New and click Enclosure Diagram.  
  ///
  ///     ![UI](Screenshots/NewEnclosureDiagram.png)  
  /// 
  /// 1. A diagram based on the Enclosure Diagram template is generated.It opens and displays in a new diagram map.Here under is what you should get after you display the network map and the newly generated diagram map side by side:  
  ///
  ///     ![UI](Screenshots/EnclosureFromTheDistr-Cbl-3Component_NoLayout.png)  
  /// 
  /// 1. For your information, the Enclosure Diagram template is configured to execute a set of diagram rules to build the content of the generated diagram.There are Expand Container rules configured to retrieve communication junction objects that are contents of the input cable component; in particular, Chassis, Connector and Port group.Then, these junction objects are expanded themselves thanks to other Expand Container rules to retrieve some other content junction objects.Among these junction objects, those that are Connector, Port, Port Group or Splice are queried and set as starting points from which a Trace rule is configured to execute a Connected trace ending at the next fiber.Then, any linear containers and orphan junctions are removed from the diagram graph.  
  /// &gt; NOTE: You can have a look to the model builder called Enclosure Diagram in the project toolbox to get the network diagram rules configured for this template in details.  
  /// 1. In the Network Diagram tab in the ribbon, in the Layout gallery, click Custom Telco Layout in the Personal Layouts section:  
  ///
  ///     ![UI](Screenshots/CustomLayoutInGallery.png)  
  ///     
  /// 1. The EnclosureLayout add-in command applies to the diagram content and arranges the fibers that are in and out the **Distr-Cbl-3** cable container.The diagram map is refreshed to reflect the changes:  
  /// 
  ///     ![UI](Screenshots/EnclosureFromTheDistr-Cbl-3Component_AfterCustomLayout.png)  
  ///
  /// 1. Close the diagram map.  
  ///
  /// ### Use the Generate Enclosure add-in command to generate a sample diagram which content is automatically arranged before its open according to the same Custom Telco Layout  
  /// 
  /// 1. Click on the Map tab on the ribbon. Then, in the Navigate group, expand Bookmarks and click any bookmark you want among those highlighted in the image below.  
  ///
  ///     ![UI](Screenshots/Bookmarks.png)  
  /// 
  /// 1. Depending on the clicked bookmark, the map zooms-in on either a hub terminator access cable component device(light pink circle with 3 dots at its center) or a hub terminator distribution cable component device(salmon circle with 3 dots at its center).  
  /// 1. Click the access or distribution cable component device at the center of the network map so it becomes selected.Then, on the Add-In tab in the ribbon, in the Telco group, click Enclosure.  
  /// 1. A diagram based on the Enclosure Diagram template is created in memory.The EnclosureLayout algorithm applies to its in-memory content to arrange the fibers that are in and out this cable component device.Then, a diagram map opens to display this newly generated diagram.  
  /// 1. The graphic below shows the enclosure diagram you obtain when you select the hub terminator cable component device located at the **Distr_Cbl_6** bookmark:  
  ///
  ///     ![UI](Screenshots/EnclosureFromTheDistr-Cbl-6Component.png)  
  /// 
  /// 1. This 2nd graphic below shows the enclosure diagram resulting from the hub terminator access component device located at the **Accss_Cbl_8** bookmark:  
  ///
  ///     ![UI](Screenshots/EnclosureFromTheAccss-Cbl-8Component.png)  
  ///
  /// </remarks>
  internal class CreateDiagramWithACustomLayoutModule : Module
  {
    private static CreateDiagramWithACustomLayoutModule _this = null;
    public const string EnableEnclosure = "CreateDiagramWithACustomLayout_EnclosureState";
    public const string csTemplateName = "Enclosure Diagram";

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>

    public static CreateDiagramWithACustomLayoutModule Current => _this ??= (CreateDiagramWithACustomLayoutModule)FrameworkApplication.FindModule("CreateDiagramWithACustomLayout_Module");

    #region Overrides
    /// <summary>
    /// Initialize Active Map View Changed
    /// </summary>
    /// <returns>boolean</returns>
    protected override bool Initialize()
    {
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      return base.Initialize();
    }

    /// <summary>
    /// Uninitialize Active Map View Changed
    /// </summary>
    protected override void Uninitialize()
    {
      ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
      base.Uninitialize();
    }
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
    /// Set Enclosure flag
    /// </summary>
    /// <param name="obj">ActiveMapViewChangedEventArgs</param>
    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      if (obj == null || obj.IncomingView == null || obj.IncomingView.Map == null)
      {
        FrameworkApplication.State.Deactivate(EnableEnclosure);
        return;
      }

      QueuedTask.Run(() =>
      {
        using UtilityNetwork un = GetUtilityNetworkFromActiveMap();
        using DiagramManager dm = un?.GetDiagramManager();
        try
        {
          DiagramTemplate dt = dm?.GetDiagramTemplate(csTemplateName);
          if (dt == null)
            FrameworkApplication.State.Deactivate(EnableEnclosure);
          else
            FrameworkApplication.State.Activate(EnableEnclosure);
        }
        catch
        {
          FrameworkApplication.State.Deactivate(EnableEnclosure);
        }
      });
    }


    /// <summary>
    /// Show a message box with the description of exception
    /// </summary>
    /// <param name="ex">Exception</param>
    internal static void ShowException(Exception ex)
    {
      string s = ExceptionFormat(ex);
      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(s);
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
          return String.Format("{0}\n{1}", ex.Message, s);
      }

      return ex.Message;
    }

    /// <summary>
    /// Get the UtilityNetwork from active Map
    /// </summary>
    /// <returns>UtilityNetwork</returns>
    internal static UtilityNetwork GetUtilityNetworkFromActiveMap()
    {
      var unLayers = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<UtilityNetworkLayer>();
      if (unLayers == null || !unLayers.Any())
      {
        var dlLayers = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<DiagramLayer>();
        if (dlLayers == null)
          return null;

        foreach (var dlLayer in dlLayers)
        {
          NetworkDiagram diagram = dlLayer.GetNetworkDiagram();
          DiagramManager dm = diagram.DiagramManager;
          UtilityNetwork un = dm.GetNetwork<UtilityNetwork>();
          if (un != null)
            return un;
        }
      }

      foreach (var unLayer in unLayers)
      {
        UtilityNetwork un = unLayer.GetUtilityNetwork();
        if (un != null)
          return un;
      }

      return null;
    }

    /// <summary>
    /// Get Selected Guid From Active Map
    /// </summary>
    /// <returns>Collection of GUID</returns>
    internal static List<Guid> GetSelectedGuidFromActiveMap()
    {
      List<Guid> listIds = new List<Guid>();

      Map map = MapView.Active.Map;

      Dictionary<MapMember, List<long>> selected = map.GetSelection().ToDictionary();

      foreach (var v in selected)
      {
        if (v.Key.GetType() == typeof(FeatureLayer))
        {
          FeatureLayer fl = v.Key as FeatureLayer;
          Selection sel = fl.GetSelection();

          //Table table = fl.GetTable();

          QueryFilter queryFilter = new QueryFilter
          {
            ObjectIDs = sel.GetObjectIDs(),
            SubFields = "*"
          };

          RowCursor cursor = sel.Search(queryFilter);

          while (cursor.MoveNext())
          {
            Row row = cursor.Current;
            listIds.Add(row.GetGlobalID());
          }
        }
        else if (v.Key.GetType() == typeof(BasicFeatureLayer))
        {
          BasicFeatureLayer fl = v.Key as BasicFeatureLayer;
          Selection sel = fl.GetSelection();

          if (sel.SelectionType == SelectionType.GlobalID)
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
    /// Get GUID from Layer
    /// </summary>
    /// <param name="Fl">BasicFeatureLayer</param>
    /// <param name="SelectedFeatures">Selection</param>
    /// <returns></returns>
    private static List<Guid> GetGuidFromLayer(BasicFeatureLayer Fl, Selection SelectedFeatures)
    {
      List<Guid> listIds = new List<Guid>();

      // Some SGDB having limitations on the list size when using WHERE IN clauses, the list is cut in smaller lists
      List<string> lEid = FormatOidToString(SelectedFeatures.GetObjectIDs().ToList());

      TableDefinition tbl = Fl.GetTable().GetDefinition();
      string FieldName = tbl.GetObjectIDField();

      QueryFilter qf = new QueryFilter
      {
        SubFields = "*"
      };

      //List<long> lselected = new List<long>();

      foreach (string se in lEid)
      {
        qf.WhereClause = String.Format("{0} IN ({1})", FieldName, se);

        try
        {
          RowCursor rc = Fl.Search(qf);

          while (rc.MoveNext())
            listIds.Add(rc.Current.GetGlobalID());
        }
        catch { }
      }

      return listIds;
    }

    /// <summary>
    /// Format the list of string for a WHERE IN Clause
    /// </summary>
    /// <param name="selectEID">List of ID</param>
    /// <returns>Collection of string</returns>
    /// <remarks>Some SGDB limits the list number in WHERE IN clause</remarks>
    private static List<string> FormatOidToString(List<long> selectEID)
    {
      string s = "";
      int i = 0;
      List<string> lEid = new List<string>();
      foreach (long il in selectEID)
      {
        i++;
        s += String.Format("{0},", il);
        if (i > 999)
        {
          s = s[0..^1];
          lEid.Add(s);
          s = "";
          i = 0;
        }
      }
      if (!String.IsNullOrEmpty(s))
      {
        s = s[0..^1];
        lEid.Add(s);
      }

      return lEid;
    }

    /// <summary>
    /// Show diagram into a map
    /// </summary>
    /// <param name="Diagram">diagram to show</param>
    internal static async void ShowDiagram(NetworkDiagram Diagram)
    {
      // Create a diagram layer from a NetworkDiagram (myDiagram)
      DiagramLayer diagramLayer = await QueuedTask.Run<DiagramLayer>(() =>
      {

        // Create the diagram map
        var newMap = MapFactory.Instance.CreateMap(Diagram.Name, ArcGIS.Core.CIM.MapType.NetworkDiagram, MapViewingMode.Map);
        if (newMap == null)
          return null;

        // Open the diagram map
        var mapPane = ArcGIS.Desktop.Core.ProApp.Panes.CreateMapPaneAsync(newMap, MapViewingMode.Map);
        if (mapPane == null)
          return null;

        //Add the diagram to the map
        return newMap.AddDiagramLayer(Diagram);
      });
    }

    private BitmapImage _img = null;
    public ImageSource Image
    {
      get
      {
        if (_img == null)
          _img = new BitmapImage(new Uri("pack://application:,,,/CreateDiagramWithACustomLayoutModule;component/images/CustomLayout32.png", UriKind.Absolute));

        return _img;
      }
    }
  }
}

