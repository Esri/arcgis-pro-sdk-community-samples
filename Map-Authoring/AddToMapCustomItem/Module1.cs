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
using System.Windows.Input;

namespace AddToMapCustomItem
{
  /// <summary>
  /// This sample illustrates how to add a custom item to the map using the IMappable and IMappebleEX Interfaces.  Item content can be added to ArcGIS Pro using the following application workflows:
  /// * "Add Data" dialog
  /// * "Add Data from Path" dialog
  /// * drag/drop of the item from the catalog window to a map
  /// * drag/drop of the item from the catalog window to the TOC
  /// * drag/drop of the item from windows explorer to a map
  /// * drag/drop of the item from windows explorer to the TOC
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in C:\Data
  /// 1. The data used in this sample is located in this folder 'C:\Data\AddToMapCustomItem'
  /// 1. In Visual Studio, build the solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. In Pro, start with a new map.  
  /// 1. Activate the map so that there is an active map view.
  /// 1. In the Catalog pane, create a new Folder Connection to the 'C:\Data\AddToMapCustomItem' folder.
  /// 1. Notice the custom item in this folder called AlaskaCitiesXY.uxh. This file contains the coordinates of cities in Alaska.
  /// [UI](Screenshots/customItem.png)
  /// 1. This custom item is defined in the solution in the "AddToMapCustomItem.cs" class file. Notice that the AddToMapCustomItem class implements the IMappable and IMappableEx interfaces.
  /// 1. Place a breakpoint in the beginning of the `public string[] OnAddToMapEx(Map map)` method.
  /// 1. Drag and drop the custom item (AlaskaCitiesXY.uxh) from the Catalog pane in Pro to the active Map view or to the TOC of the map view. This action will trigger the OnAddToMapEx callback and your breakpoint will be hit.
  /// 1. 'CSVToPointFC' method converts the `AlaskaCitiesXY.uxh` file into a feature class in the project's file geodatabase. The geoprocessing tool `XYTableToPoint_management` is used to accomplish this.
  /// 1. `LayerFactory.Instance.CreateLayer` method is then invoked to add this feature class to the active map view, using the Uri of the feature class in the File Geodatabase.
  /// [UI](Screenshots/CreateLayer.png) 
  /// 1. Alternatively, you can click the Add Data button on Pro's Map tab on the ribbon. Browse to the 'C:\Data\AddToMapCustomItem' in the Add data dialog that opens up.
  /// 1. Select the AlaskaCitiesXY.uxh item located in this folder and click OK. The feature class will be added to the map view.
  /// [UI](Screenshots/AddToMap.png)    
  /// 1. This action will also invoke the OnAddToMapEx callback method. The Geoprocessing tool converts AlaskaCitiesXY.uxh custom item to a feature class and it will be added to the active map view.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("AddToMapCustomItem_Module"));
      }
    }


    public Item CustomItem
    {
      get
      {
        var catalog = Project.GetCatalogPane();
        var items = catalog.SelectedItems;
        var uxhItem = items.FirstOrDefault();
        if (uxhItem == null)
          return null;

        var isUXHItem = ItemFactory.Instance.IsCustomItem(uxhItem);

        var uxhItemPath = uxhItem.Path;
        Item customItem = null;
        if (isUXHItem)
        {
          // create the item
          customItem = ItemFactory.Instance.Create(uxhItemPath);
        }
        return customItem;
      }
    }

    public string CreateLayerMode
    {
      get; set;
    }
    
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

  }
}
