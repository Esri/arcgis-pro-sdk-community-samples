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

namespace ImportLayoutTemplate
{
  internal class ImportLayout : Button
  {
    protected async override void OnClick()
    {
      try
      {
        // Create a Project Item from a layout template pagx file
        // Use the Project's HomeFolder for pagxFilePath
        string homeFolder = Project.Current.HomeFolderPath;
        string pagxFilePath = Path.Combine(homeFolder, "LayoutTemplate.pagx");

        await QueuedTask.Run(() =>
        {
          IProjectItem layoutItem = ItemFactory.Instance.Create(pagxFilePath) as IProjectItem;
          if (layoutItem != null)
          {
            Project.Current.AddItem(layoutItem);
          }

          // Find the layout named "Visitors"
          LayoutProjectItem visitorsLayoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals("Visitors"));
          if (visitorsLayoutItem != null)
          {
            Layout visitorsLayout = visitorsLayoutItem.GetLayout();
            // Find the map frame named "Map Frame" in the layout
            MapFrame mapFrame = visitorsLayout.FindElement("Map Frame") as MapFrame;
            // set the map frame's map to the first map in the project
            // set the map frame's map to the first map name "USNationalParks" in the project
            // set the map frame's map to the first map name "USNationalParks" in the project
            if (mapFrame != null)
            {
              Map map = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("USNationalParks"))?.GetMap();
              if (map != null)
              {
                mapFrame.SetMap(map);
              }
            }

            if (mapFrame != null)
            {
              Map map = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("Map"))?.GetMap();
              if (map != null)
              {
                mapFrame.SetMap(map);
              }
            }
          }
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.Message}");
      }
    }
  }
}
