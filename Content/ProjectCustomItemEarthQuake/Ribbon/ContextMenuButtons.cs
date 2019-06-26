/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using ArcGIS.Desktop.Mapping;
using ProjectCustomItemEarthQuake.Items;

namespace ProjectCustomItemEarthQuake.Ribbon
{
  internal class AddQuakeToProject : Button
  {
    protected override void OnClick()
    {
      var catalog = Project.GetCatalogPane();
      var items = catalog.SelectedItems;
      var item = items.OfType<QuakeProjectItem>().FirstOrDefault();
      if (item == null)
        return;
      QueuedTask.Run(() => Project.Current.AddItem(item));

    }
  }

  internal class DelQuakeFromProject : Button
  {
    protected override void OnClick()
    {
      var catalog = Project.GetCatalogPane();
      var items = catalog.SelectedItems;
      var item = items.OfType<QuakeProjectItem>().FirstOrDefault();
      if (item == null)
        return;
      QueuedTask.Run(() => Project.Current.RemoveItem(item));

    }
  }

  internal class RenameItem : Button
  {
    protected override void OnClick()
    {
      var catalog = Project.GetCatalogPane();
      var items = catalog.SelectedItems;
      var item = items.OfType<QuakeEventCustomItem>().FirstOrDefault();
      if (item == null)
        return;
      item.SetNewName("Hello Test!");
    }
  }

	internal class ShowCim : Button
	{
		protected override void OnClick()
		{
			var catalog = Project.GetCatalogPane();
			var items = catalog.SelectedItems;
			var item = items.OfType<QuakeProjectItem>().FirstOrDefault();
			if (item == null)
				return;
			System.Diagnostics.Debug.WriteLine (item.GetXml());
			//System.IO.File.WriteAllText(@"c:\data\CimProjectItem.json", item.ToJson());
		}
	}
}
