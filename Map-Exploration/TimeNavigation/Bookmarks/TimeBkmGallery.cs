//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 


using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;

namespace TimeNavigation
{
  internal class TimeBkmGalleryItem : GalleryItem
  {
    public TimeBkmGalleryItem(Bookmark bookmark) :
      base(bookmark.Name, bookmark.Thumbnail, bookmark.Name)
    {
      Bookmark = bookmark;
    }

    public Bookmark Bookmark { get; private set; }
  }

  internal class TimeBkmGallery : Gallery
  {
    protected override void OnDropDownOpened()
    {
      PopulateGallery();
    }

    private void PopulateGallery()
    {
      this.LoadingMessage = "Loading...";
      this.HasFailedToLoadItems = false;

      Clear();

      foreach (var bookmark in TimeModule.Bookmarks)
      {
        this.Add(new TimeBkmGalleryItem(bookmark));
      }

      if (this.ItemCollection.Count == 0)
      {
        this.LoadingMessage = "No time enabled bookmarks available.";
        this.HasFailedToLoadItems = true;
      }        
    }

    protected override void OnClick(GalleryItem item)
    {
      var mapview = MapView.Active;
      if (mapview == null)
        return;

      TimeBkmGalleryItem galleryItem = item as TimeBkmGalleryItem;    
      mapview.ZoomToAsync(galleryItem.Bookmark);
    }
  }
}
