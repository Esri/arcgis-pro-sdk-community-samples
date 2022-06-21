/*

   Copyright 2017 Esri

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
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ESRI.ArcGIS.ItemIndex;
using ProSqlExpressDb;
using System.Runtime.InteropServices;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.DragDrop;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Core.Data;

namespace ProSqlExpressReader
{

  internal class ProDataProjectItem : CustomProjectItemBase, IMappableItem
  {
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [DllImport("kernel32.dll")]

    internal static extern uint GetCurrentThreadId();

    List<ProSqlExpressDb.ProSqlExpressDb> _ProSqlExpressDbs = new List<ProSqlExpressDb.ProSqlExpressDb>();

    protected ProDataProjectItem() : base()
    {
    }

    protected ProDataProjectItem(ItemInfoValue iiv) : base(FlipBrowseDialogOnly(iiv))
    {

    }

    private static ItemInfoValue FlipBrowseDialogOnly(ItemInfoValue iiv)
    {
      iiv.browseDialogOnly = "FALSE";
      return iiv;
    }

    /// <summary>
    /// DTor
    /// </summary>
    ~ProDataProjectItem()
    {
    }

    public override ImageSource LargeImage
    {
      get
      {
        var largeImg = new BitmapImage(new Uri(@"pack://application:,,,/ProSqlExpressReader;component/Images/ZipDetail32.png")) as ImageSource;
        return largeImg;
      }
    }

    public override Task<ImageSource> SmallImage
    {
      get
      {
        var smallImage = new BitmapImage(new Uri(@"pack://application:,,,/ProSqlExpressReader;component/Images/ZipDetail16.png")) as ImageSource;
        if (smallImage == null) throw new ArgumentException("SmallImage for CustomProjectItem doesn't exist");
        return Task.FromResult(smallImage as ImageSource);
      }
    }
    public override ProjectItemInfo OnGetInfo()
    {
      var projectItemInfo = new ProjectItemInfo
      {
        Name = this.Name,
        Path = this.Path,
        Type = ProDataProjectItemContainer.ContainerName
      };
      return projectItemInfo;
    }

    public override bool IsContainer => true;

    //TODO: Fetch is required if <b>IsContainer</b> = <b>true</b>
    public override void Fetch()
    {
      // Retrieve your child items
      // child items must also derive from CustomItemBase
      // the sqlexpress file contains one or more lines of SQLExpress connection strings
      // each connection string represents a database
      // don't refresh if this list is already primed before
      if (this.HasChildren) return;
      var children = new List<ProDataSubItem>();
      var sqlConnections = System.IO.File.ReadAllLines(this.Path);
      foreach (var sqlConnection in sqlConnections)
      {
        if (string.IsNullOrEmpty(sqlConnection)) continue;
        ProSqlExpressDb.ProSqlExpressDb sqlDb = null;
        var dbChildren = new List<ProDataSubItem>();
        var nodeName = string.Empty;
        try
        {
          sqlDb = new ProSqlExpressDb.ProSqlExpressDb(sqlConnection);
          nodeName = sqlDb.DatabaseName;
          _ProSqlExpressDbs.Add(sqlDb);
          // child items must also derive from CustomItemBase
          var lstTbl = sqlDb.GetSpatialTables();
          ProDataSubItem featDataset = null;
          foreach (var tableInfo in lstTbl)
          {
            // the path has to be 'unique' for each entry otherwise the UI
            // will not treat the enumeration as a real enumeration
            var uniqueDbPath = $@"{this.Path}|{sqlConnection}|{tableInfo.TableName}";
            var str = featDataset == null ? "-" : featDataset.Name;
            if (str != tableInfo.FeatureDataset)
            {
              featDataset = !string.IsNullOrEmpty(tableInfo.FeatureDataset)
                ? new ProDataSubItem(tableInfo.FeatureDataset, $@"{this.Path}|{sqlConnection}|{tableInfo.FeatureDataset}",
                      this.TypeID, tableInfo,
                      ProDataSubItem.EnumSubItemType.DataSet)
                : null;
              if (featDataset != null) dbChildren.Add(featDataset);
            }
            var dbChild = new ProDataSubItem(tableInfo.TableName, uniqueDbPath, this.TypeID,
                        tableInfo,
                        ProDataSubItem.EnumSubItemType.SqlType);
            if (featDataset != null)
              featDataset.AddChild(dbChild);
            else
              dbChildren.Add(dbChild);
          }
        }
        catch (Exception ex)
        {
          sqlDb = null;
          throw new Exception($@"Problem while initializing database connection.  Error: {ex.Message}");
        }
        if (dbChildren.Count() == 0) break;
        var uniquePath = $@"{this.Path}|{sqlConnection}|";
        var child = new ProDataSubItem(nodeName, uniquePath, this.TypeID,
          null, ProDataSubItem.EnumSubItemType.SqlType, dbChildren);
        children.Add(child);
      }
      this.AddRangeToChildren(children);
    }

    /// <summary>
    /// subItem has a path that needs to be parsed (from the back) in order to build the 
    /// directory tree.  Only the top node of the directory tree (at topNodeName) will be
    /// returned and then added to the root.
    /// </summary>
    /// <param name="topNodeName">top directory where all data is located</param>
    /// <param name="subItem">Item to be inserted at the end of the branch</param>
    /// <returns>top node that needs to be added to the root 'data folder'</returns>
    private ProDataSubItem GetParentFolder(string topNodeName, ProDataSubItem subItem)
    {
      var parts = topNodeName.Split(new char[] { '/', '\\' });
      var rootPartsCnt = parts.Length;
      parts = subItem.Path.Split(new char[] { '/', '\\' });
      var topNode = subItem;
      for (int idx = parts.Length - 2; idx >= rootPartsCnt; idx--)
      {
        var completeFolderPath = string.Empty;
        for (int iidx = 0; iidx <= idx; iidx++)
        {
          if (iidx > 0) completeFolderPath += @"\";
          completeFolderPath += parts[iidx];
        }
        var uniquePath = System.IO.Path.Combine(Path, completeFolderPath);
        topNode = new ProDataSubItem(parts[idx], uniquePath, this.TypeID,
          null, ProDataSubItem.EnumSubItemType.DirType, new List<ProDataSubItem> { topNode });
      }
      return topNode;
    }

    bool IMappableItem.CanAddToMap(MapType? mapType)
    {
      return false;
    }

    public List<string> OnAddToMap(Map map)
    {
      throw new NotImplementedException();
    }

    public List<string> OnAddToMap(Map map, ILayerContainerEdit groupLayer, int index)
    {
      throw new NotImplementedException();
    }

    private static readonly ASCIIEncoding ASCIIencoding = new ASCIIEncoding();

  }


  internal class ProDataSubItem : CustomItemBase, IMappableItem
  {
    public enum EnumSubItemType
    {
      DirType = 0,
      DataSet = 1,
      SqlType = 3
    }

    public ProDataSubItem(string name, string catalogPath,
               string typeID, object specialization,
               EnumSubItemType zipSubItemType,
               List<ProDataSubItem> children = null) :
      base(System.IO.Path.GetFileNameWithoutExtension(name), catalogPath, typeID, DateTime.Now.ToString())
    {
      ComboPath = name;
      SubItemType = zipSubItemType;
      this.DisplayType = "Classic Personal GIS Data";
      this.ContextMenuID = "ProDataSubItem_ContextMenu";
      if (children != null)
      {
        this.AddRangeToChildren(children);
      }
      Specialization = specialization;
    }

    public void AddChild(ProDataSubItem child)
    {
      this.AddRangeToChildren(new List<ProDataSubItem>() { child });
    }

    public override bool IsContainer => GetChildren().Count() > 0;

    public string ComboPath { get; set; }

    public object Specialization { get; set; }

    public EnumSubItemType SubItemType { get; set; }

    public override ImageSource LargeImage
    {
      get
      {
        var imgSrc = GetIconImage(false);
        return imgSrc;
      }
    }

    public override Task<ImageSource> SmallImage
    {
      get
      {
        var imgSrc = GetIconImage(true);
        return Task.FromResult(imgSrc);
      }
    }

    private ImageSource GetIconImage(bool bSmall)
    {
      var size = bSmall ? "16" : "32";
      var imgSrc = System.Windows.Application.Current.Resources[$@"T-Rex{size}"] as ImageSource;
      switch (SubItemType)
      {
        case EnumSubItemType.DirType:
          imgSrc = System.Windows.Application.Current.Resources[$@"Folder{size}"] as ImageSource;
          break;
        case EnumSubItemType.DataSet:
          imgSrc = new BitmapImage(new Uri($@"pack://application:,,,/ProSqlExpressReader;component/Images/GeodatabaseFeatureDataset{size}.png"));
          break;
        case EnumSubItemType.SqlType:
          {
            var fcType = 0;
            var ProSqlExpressTableInfo = Specialization as ProSqlExpressTableInfo;
            if (ProSqlExpressTableInfo != null) fcType = ProSqlExpressTableInfo.GeometryType;
            switch (fcType)
            {
              case 10:
                imgSrc = new BitmapImage(new Uri($@"pack://application:,,,/ProSqlExpressReader;component/Images/TableStandalone{size}.png"));
                break;
              case 1:
                imgSrc = System.Windows.Application.Current.Resources[$@"GeodatabaseFeatureClassPoint{size}"] as ImageSource;
                break;
              case 2:
                imgSrc = System.Windows.Application.Current.Resources[$@"GeodatabaseFeatureClassMultipoint{size}"] as ImageSource;
                break;
              case 3:
                imgSrc = System.Windows.Application.Current.Resources[$@"GeodatabaseFeatureClassLine{size}"] as ImageSource;
                break;
              case 4:
                imgSrc = System.Windows.Application.Current.Resources[$@"GeodatabaseFeatureClassPolygon{size}"] as ImageSource;
                break;
              default:
                imgSrc = new BitmapImage(new Uri($@"pack://application:,,,/ProSqlExpressReader;component/Images/Sql{size}.png"));
                break;
            }
          }
          break;
        default:
          imgSrc = System.Windows.Application.Current.Resources[$@"T-Rex{size}"] as ImageSource;
          break;
      }
      if (imgSrc == null) throw new ArgumentException($@"Unable to find small image for ProSqlTable");
      return imgSrc;
    }

    bool IMappableItem.CanAddToMap(MapType? mapType)
    {
      System.Diagnostics.Debug.WriteLine("CanAddToMap");
      return true;
    }

    public List<string> OnAddToMap(Map map)
    {
      List<ProDataSubItem> proDataSubItems = new()
      {
        this
      };
      return AddProDataSubItemsAsync(proDataSubItems, map);
    }

    public List<string> OnAddToMap(Map map, ILayerContainerEdit groupLayer, int lyrIdx)
    {
      List<ProDataSubItem> proDataSubItems = new()
      {
        this
      };
      return AddProDataSubItemsAsync(proDataSubItems, map, groupLayer, lyrIdx);
    }

    public static List<string> AddProDataSubItemsAsync(List<ProDataSubItem> proDataSubItems, Map mapToAddTo, ILayerContainerEdit grpLyr = null, int lyrIdx = -1)
    {
      List<string> result = new();
      try
      {
        {
          foreach (var proDataSubItem in proDataSubItems)
          {
            switch (proDataSubItem.SubItemType)
            {
              case ProDataSubItem.EnumSubItemType.DirType:
                break;
              case ProDataSubItem.EnumSubItemType.DataSet:
                var children = proDataSubItem.GetChildren().OfType<ProDataSubItem>().ToList();
                result.AddRange(AddProDataSubItemsAsync(children, mapToAddTo, grpLyr, lyrIdx));
                break;
              case ProDataSubItem.EnumSubItemType.SqlType:
                // path is comprised for sql DB path followed by '|' and the table name
                var parts = proDataSubItem.Path.Split('|');
                if (parts.Length < 2)
                {
                  throw new Exception($@"proDataSubproDataSubItem path can't be parsed: {proDataSubItem.Path}");
                }
                var sqlPath = parts[0];
                var sqlConStr = parts[1];
                var tableName = string.Empty;
                if (parts.Length == 3) tableName = parts[2];

                var conSql = new PluginDatasourceConnectionPath("ProSqlExpressPluginDatasource",
                        new Uri(proDataSubItem.Path.Replace(";", "||"), UriKind.Absolute));
                using (var pluginSql = new PluginDatastore(conSql))
                {
                  var tableNames = new List<string>();
                  if (string.IsNullOrEmpty(tableName))
                  {
                    tableNames = new List<string>(pluginSql.GetTableNames());
                  }
                  else tableNames.Add(tableName);
                  foreach (var tn in tableNames)
                  {
                    System.Diagnostics.Debug.Write($"Open table: {tn}\r\n");
                    //open the table
                    using (var table = pluginSql.OpenTable(tn))
                    {
                      if (table is FeatureClass fc)
                      {
                        if (grpLyr != null)
                          //Add as a layer to the active map or scene
                          LayerFactory.Instance.CreateLayer<FeatureLayer>(new FeatureLayerCreationParams(fc) { MapMemberIndex = lyrIdx }, grpLyr);
                        else
                          //Add as a layer to the active map or scene

                          LayerFactory.Instance.CreateLayer<FeatureLayer>(new FeatureLayerCreationParams(fc), mapToAddTo);
                      }
                      else
                      {
                        //add as a standalone table
                        StandaloneTableFactory.Instance.CreateStandaloneTable(new StandaloneTableCreationParams(table), mapToAddTo);
                      }
                    }
                  }
                }
                result.Add(proDataSubItem.Path);
                break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception($@"Unable to add to map: {ex.Message}");
      }
      return result;
    }
  }
}
