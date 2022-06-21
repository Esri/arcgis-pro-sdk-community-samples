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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;
using ESRI.ArcGIS.ItemIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace QuakeItem.Items
{
  /// <summary>
  /// Example custom project item. A custom project item is a custom item which
  /// can be persisted in a project file
  /// </summary>
  /// <remarks>
  /// As a <i>project</i> item, QuakeProjectItems can save state into the aprx. Conversely,
  /// when a project is opened that contains a persisted QuakeProjectItem, QuakeProjectItem
  /// is asked to re-hydrate itself (based on the name, catalogpath, and type that was
  /// saved in the project)</remarks>
  internal class QuakeProjectItem : ArcGIS.Desktop.Core.CustomProjectItemBase
  {

   // private static int event_count;

    protected QuakeProjectItem() : base()
    {
      this._pathSaveRelative = true;
    }
    protected QuakeProjectItem(ItemInfoValue iiv) : base(FlipBrowseDialogOnly(iiv))
    {
      this._pathSaveRelative = true;
    }

    public QuakeProjectItem(string name, string catalogPath, string typeID, string containerTypeID) :
      base(name, catalogPath, typeID, containerTypeID)
    {
      _pathSaveRelative = true;
    }

    /// <summary>
    /// Gets whether the project item can contain child items
    /// </summary>
    public override bool IsContainer => true;

    public override ImageSource LargeImage
    {
      get
      {
        return System.Windows.Application.Current.Resources["BexDog32"] as ImageSource;
      }
    }

    public override Task<ImageSource> SmallImage
    {
      get
      {
        ImageSource img = System.Windows.Application.Current.Resources["BexDog16"] as ImageSource;
        return Task.FromResult(img);
      }
    }

    protected override bool CanRename => true;

    protected override bool OnRename(string newName)
    {
      // have to do some work to actually change the name so if they call refresh it'll be there
      // whether it's a file on disk or node in the xml
      var new_ext = System.IO.Path.GetExtension(newName);
      if (string.IsNullOrEmpty(new_ext))
      {
        new_ext = System.IO.Path.GetExtension(this.Path);
        newName = System.IO.Path.ChangeExtension(newName, new_ext);
      }
        
      var new_file_path = System.IO.Path.Combine(
        System.IO.Path.GetDirectoryName(this.Path), newName);
      System.IO.File.Move(this.Path, new_file_path);
      this.Path = new_file_path;
      return base.OnRename(newName);
    }

    #region Fetch override to provide 'children'

    /// <summary>
    /// Fetch is called if <b>IsContainer</b> = <b>true</b> and the project item is being
    /// expanded in the Catalog pane for the first time.
    /// </summary>
    /// <remarks>The project item should instantiate items for each of its children and
    /// add them to its child collection (see <b>AddRangeToChildren</b>)</remarks>
    public override void Fetch()
    {
      //This is where the quake item is located
      string filePath = this.Path;
      //Quake is an xml document, we parse it's content to provide the list of children
      XDocument doc = XDocument.Load(filePath);

      XNamespace aw = "http://quakeml.org/xmlns/bed/1.2";
      IEnumerable<XElement> earthquakeEvents = from el in doc.Root.Descendants(aw + "event") select el;

      List<QuakeEventCustomItem> events = new List<QuakeEventCustomItem>();
      var existingChildren = this.GetChildren().ToList();
      int event_count = 1;
      //Parse out the child quake events
      foreach (XElement earthquakeElement in earthquakeEvents)
      {
        var path = filePath + $"[{event_count}]";
        XElement desc = earthquakeElement.Element(aw + "description");
        XElement name = desc.Element(aw + "text");
        string fullName = name.Value;
        if (existingChildren.Any(i => i.Path == path)) continue;

        XElement origin = earthquakeElement.Element(aw + "origin");
        XElement time = origin.Element(aw + "time");
        XElement value = time.Element(aw + "value");
        string date = value.Value;
        DateTime timestamp = Convert.ToDateTime(date);
        XElement xLong = origin.Element(aw + "longitude");
        value = xLong.Element(aw + "value");
        var longitude = Convert.ToDouble(value.Value);
        XElement xLat = origin.Element(aw + "latitude");
        value = xLat.Element(aw + "value");
        var latitude = Convert.ToDouble(value.Value);

        //Make an "event" item for each child read from the quake file
        QuakeEventCustomItem item = new QuakeEventCustomItem(
          fullName, path, "acme_quake_event", timestamp.ToString(), longitude, latitude);
        //if (events.Any(s => s.Name == fullName))
        //    continue;
        events.Add(item);
        event_count++;
      }
      //Add the event "child" items to the children collection
      this.AddRangeToChildren(events);
    }

    #endregion Fetch override to provide 'children'


    private static ItemInfoValue FlipBrowseDialogOnly(ItemInfoValue iiv)
    {
      iiv.browseDialogOnly = "FALSE";
      return iiv;
    }

    #region Uniquely identify each custom project item

    public override ProjectItemInfo OnGetInfo()
    {
      var projectItemInfo = new ProjectItemInfo
      {
        Name = this.Name,
        Path = this.Path,
        Type = QuakeProjectItemContainer.ContainerName
      };
      return projectItemInfo;
    }

    #endregion Uniquely identify each custom project item
  }

  /// <summary>
	/// Quake event items. These are children of a QuakeProjectItem
	/// </summary>
	/// <remarks>QuakeEventCustomItems are, themselves, custom items</remarks>
	internal class QuakeEventCustomItem : CustomItemBase, IMappableItem
  {
    public MapPoint QuakeLocation;

    public QuakeEventCustomItem(string name, string path, string type, string lastModifiedTime, double longitude, double latitude) : base(name, path, type, lastModifiedTime)
    {
      this.DisplayType = "QuakeEvent";
      QuakeLocation = MapPointBuilderEx.CreateMapPoint(
        longitude, latitude, SpatialReferences.WGS84);
    }

    public void SetNewName(string newName)
    {
      this.Name = newName;
      NotifyPropertyChanged("Name");
      this.Title = newName;
      NotifyPropertyChanged("Title");
      this._itemInfoValue.name = newName;
      this._itemInfoValue.description = newName;
    }

    public bool CanAddToMap(MapType? mapType)
    {
      if (mapType != null)
      {
        if (mapType != MapType.Map)
          return false;
      }
      return MapView.Active?.Map.GetLayersAsFlattenedList()
        .OfType<GraphicsLayer>().FirstOrDefault() != null;
    }

    public List<string> OnAddToMap(Map map)
    {
      return OnAddToMap(map, null, -1);
    }

    public List<string> OnAddToMap(Map map, ILayerContainerEdit groupLayer, int index)
    {
      var uri = Module1.AddToGraphicsLayer(this);
      return new List<string> { uri };
    }

    public override ImageSource LargeImage
    {
      get
      {
        return System.Windows.Application.Current.Resources["T-Rex32"] as ImageSource;
      }
    }

    public override Task<ImageSource> SmallImage
    {
      get
      {
        ImageSource img = System.Windows.Application.Current.Resources["T-Rex16"] as ImageSource;
        return Task.FromResult(img);
      }
    }
  }
}
