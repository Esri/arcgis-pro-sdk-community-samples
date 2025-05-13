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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenChartPane
{
  internal class CreateLayout : Button
  {
    protected override void OnClick()
    {
      try
      {
        var defaultPath = CoreModule.CurrentProject.HomeFolderPath;
        var layoutFilePath = Path.Combine(defaultPath, "Visitors.pagx");
        QueuedTask.Run(() =>
        {
          IProjectItem pagx = ItemFactory.Instance.Create(layoutFilePath) as IProjectItem;
          Project.Current.AddItem(pagx);

          // Reference and load the layout associated with the layout item
          LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>()
                         .FirstOrDefault();
          // Reference and load the layout associated with the layout item
          Layout layout = layoutItem.GetLayout();
          if (layout != null)
          {
            var mapFrame = layout.FindElement("Map Frame") as MapFrame;
            MapProjectItem mapPrjItem = Project.Current.GetItems<MapProjectItem>()
                       .FirstOrDefault(item => item.Name.Equals("USNationalParks"));
            if (mapPrjItem != null)
            {
              var activeMap = mapPrjItem.GetMap();
              mapFrame.SetMap(activeMap);
              var parksLayer = activeMap.GetLayersAsFlattenedList().FirstOrDefault((lyr) => lyr.Name == "NationalParks");
              if (parksLayer != null)
              {
                //mapFrame.SetCamera(parksLayer, true);
                MapView.Active.ZoomToSelected();
              }
            }
            System.Diagnostics.Trace.WriteLine($@"{mapFrame.GetType()}");
            var chartFrame = layout.FindElement("Chart Frame") as ArcGIS.Desktop.Layouts.ChartFrame;
            System.Diagnostics.Trace.WriteLine($@"{chartFrame.GetType()}");
          }
          });
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception: {ex}");
      }
    }
  }
}
