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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenChartPane
{
  internal class GenerateLayout : Button
  {
    protected async override void OnClick()
    {
      // find a mapview by its Caption using the FrameworkApplication Panes collection
      var mapPaneCaption = "USNationalParks";
      var mapViewPane = FrameworkApplication.Panes.OfType<IMapPane>().FirstOrDefault((p) => p.Caption == mapPaneCaption);
      if (mapViewPane != null)
      {
        // activate the MapPane
        (mapViewPane as Pane).Activate();
        var mapView = mapViewPane.MapView;
        if (mapView != null)
        {
          // get the layers selected in the map's TOC
          var selectedLayers = mapView.GetSelectedLayers();
        }
      }

      //// Zoom the active map to the selected features of the NationalParks layer
      //_ = QueuedTask.Run(() =>
      //{
      //  MapProjectItem mapPrjItem = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("USNationalParks"));
      //  if (mapPrjItem != null)
      //  {
      //    var parksLayer = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault((lyr) => lyr.Name == "NationalParks");
      //    if (parksLayer != null)
      //    {
      //      mapPrjItem.GetMap().
      //      MapView.Active.ZoomToSelected();
      //    }
      //  }
      //});
      // Get layoutTemplatePath from the project's home folder with a file name of "VisitorsLayout.pagx"
      var projectPath = CoreModule.CurrentProject.HomeFolderPath;
      var layoutFilePath = System.IO.Path.Combine(projectPath, "VisitorsLayout.pagx");
      // Create a new layout project item with the layout file path
      await QueuedTask.Run(() =>
      {
        IProjectItem pagx = ItemFactory.Instance.Create(layoutFilePath) as IProjectItem;
        Project.Current.AddItem(pagx);
      });
      // Get the layout project item named 'Visitors' and it's layout set the mapframe element's
      // Map called "Map Frame" to the MapProjectItem called "USNationalParks"'s map
      var layoutName = "Visitors";
      Layout layout = await QueuedTask.Run<Layout>(() =>
      {
        LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>()
        .FirstOrDefault((lpi) => lpi.Name == layoutName);
        return layoutItem.GetLayout();
      });

      var mapFrameElementName = "Map Frame";
      var mapProjItemName = "USNationalParks";
      LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>()
        .FirstOrDefault((lpi) => lpi.Name == layoutName);
      await QueuedTask.Run(() =>
      {
        // Find the MapFrame in the given Layout using mapFrameElementName
        var mapFrame = layoutItem.GetLayout().FindElement(mapFrameElementName) as MapFrame;
        // get the Map from a MapProjectItem by its Name using mapProjItemName
        MapProjectItem mapPrjItem = Project.Current.GetItems<MapProjectItem>()
        .FirstOrDefault(item => item.Name.Equals(mapProjItemName));
        if (mapPrjItem != null)
        {
          var activeMap = mapPrjItem.GetMap();
          // this is usually required after a layout has been loaded from a layout template
          mapFrame.SetMap(activeMap);
        }
      });
    }
  }
}
