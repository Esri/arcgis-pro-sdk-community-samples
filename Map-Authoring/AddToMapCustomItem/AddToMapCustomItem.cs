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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ESRI.ArcGIS.ItemIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AddToMapCustomItem
{
  internal class AddToMapCustomItem : CustomItemBase, IMappableItem
  {
    protected AddToMapCustomItem() : base()
    {
    }

    protected AddToMapCustomItem(ItemInfoValue iiv) : base(FlipBrowseDialogOnly(iiv))
    {
    }

    private static ItemInfoValue FlipBrowseDialogOnly(ItemInfoValue iiv)
    {
      iiv.browseDialogOnly = "FALSE";
      return iiv;
    }
    public static bool OnMainThread => QueuedTask.OnWorker;

    private static bool OnUIThread
    {
      get { return Application.Current.Dispatcher.CheckAccess(); }
    }

    //Gets whether the item can be added to a map of the specified mapType.
    public bool CanAddToMap(MapType? mapType)
    {
      // can only add to 2d maps
      if (mapType == MapType.Map)
        return true;

      return false;
    }
    
    //Add the item to the map.
    //OnAddToMapEx is already on the MCT. 
    List<string> IMappableItem.OnAddToMap(Map map)
    {
      //Converts the custom item file to a feature class in the File GDB
      //The output will not be added to the map
      CSVToPointFC(this.Path); //Feature class created is "AlaskaCitiesPointConv"
      
      //Get the newly added feature class, but first check if it exists
      var gdb = Project.Current.GetItems<GDBProjectItem>().FirstOrDefault().Path;
      string nameFC = "AlaskaCitiesPointConv";
      
      // does the outputfc exist?
      if (FeatureClassExists(gdb, nameFC) == false) return null;
      
      //Create a layer from the feature class
      var itemUri = new Uri($@"{gdb}\{nameFC}");
      var layer = LayerFactory.Instance.CreateLayer(itemUri, MapView.Active.Map);

      List<string> uris = new List<string>();
      if (layer != null)
        uris.Add(layer.URI);
      return uris;
    }
  
    public override ImageSource LargeImage
    {
      get
      {
        var largeImg = new BitmapImage(new Uri(@"pack://application:,,,/AddToMapCustomItem;component/Images/BexDog32.png"));
        return largeImg;
      }
    }

    public override Task<ImageSource> SmallImage
    {
      get
      {
        var smallImage = new BitmapImage(new Uri(@"pack://application:,,,/AddToMapCustomItem;component/Images/BexDog16.png"));
        if (smallImage == null) throw new ArgumentException("SmallImage for CustomItem doesn't exist");
        return Task.FromResult(smallImage as ImageSource);
      }
    }

    public override bool IsContainer => false;

    //TODO: Fetch is required if <b>IsContainer</b> = <b>true</b>
    //public override void Fetch()
    //    {
    //TODO Retrieve your child items
    //TODO child items must also derive from CustomItemBase
    //this.AddRangeToChildren(children);
    //   }
    private async void CSVToPointFC(string path)
    {
      //Get the folder path
      string folderPath = System.IO.Path.GetDirectoryName(path);
      System.Diagnostics.Debug.WriteLine($"folderPath: {folderPath}");

      var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(path);
      System.Diagnostics.Debug.WriteLine($"fileNameWithoutExtension: {fileNameWithoutExtension}");

      //csv
      var csvFileName = $"{fileNameWithoutExtension}.csv";
      var csvFullPath = $@"{System.IO.Path.Combine(folderPath, csvFileName)}";

      //Delete the csv
      System.IO.File.Delete(csvFullPath);// C:\Users\uma2526\Documents\ArcGIS\Projects\CustomItemTest\AlaskaCitiesXY.csv");
      System.Diagnostics.Debug.WriteLine($"Delete: {csvFullPath}");

      //rename .uxh (custom item) to .csv. GP Tool only works with CSV files
      System.IO.File.Copy($@"{path}", csvFullPath, true);
      System.Diagnostics.Debug.WriteLine($"Rename: {csvFullPath}");
      
      //args for GP Tool
      string input_table = @"C:\Users\uma2526\Documents\ArcGIS\Projects\CustomItemTest\AlaskaCitiesXY.csv";
      var gbd = Project.Current.GetItems<GDBProjectItem>().FirstOrDefault().Path;
      string outputFC = $@"{gbd}\AlaskaCitiesPointConv";
      string XField = "POINT_X";
      string YField = "POINT_Y";
      var sr = MapView.Active.Map.SpatialReference;
      
      var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
      var cts = new CancellationTokenSource();

      //Make the GP Tool arg array
      var args = Geoprocessing.MakeValueArray(input_table, outputFC, XField, YField, "", sr);
      //Execute
      _ = await Geoprocessing.ExecuteToolAsync("XYTableToPoint_management", args, environments, cts.Token,
                        (eventName, o) =>
                        {
                          System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                        }, GPExecuteToolFlags.None); //No action is taken, so the new Feature class doesn't get added to the map
    }

    public bool FeatureClassExists(string fileGDBPath, string featureClassName)
    {
      try
      {
        using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(fileGDBPath))))
        {
          FeatureClassDefinition featureClassDefinition = geodatabase.GetDefinition<FeatureClassDefinition>(featureClassName);
          featureClassDefinition.Dispose();
          System.Diagnostics.Debug.WriteLine($"Feature class exists");
          return true;
        }      
      }
      catch
      {
        // GetDefinition throws an exception if the definition doesn't exist
        System.Diagnostics.Debug.WriteLine($"Feature class does not exist");
        return false;
      }
    }


    List<string> IMappableItem.OnAddToMap(Map map, ILayerContainerEdit groupLayer, int index)
    {
      return null;
    }

  }
  internal class ShowItemNameAddToMap : Button
  {
    protected override void OnClick()
    {
      var catalog = Project.GetCatalogPane();
      var items = catalog.SelectedItems;
      var item = items.OfType<AddToMapCustomItem>().FirstOrDefault();
      if (item == null)
        return;
      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Selected Custom Item: {item.Name}");
    }
  }
}
