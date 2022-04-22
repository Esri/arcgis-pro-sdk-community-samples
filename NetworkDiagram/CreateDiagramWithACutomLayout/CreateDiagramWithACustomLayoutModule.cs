using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.NetworkDiagrams;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CreateDiagramWithACustomLayout
{
  internal class CreateDiagramWithACustomLayoutModule : Module
  {
    private static CreateDiagramWithACustomLayoutModule _this = null;
    public const string EnableEnclosure = "CreateDiagramWithACustomLayout_EnclosureState";
    public const string csTemplateName = "Enclosure Diagram";

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static CreateDiagramWithACustomLayoutModule Current
    {
      get
      {
        return _this ?? (_this = (CreateDiagramWithACustomLayoutModule)FrameworkApplication.FindModule("CreateDiagramWithACustomLayout_Module"));
      }
    }


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
    /// <returns>False to prevent ArcGIS Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

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
        using (UtilityNetwork un = GetUtilityNetworkFromActiveMap())
        using (DiagramManager dm = un?.GetDiagramManager())
        {
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
        }
      });
    }

    #endregion Overrides

    /// <summary>
    /// Show a message box with the description of exception
    /// </summary>
    /// <param name="ex">Exception</param>
    internal static void ShowException(Exception ex)
    {
      string s = ExceptionFormat(ex);
      Debug.WriteLine(s);
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
      if (unLayers == null || unLayers.Count() == 0)
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

      Dictionary<MapMember, List<long>> selected = map.GetSelection();

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

      foreach (string se in lEid)
      {
        qf.WhereClause = String.Format("{0} IN ({1})", FieldName, se);

        Debug.WriteLine(qf.WhereClause);
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
          s = s.Substring(0, s.Length - 1);
          lEid.Add(s);
          s = "";
          i = 0;
        }
      }
      if (!String.IsNullOrEmpty(s))
      {
        s = s.Substring(0, s.Length - 1);
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
